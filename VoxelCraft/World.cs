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
        public static int RenderDistance = 5;
        public static Camera Camera;

        public static void Initialize()
        {
            ChunkMaterial = new ChunkMaterial(
                RenderDataHandler.GenerateProgram("chunkVertex.txt", "chunkFragment.txt", ChunkBlockVertexData.ShaderAttributes),
                RenderDataHandler.LoadTextureArray(new string[] { 
                    "./Artwork/GrassTop.png", "./Artwork/GrassSide.png", 
                    "./Artwork/Dirt.png", "./Artwork/Wood.png", 
                    "./Artwork/WoodTop.png", "./Artwork/Leaf.png" }, 16, 16));

            Camera = new Camera(0, 80, 0);

            Graphics.UseCamera(Camera);
        }

        public static void UpdateWorld(double timeDelta)
        {
            Camera.Update(timeDelta);
            WorldCenter = Coordinate.WorldToChunk(Camera.cameraPos);

            ChunkHandler.CheckToUnload(WorldCenter);
            ChunkHandler.CheckToLoadAndUpdate(WorldCenter);
            ChunkHandler.CheckToRender(WorldCenter);

            ChunkOperationDispatcher.RunActionsWaitingForMainThread();
        }

        public static void DeleteChunk(Coordinate chunkLoc)
        {
            LoadedChunks.Remove(chunkLoc, out ChunkData chunk);

            if (chunk != null)
            {
                if (chunk.GeneratedMesh != null)
                {
                    chunk.GeneratedMesh.RemoveMesh();
                }

                chunk.CurrentChunkOperation = ChunkData.ChunkStage.Unloading;
            }
        }

        public static void CreateChunk(Coordinate coordinate)
        {
            if (LoadedChunks.ContainsKey(coordinate) == false)
            {
                LoadedChunks.TryAdd(coordinate, new ChunkData(coordinate) { CurrentChunkOperation = ChunkData.ChunkStage.New });
            }
            else
            {
                throw new System.Exception("Chunk already existed!");
            }
        }

        public static void UpdateChunk(ChunkData chunk, int ring)
        {
            switch (chunk.CurrentChunkOperation)
            {
                case ChunkData.ChunkStage.New:
                    ChunkOperationDispatcher.DispatchTerrainGeneration(ref chunk);
                    break;
                case ChunkData.ChunkStage.Terrain_Complete:
                    if (ring - 1 < ChunkHandler.DISTANCE_FOR_UPDATE && GetSurroundingNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                    {
                        ChunkOperationDispatcher.DispatchStructureGeneration(ref chunk, neighbors);
                    }
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
                Graphics.DrawNow(ChunkMaterial, chunk.GeneratedMesh, Mathmatics.CreateTransformationMatrix(chunk.ChunkPosition.ChunkToWorld().ToVector(), Quaterniond.Identity, Vector3d.One));
            }

            if (chunk.IsDirty && chunk.CurrentChunkOperation == ChunkData.ChunkStage.Ready)
            {
                if (GetAdjacentNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                {
                    ChunkOperationDispatcher.DispatchMeshGeneration(ref chunk, neighbors);
                }
            }
        }

        private static bool GetAdjacentNeighbors(Coordinate location, out ChunkData[] neighbors)
        {
            neighbors = new ChunkData[6];

            if (LoadedChunks.TryGetValue(location + new Coordinate(0, 0, 1), out neighbors[0]) == false || !IsNeighborRenderReady(neighbors[0]))
                return false;

            if (LoadedChunks.TryGetValue(location - new Coordinate(0, 0, 1), out neighbors[1]) == false || !IsNeighborRenderReady(neighbors[1]))
                return false;

            if (LoadedChunks.TryGetValue(location + new Coordinate(1, 0, 0), out neighbors[2]) == false || !IsNeighborRenderReady(neighbors[2]))
                return false;

            if (LoadedChunks.TryGetValue(location - new Coordinate(1, 0, 0), out neighbors[3]) == false || !IsNeighborRenderReady(neighbors[3]))
                return false;

            if (location.Y != Region.REGION_SIZE && (LoadedChunks.TryGetValue(location + new Coordinate(0, 1, 0), out neighbors[4]) == false || !IsNeighborRenderReady(neighbors[4])))
                return false;

            if (location.Y != 0 && (LoadedChunks.TryGetValue(location - new Coordinate(0, 1, 0), out neighbors[5]) == false || !IsNeighborRenderReady(neighbors[5])))
                return false;

            return true;
        }

        private static bool IsNeighborRenderReady(ChunkData chunk)
        {
            return chunk.CurrentChunkOperation >= ChunkData.ChunkStage.Ready;
        }

        private static bool GetSurroundingNeighbors(Coordinate location, out ChunkData[] neighbors)
        {
            neighbors = new ChunkData[27];

            int index = 0;
            for (int z = -1; z <= 1; z++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if((location.Y + y >= 0 && location.Y + y < Region.REGION_SIZE) && !(x == 0 && y == 0 && z == 0))
                        {
                            if(LoadedChunks.TryGetValue(location + new Coordinate(x, y, z), out neighbors[index]) == false || !IsNeighborStructureReady(neighbors[index]))
                            {
                                return false;
                            }
                        }

                        index++;
                    }
                }
            }

            return true;
        }

        private static bool IsNeighborStructureReady(ChunkData chunk)
        {
            return chunk.CurrentChunkOperation >= ChunkData.ChunkStage.Terrain_Complete;
        }
    }
}
