using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace VoxelCraft
{
    public static class ChunkOperationDispatcher
    {
        private static readonly SemaphoreSlim jobSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private static readonly Thread[] jobThreads = new Thread[Environment.ProcessorCount - 1];

        private static readonly ConcurrentQueue<(ChunkData chunk, ChunkData[] neighbors)> meshJobs = new ConcurrentQueue<(ChunkData, ChunkData[])>();
        private static readonly ConcurrentQueue<ChunkData> terrainJobs = new ConcurrentQueue<ChunkData>();
        private static readonly ConcurrentQueue<ChunkData> structureJobs = new ConcurrentQueue<ChunkData>();

        private static readonly ConcurrentQueue<Action> actionsWaitingForMainThread = new ConcurrentQueue<Action>();
        private static readonly ConcurrentQueue<ChunkMeshGenerator> meshGeneratorPool = new ConcurrentQueue<ChunkMeshGenerator>();
        private static int activeMeshGenerators = 0;

        private static readonly ConcurrentQueue<string> debugMessages = new ConcurrentQueue<string>();
        private static bool shutdownThreads = false;

        public const int MAX_MESH_GENERATORS = 32;

        static ChunkOperationDispatcher()
        {
            for (int i = 0; i < jobThreads.Length; i++)
            {
                jobThreads[i] = new Thread(ThreadLoop);
                jobThreads[i].Start();
            }
        }

        public static void ShutdownThreads()
        {
            shutdownThreads = true;
            jobSemaphore.Release(Environment.ProcessorCount - 1);
        }

        private static void ThreadLoop()
        {
            while (!shutdownThreads)
            {
                jobSemaphore.Wait();

                if (activeMeshGenerators < MAX_MESH_GENERATORS && meshJobs.TryDequeue(out (ChunkData chunk, ChunkData[] neighbors) meshJob))
                {
                    if (!meshGeneratorPool.TryDequeue(out ChunkMeshGenerator meshGenerator))
                    {
                        meshGenerator = new ChunkMeshGenerator();
                    }

                    activeMeshGenerators++;
                    meshGenerator.RunJob(ref meshJob.chunk, in meshJob.neighbors);

                    actionsWaitingForMainThread.Enqueue(() =>
                    {
                        if (meshJob.chunk.GeneratedMesh != null)
                        {
                            meshGenerator.FinishMeshGeneration(meshJob.chunk.GeneratedMesh);
                        }
                        meshGeneratorPool.Enqueue(meshGenerator);

                        meshJob.chunk.CurrentChunkOperation = ChunkData.ChunkStage.Ready;
                        activeMeshGenerators--;
                    });
                }
                else if (activeMeshGenerators >= MAX_MESH_GENERATORS && meshJobs.Count > 0)
                {
                    jobSemaphore.Release(1);
                }
                else if (terrainJobs.TryDequeue(out ChunkData chunkToGenerateTerrain))
                {
                    ChunkTerrainGenerator.GenerateTerrain(ref chunkToGenerateTerrain);

                    chunkToGenerateTerrain.CurrentChunkOperation = ChunkData.ChunkStage.Terrain_Complete;
                    chunkToGenerateTerrain.IsDirty = true;
                }
                else if (structureJobs.TryDequeue(out ChunkData chunkToGenerateStructure))
                {
                    ChunkStructureGenerator.GenerateStructures(ref chunkToGenerateStructure);
                    chunkToGenerateStructure.IsDirty = true;

                    chunkToGenerateStructure.CurrentChunkOperation = ChunkData.ChunkStage.Ready;
                }
            }
        }

        public static void DispatchMeshGeneration(ref ChunkData chunkData, in ChunkData[] neighbors)
        {
            chunkData.CurrentChunkOperation = ChunkData.ChunkStage.Generating_Mesh;
            chunkData.IsDirty = false;
            meshJobs.Enqueue((chunkData, neighbors));
            jobSemaphore.Release(1);
        }

        public static void DispatchTerrainGeneration(ref ChunkData chunkData)
        {
            chunkData.CurrentChunkOperation = ChunkData.ChunkStage.Generating_Terrain;
            terrainJobs.Enqueue(chunkData);
            jobSemaphore.Release(1);
        }

        public static void DispatchStructureGeneration(ref ChunkData chunkData)
        {
            chunkData.CurrentChunkOperation = ChunkData.ChunkStage.Generating_Structures;
            structureJobs.Enqueue(chunkData);
            jobSemaphore.Release(1);
        }

        public static void RunActionsWaitingForMainThread(long maxTimeInMS = 10)
        {
            while (debugMessages.TryDequeue(out string msg))
            {
                Debug.Log(msg);
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (actionsWaitingForMainThread.TryDequeue(out Action toRun))
            {
                toRun.Invoke();

                if (stopWatch.ElapsedMilliseconds > maxTimeInMS)
                {
                    break;
                }
            }
        }
    }
}
