using OpenToolkit.Mathematics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public static class World
    {
        public static ConcurrentDictionary<Coordinate, ChunkData> LoadedChunks = new ConcurrentDictionary<Coordinate, ChunkData>();
        public static Coordinate WorldCenter;
        public static Material ChunkMaterial;
        public static int RenderDistance = 4;

        public static void UpdateWorld()
        {
            WorldCenter = new Coordinate(0);

            ChunkHandler.CheckToUnload(WorldCenter);
            ChunkHandler.CheckToLoadAndUpdate(WorldCenter);
            ChunkHandler.CheckToRender(WorldCenter);

            ChunkOperationDispatcher.RunActionsWaitingForMainThread();
        }

        public static void DeleteChunk(Coordinate chunkLoc)
        {
            LoadedChunks.Remove(chunkLoc, out ChunkData chunk);

            if(chunk != null)
            {
                if(chunk.GeneratedMesh != null)
                {
                    Debug.Log("Deleting");
                    chunk.GeneratedMesh.RemoveMesh();
                }

                chunk.CurrentChunkOperation = ChunkData.ChunkStage.Unloading;
            }
        }

        public static void CreateChunk(Coordinate coordinate)
        {
            if(LoadedChunks.ContainsKey(coordinate) == false)
            {
                LoadedChunks.TryAdd(coordinate, new ChunkData(coordinate) { CurrentChunkOperation = ChunkData.ChunkStage.New });
            }
            else
            {
                throw new System.Exception("Chunk already existed!");
            }
        }

        public static void UpdateChunk(ChunkData chunk)
        {
            switch (chunk.CurrentChunkOperation)
            {
                case ChunkData.ChunkStage.New:
                    ChunkOperationDispatcher.DispatchTerrainGeneration(ref chunk);
                    break;
                case ChunkData.ChunkStage.Terrain_Complete:
                    ChunkOperationDispatcher.DispatchStructureGeneration(ref chunk);
                    break;
                case ChunkData.ChunkStage.Ready:
                case ChunkData.ChunkStage.Generating_Mesh:
                    
                    // This chunk is ready to process updates

                    break;
                default:
                    break;
            }
        }

        public static void RenderChunk(ChunkData chunk)
        {
            if (chunk.GeneratedMesh == null)
            {
                chunk.GeneratedMesh = Mesh.GenerateMesh(ChunkBlockVertexData.Attributes);
            }
            else if (chunk.GeneratedMesh != null && chunk.GeneratedMesh.VertexCount > 0)
            {
                Graphics.QueueDraw(ChunkMaterial, chunk.GeneratedMesh, Mathmatics.CreateTransformationMatrix(chunk.ChunkPosition.ChunkToWorld().ToVector(), Quaterniond.Identity, Vector3d.One));
            }

            if (chunk.IsDirty && chunk.CurrentChunkOperation == ChunkData.ChunkStage.Ready)
            {
                ChunkOperationDispatcher.DispatchMeshGeneration(ref chunk, null);
            }
        }
    }
}
