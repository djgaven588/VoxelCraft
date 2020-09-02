using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using VoxelCraft.Engine.Rendering.Standard;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering;
using VoxelCraft.Rendering.Standard;
using Color4 = OpenTK.Mathematics.Color4;

namespace VoxelCraft
{
    public static class World
    {
        public static ConcurrentDictionary<Coordinate, ChunkData> LoadedChunks = new ConcurrentDictionary<Coordinate, ChunkData>();
        public static Coordinate PlayerChunk;
        
        public static int Crosshair;
        public static Material TestMaterial;
        public static int RenderDistance = 6;
        public static Camera Camera;

        public static int TerrainUpdate = 0;
        public static int StructureUpdate = 0;
        public static int MeshUpdate = 0;

        private static readonly RollingAverageDebug<(int, int, int)> updateDebug = new RollingAverageDebug<(int, int, int)>(60);

        private static DebugUI _debugMenu;
        private static UIImage _crosshair;

        public static void Initialize()
        {
            _debugMenu = new DebugUI();

            Crosshair = RenderDataHandler.LoadTexture("./Artwork/Dirt.png");

            _crosshair = new UIImage(new UIPosition(Vector2.One * 0.5f, Vector2.Zero), Crosshair, Color4.White);

            TestMaterial = new Material(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/vertex.txt", "./Engine/Rendering/Standard/Shaders/fragment.txt", StandardMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadTexture("./Artwork/Dirt.png"));

            Camera = new Camera(0, 80, 0);

            Graphics.UseCamera(Camera);
        }

        public static void BeforeRender(double timeDelta)
        {
            Camera.Update((float)timeDelta);
        }

        public static void UpdateWorld(double timeDelta)
        {
            (bool raycastHit, Coordinate pos, Coordinate face) = BlockRaycast.Raycast(Camera.Position, ForwardVector(Camera.Rotation), 10);

            if (raycastHit)
            {
                // We maybe hit something, we need to check though as we could have hit a missing chunk.
                if(LoadedChunks.TryGetValue(pos.WorldToChunk(), out ChunkData chunk))
                {
                    // We hit something! Let's display the results.
                    //Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix(pos.ToVector() + Vector3.One * 0.5f, Vector3.Zero, Vector3.One));
                    Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix((pos).ToVector() + Vector3.One * 0.5f + face.ToVector() * 0.75f, Vector3.Zero, Vector3.One / 2));

                    if (InputManager.IsMouseNowDown(OpenTK.Windowing.Common.Input.MouseButton.Button1))
                    {
                        chunk.ModifyBlock(pos, BlockDatabase.IDToBlockData[0], true);
                    }
                    else if (InputManager.IsMouseNowDown(OpenTK.Windowing.Common.Input.MouseButton.Button2))
                    {
                        ChunkData toModify = chunk;
                        Coordinate placeChunk = (pos + face).WorldToChunk();
                        Coordinate placePosition = (pos + face).WorldToBlock();

                        if (placeChunk != chunk.ChunkPosition)
                        {
                            LoadedChunks.TryGetValue(placeChunk, out toModify);
                        }
                        
                        toModify?.ModifyBlock(placePosition, BlockDatabase.IDToBlockData[2], false);
                    }
                }
            }

            PlayerChunk = Coordinate.WorldToChunk(Camera.Position);

            ChunkOperationDispatcher.RunActionsWaitingForMainThread();

            ChunkHandler.CheckToUnload(PlayerChunk);
            ChunkHandler.CheckToLoadAndUpdate(PlayerChunk);
            ChunkHandler.CheckToRender(PlayerChunk);

            updateDebug.AddData((TerrainUpdate, StructureUpdate, MeshUpdate));

            (int x, int y, int z)[] debugData = updateDebug.GetData();
            int totalTerrain = 0, totalStructure = 0, totalMesh = 0;
            for (int i = 0; i < debugData.Length; i++)
            {
                totalTerrain += debugData[i].x;
                totalStructure += debugData[i].y;
                totalMesh += debugData[i].z;
            }

            TerrainUpdate = 0;
            StructureUpdate = 0;
            MeshUpdate = 0;

            _debugMenu.AddDebugData($"Chunk Updates (Last Second)\n- Terrain: {totalTerrain}\n- Structure: {totalStructure}\n- Mesh: {totalMesh}");
            _debugMenu.AddDebugData($"Position: {Camera.Position}\nLook Direction: {ForwardVector(Camera.Rotation)}");
            _debugMenu.AddDebugData($"Chunk: {Coordinate.WorldToChunk(Camera.Position)}\nBlock: {Coordinate.WorldToBlock(Camera.Position)}");
            _debugMenu.AddDebugData($"World Block: {new Coordinate(Camera.Position)}");

            if (raycastHit)
            {
                _debugMenu.AddDebugData($"Hit block: {pos}\nSide: {face}");
            }

            _debugMenu.Update(0f);

            _debugMenu.Draw();

            _debugMenu.Clear();

            _crosshair.Draw();

            //Graphics.QueueDraw(Crosshair, PrimitiveMeshes.CenteredQuad, Graphics.GetUIMatrix(new Vector2(0, 0), 25, new UIPosition(Vector2.One * 0.5f, Vector2.Zero)));
        }

        /// <summary>
        /// Marks bordering chunks to the provided position for mesh regen.
        /// </summary>
        /// <param name="chunkPosition"></param>
        /// <param name="blockPosition"></param>
        public static void MarkNearbyChunksForRegen(Coordinate chunkPosition, Coordinate blockPosition)
        {
            static void MarkChunk(Coordinate chunk)
            {
                if (LoadedChunks.TryGetValue(chunk, out ChunkData c))
                {
                    c.MarkForRegen();
                }
            }

            if(blockPosition.X == 0)
            {
                MarkChunk(chunkPosition + Coordinate.Left);
            }
            else if(blockPosition.X == ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                MarkChunk(chunkPosition + Coordinate.Right);
            }

            if (blockPosition.Y == 0)
            {
                MarkChunk(chunkPosition + Coordinate.Down);
            }
            else if (blockPosition.Y == ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                MarkChunk(chunkPosition + Coordinate.Up);
            }

            if (blockPosition.Z == 0)
            {
                MarkChunk(chunkPosition + Coordinate.Backward);
            }
            else if (blockPosition.Z == ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                MarkChunk(chunkPosition + Coordinate.Forward);
            }
        }

        public static Vector3 ForwardVector(Vector3 euler)
        {
            euler.X = Mathmatics.ConvertToRadians(euler.X);
            euler.Y = Mathmatics.ConvertToRadians(euler.Y);
            return new Vector3((float)Math.Sin(euler.Y) * (float)Math.Cos(euler.X), -(float)Math.Sin(euler.X), (float)Math.Cos(euler.Y) * (float)Math.Cos(euler.X));
        }

        public static void DeleteChunk(Coordinate chunkLoc)
        {
            if (LoadedChunks.Remove(chunkLoc, out ChunkData chunk))
            {
                if (chunk.Mesh != null)
                {
                    chunk.Mesh.CleanUp();
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

        public static void UpdateChunk(ChunkData chunk)
        {
            if (chunk.CurrentOperation == ChunkData.ChunkOperation.Unloading)
            {
                Debug.Log("BROKEN CHUNK!");
            }

            switch (chunk.CurrentOperation)
            {
                case ChunkData.ChunkOperation.New:
                    ChunkOperationDispatcher.DispatchTerrainGeneration(ref chunk);
                    TerrainUpdate++;
                    break;
                case ChunkData.ChunkOperation.Terrain_Complete:
                    if (GetSurroundingNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                    {
                        ChunkOperationDispatcher.DispatchStructureGeneration(ref chunk, neighbors);
                        StructureUpdate++;
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
                Graphics.DrawNow(StandardMaterials.ChunkMaterial, chunk.Mesh, Mathmatics.CreateTransformationMatrix(chunk.ChunkPosition.ChunkToWorld().ToVector(), Vector3.Zero, Vector3.One));
            }

            if (chunk.RegenerateMesh && chunk.CurrentOperation == ChunkData.ChunkOperation.Ready)
            {
                if (GetAdjacentNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                {
                    ChunkOperationDispatcher.DispatchMeshGeneration(ref chunk, neighbors);
                    if(chunk.ChunkPosition.Y + 1 == Region.REGION_SIZE)
                    {
                        Debug.Log(chunk.ChunkPosition);
                    }

                    MeshUpdate++;
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

            

            if (location.Y != Region.REGION_SIZE - 1 && (LoadedChunks.TryGetValue(location + new Coordinate(0, 1, 0), out neighbors[4]) == false || !IsNeighborRenderReady(neighbors[4])))
            {
                if (location.Y == Region.REGION_SIZE - 1)
                {
                    return true;
                }
                return false;
            }

            if (location.Y != 0 && (LoadedChunks.TryGetValue(location - new Coordinate(0, 1, 0), out neighbors[5]) == false || !IsNeighborRenderReady(neighbors[5])))
            {
                if (location.Y == 0)
                {
                    return true;
                }
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
