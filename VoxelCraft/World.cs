﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public static class World
    {
        public static ConcurrentDictionary<Coordinate, ChunkData> LoadedChunks = new ConcurrentDictionary<Coordinate, ChunkData>();
        public static Coordinate WorldCenter;
        public static Material ChunkMaterial;
        public static Material TestMaterial;
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

            TestMaterial = new Material(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Shaders/vertex.txt", "./Engine/Rendering/Shaders/fragment.txt", StandardMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadTexture("./Artwork/Dirt.png"));

            Camera = new Camera(0, 80, 0);

            Graphics.UseCamera(Camera);
        }

        public static void UpdateWorld(double timeDelta)
        {
            Camera.Update((float)timeDelta);
            Vector3 forward = TransformUnitZ(Camera.Rotation);
            BlockRaycast.RaycastData data = BlockRaycast.FindBlock(Camera.Position, forward, 10);
            if (data.HitBlock && data.RayTrapped == false)
            {
                //Debug.Log($"Hit! {data.Chunk} {data.Block} {data.DistanceRemaining} - {(temp * new Vector3(0, 0, 1))}");
                //Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix((data.Chunk.ChunkToWorld() + data.Block).ToVector() + Vector3d.One * 0.5, Quaterniond.Identity, Vector3d.One));
            }
            else
            {
                //Debug.Log($"Nope! {data.HitBlock} {data.RayTrapped} {data.Chunk} {data.Block} {data.DistanceRemaining} - {(Camera.cameraRot * new Vector3d(0, 0, 1)).Normalized()}");
            }

            Debug.Log(forward);

            //Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix(Camera.Position + Camera.Rotation * new Vector3(0, 0, 5), Quaternion.Identity, Vector3d.One));
            WorldCenter = Coordinate.WorldToChunk(Camera.Position);

            ChunkOperationDispatcher.RunActionsWaitingForMainThread();

            ChunkHandler.CheckToUnload(WorldCenter);
            ChunkHandler.CheckToLoadAndUpdate(WorldCenter);
            ChunkHandler.CheckToRender(WorldCenter);
        }

        public static Vector3 TransformUnitZ(in Quaternion rotation)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float yz2 = rotation.Y * z2;
            float wx2 = rotation.W * x2;
            float wy2 = rotation.W * y2;
            return new Vector3(xz2 + wy2, yz2 - wx2, xx2 - yy2);
        }

        public static void DeleteChunk(Coordinate chunkLoc)
        {
            if (LoadedChunks.Remove(chunkLoc, out ChunkData chunk))
            {
                if (chunk.Mesh != null)
                {
                    chunk.Mesh.RemoveMesh();
                    Debug.Log("DELETE MESH CHUNK");
                }

                chunk.CurrentOperation = ChunkData.ChunkOperation.Unloading;
            }
        }

        public static void CreateChunk(Coordinate coordinate)
        {
            if (LoadedChunks.ContainsKey(coordinate) == false)
            {
                if (!LoadedChunks.TryAdd(coordinate, new ChunkData(coordinate) { CurrentOperation = ChunkData.ChunkOperation.New }))
                {
                    Debug.Log("FAILED TO CREATE CHUNK!");
                }
            }
            else
            {
                throw new System.Exception("Chunk already existed!");
            }
        }

        public static void UpdateChunk(ChunkData chunk, int ring)
        {
            if (chunk.CurrentOperation == ChunkData.ChunkOperation.Unloading)
            {
                Debug.Log("BROKEN CHUNK!");
            }

            switch (chunk.CurrentOperation)
            {
                case ChunkData.ChunkOperation.New:
                    ChunkOperationDispatcher.DispatchTerrainGeneration(ref chunk);
                    break;
                case ChunkData.ChunkOperation.Terrain_Complete:
                    if (GetSurroundingNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                    {
                        ChunkOperationDispatcher.DispatchStructureGeneration(ref chunk, neighbors);
                    }
                    break;
                case ChunkData.ChunkOperation.Ready:
                case ChunkData.ChunkOperation.Generating_Mesh:

                    // This chunk is ready to process updates

                    break;
                default:
                    break;
            }
        }

        public static void RenderChunk(ChunkData chunk)
        {
            if (chunk.Mesh == null && chunk.CurrentOperation == ChunkData.ChunkOperation.Ready)
            {
                chunk.Mesh = Mesh.GenerateMesh(ChunkBlockVertexData.Attributes);
            }
            else if (chunk.Mesh != null && chunk.Mesh.VertexCount > 0)
            {
                Graphics.DrawNow(ChunkMaterial, chunk.Mesh, Mathmatics.CreateTransformationMatrix(chunk.ChunkPosition.ChunkToWorld().ToVector(), Quaternion.Identity, Vector3.One));
            }

            if (chunk.RegenerateMesh && chunk.CurrentOperation == ChunkData.ChunkOperation.Ready)
            {
                if (GetAdjacentNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                {
                    ChunkOperationDispatcher.DispatchMeshGeneration(ref chunk, neighbors);
                }
            }
        }

        /// <summary>
        /// Gets all adjacent neighbors (1 distance away).
        /// </summary>
        /// <param name="location">The location to start at</param>
        /// <param name="neighbors">The neighboring chunks returned</param>
        /// <returns>True or false, all neighbors were correctly found.</returns>
        private static bool GetAdjacentNeighbors(Coordinate location, out ChunkData[] neighbors)
        {
            neighbors = new ChunkData[6];

            if (LoadedChunks.TryGetValue(location + new Coordinate(0, 0, 1), out neighbors[0]) == false || !IsNeighborRenderReady(neighbors[0]))
            {
                return false;
            }

            if (LoadedChunks.TryGetValue(location - new Coordinate(0, 0, 1), out neighbors[1]) == false || !IsNeighborRenderReady(neighbors[1]))
            {
                return false;
            }

            if (LoadedChunks.TryGetValue(location + new Coordinate(1, 0, 0), out neighbors[2]) == false || !IsNeighborRenderReady(neighbors[2]))
            {
                return false;
            }

            if (LoadedChunks.TryGetValue(location - new Coordinate(1, 0, 0), out neighbors[3]) == false || !IsNeighborRenderReady(neighbors[3]))
            {
                return false;
            }

            if (location.Y != Region.REGION_SIZE && (LoadedChunks.TryGetValue(location + new Coordinate(0, 1, 0), out neighbors[4]) == false || !IsNeighborRenderReady(neighbors[4])))
            {
                return false;
            }

            if (location.Y != 0 && (LoadedChunks.TryGetValue(location - new Coordinate(0, 1, 0), out neighbors[5]) == false || !IsNeighborRenderReady(neighbors[5])))
            {
                return false;
            }

            return true;
        }

        private static bool IsNeighborRenderReady(ChunkData chunk)
        {
            return chunk.CurrentOperation >= ChunkData.ChunkOperation.Ready;
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
                        if ((location.Y + y >= 0 && location.Y + y < Region.REGION_SIZE) && !(x == 0 && y == 0 && z == 0))
                        {
                            if (LoadedChunks.TryGetValue(location + new Coordinate(x, y, z), out neighbors[index]) == false || !IsNeighborStructureReady(neighbors[index]))
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
            return chunk.CurrentOperation >= ChunkData.ChunkOperation.Terrain_Complete;
        }
    }
}
