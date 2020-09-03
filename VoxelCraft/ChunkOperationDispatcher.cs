using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public static class ChunkOperationDispatcher
    {
        private static readonly SemaphoreSlim jobSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private static readonly Thread[] jobThreads = new Thread[Environment.ProcessorCount - 1];

        private static readonly ConcurrentQueue<(ChunkData chunk, ChunkData[] neighbors)> meshJobs = new ConcurrentQueue<(ChunkData, ChunkData[])>();
        private static readonly ConcurrentQueue<ChunkData> terrainJobs = new ConcurrentQueue<ChunkData>();
        private static readonly ConcurrentQueue<(ChunkData chunk, ChunkData[] neighbors)> structureJobs = new ConcurrentQueue<(ChunkData, ChunkData[])>();

        private static readonly ConcurrentQueue<Action> actionsWaitingForMainThread = new ConcurrentQueue<Action>();
        private static readonly ConcurrentQueue<ChunkMeshGenerator> meshGeneratorPool = new ConcurrentQueue<ChunkMeshGenerator>();
        private static int activeMeshGenerators = 0;
        private static int activeLightGenerators = 0;

        private static readonly ConcurrentQueue<string> debugMessages = new ConcurrentQueue<string>();
        private static bool shutdownThreads = false;

        public static readonly int MAX_MESH_GENERATORS = Math.Min(Math.Max(Environment.ProcessorCount, 2) * 4, 32);
        public static readonly int MAX_LIGHT_GENERATORS = Math.Min(Math.Max(Environment.ProcessorCount, 2), 32);

        static ChunkOperationDispatcher()
        {
            for (int i = 0; i < jobThreads.Length; i++)
            {
                int copy = i;
                jobThreads[i] = new Thread(() => ThreadLoop(copy));
                jobThreads[i].Start();
            }
        }

        public static void ShutdownThreads()
        {
            shutdownThreads = true;
            jobSemaphore.Release(Environment.ProcessorCount - 1);
        }

        private static void ThreadLoop(int threadNumber)
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
                        if (meshJob.chunk.Mesh != null)
                        {
                            meshGenerator.FinishMeshGeneration(meshJob.chunk.Mesh);
                            
                        }
                        else
                        {
                            Debug.Log("Failed to generate mesh");
                        }

                        meshJob.chunk.CurrentOperation = ChunkData.ChunkOperation.Ready;

                        meshGeneratorPool.Enqueue(meshGenerator);
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

                    chunkToGenerateTerrain.CurrentOperation = ChunkData.ChunkOperation.Terrain_Complete;
                    chunkToGenerateTerrain.RegenerateMesh = true;
                }
                else if (structureJobs.TryDequeue(out (ChunkData chunk, ChunkData[] neighbors) structureJob))
                {
                    ChunkStructureGenerator.GenerateStructures(ref structureJob.chunk, structureJob.neighbors);

                    structureJob.chunk.CurrentOperation = ChunkData.ChunkOperation.Ready;
                }
                else if(activeLightGenerators < MAX_LIGHT_GENERATORS)
                {
                    activeLightGenerators++;
                    WorldLightingGenerator.RunLighting(50000);
                    activeLightGenerators--;
                }
            }
        }

        public static void DispatchMeshGeneration(ref ChunkData chunkData, in ChunkData[] neighbors)
        {
            chunkData.CurrentOperation = ChunkData.ChunkOperation.Generating_Mesh;
            chunkData.RegenerateMesh = false;
            chunkData.LightUpdateRequired = false;
            meshJobs.Enqueue((chunkData, neighbors));
            jobSemaphore.Release(1);
        }

        public static void DispatchTerrainGeneration(ref ChunkData chunkData)
        {
            chunkData.CurrentOperation = ChunkData.ChunkOperation.Generating_Terrain;
            terrainJobs.Enqueue(chunkData);
            jobSemaphore.Release(1);
        }

        public static void DispatchStructureGeneration(ref ChunkData chunkData, in ChunkData[] neighbors)
        {
            chunkData.CurrentOperation = ChunkData.ChunkOperation.Generating_Structures;
            structureJobs.Enqueue((chunkData, neighbors));
            jobSemaphore.Release(1);
        }

        public static void RunActionsWaitingForMainThread(long maxTimeInMS = 8)
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

            // Make sure all jobs are done
            if (jobSemaphore.CurrentCount < jobThreads.Length)
            {
                jobSemaphore.Release(jobThreads.Length);
            }
        }
    }
}
