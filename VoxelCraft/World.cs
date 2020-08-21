using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using VoxelCraft.Engine.Rendering;
using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering;
using VoxelCraft.Rendering.Standard;

namespace VoxelCraft
{
    public static class World
    {
        public static ConcurrentDictionary<Coordinate, ChunkData> LoadedChunks = new ConcurrentDictionary<Coordinate, ChunkData>();
        public static Coordinate WorldCenter;
        public static Material ChunkMaterial;
        public static UIMaterial WhiteTextMaterial;
        public static UIMaterial BlackTextMaterial;
        public static UIMaterial UIMeshTest;
        public static Material TestMaterial;
        public static int RenderDistance = 5;
        public static Camera Camera;

        public static FontData ArialFont;
        public static Mesh DebugTextMesh = null;

        private static RollingAverageDebug<(int, int, int)> updateDebug = new RollingAverageDebug<(int, int, int)>(60);

        public static int TerrainUpdate = 0;
        public static int StructureUpdate = 0;
        public static int MeshUpdate = 0;

        public static void Initialize()
        {
            ArialFont = new FontData("./Font/Arial.png", "./Font/Arial.fnt", 1024);

            ChunkMaterial = new ChunkMaterial(
                RenderDataHandler.GenerateProgram("chunkVertex.txt", "chunkFragment.txt", ChunkBlockVertexData.ShaderAttributes),
                RenderDataHandler.LoadTextureArray(new string[] {
                    "./Artwork/GrassTop.png", "./Artwork/GrassSide.png",
                    "./Artwork/Dirt.png", "./Artwork/Wood.png",
                    "./Artwork/WoodTop.png", "./Artwork/Leaf.png" }, 16, 16));

            WhiteTextMaterial = new UIMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes),
                ArialFont.TextureId);

            BlackTextMaterial = new UIMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes),
                ArialFont.TextureId);

            WhiteTextMaterial.ChangeColor(OpenToolkit.Mathematics.Color4.White);
            BlackTextMaterial.ChangeColor(OpenToolkit.Mathematics.Color4.Black);

            TestMaterial = new Material(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/vertex.txt", "./Engine/Rendering/Standard/Shaders/fragment.txt", StandardMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadTexture("./Artwork/Dirt.png"));

            UIMeshTest = new UIMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes),
                RenderDataHandler.LoadTexture("./Artwork/Dirt.png"));

            UIMeshTest.ChangeColor(OpenToolkit.Mathematics.Color4.White);

            Camera = new Camera(0, 80, 0);

            Graphics.UseCamera(Camera);
        }

        public static void BeforeRender(double timeDelta)
        {
            Camera.Update((float)timeDelta);
        }

        private static Vector3[] FaceDirections = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0)
        };

        public static void UpdateWorld(double timeDelta)
        {
            (bool found, Coordinate pos, Coordinate face) = BlockRaycast.Raycast(Camera.Position, ForwardVector(Camera.Rotation), 10);

            if (found)
            {
                // We maybe hit something, we need to check though as we could have hit a missing chunk.
                if(LoadedChunks.TryGetValue(pos.WorldToChunk(), out ChunkData chunk))
                {
                    // We hit something! Let's display the results.
                    Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix(pos.ToVector() + Vector3.One * 0.5f, Vector3.Zero, Vector3.One));
                    Graphics.QueueDraw(TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix((pos).ToVector() + Vector3.One * 0.5f + face.ToVector() * 0.75f, Vector3.Zero, Vector3.One / 2));
                }
            }

            WorldCenter = Coordinate.WorldToChunk(Camera.Position);

            ChunkOperationDispatcher.RunActionsWaitingForMainThread();

            ChunkHandler.CheckToUnload(WorldCenter);
            ChunkHandler.CheckToLoadAndUpdate(WorldCenter);
            ChunkHandler.CheckToRender(WorldCenter);

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

            Graphics.QueueDraw(UIMeshTest, PrimitiveMeshes.CenteredQuad, Graphics.GetUIMatrix(new Vector2(0, 0), 25, true));

            string text = $"Chunk Updates (Last Second)\n- Terrain: {totalTerrain}\n- Structure: {totalStructure}\n- Mesh: {totalMesh}\n\n" +
                $"Position: {Camera.Position}\nLook Direction: {ForwardVector(Camera.Rotation)}\n" +
                $"Chunk: {Coordinate.WorldToChunk(Camera.Position)}\nBlock: {Coordinate.WorldToBlock(Camera.Position)}\n" +
                //$"Hit block: {data.HitBlock} - {data.Block} - {data.Chunk}\n" + 
                $"World Block: {new Coordinate(Camera.Position)}";
            DebugTextMesh = TextMeshGenerator.RegenerateMesh(text, ArialFont, 10, 50, OpenToolkit.Mathematics.Color4.Black, 18, 0.9f, DebugTextMesh);
            Graphics.QueueDraw(WhiteTextMaterial, DebugTextMesh, Graphics.GetUIMatrix(Vector2.One * 50, 1));
            Graphics.QueueDraw(BlackTextMaterial, DebugTextMesh, Graphics.GetUIMatrix(Vector2.One * 48, 1));
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
                Graphics.DrawNow(ChunkMaterial, chunk.Mesh, Mathmatics.CreateTransformationMatrix(chunk.ChunkPosition.ChunkToWorld().ToVector(), Vector3.Zero, Vector3.One));
            }

            if (chunk.RegenerateMesh && chunk.CurrentOperation == ChunkData.ChunkOperation.Ready)
            {
                if (GetAdjacentNeighbors(chunk.ChunkPosition, out ChunkData[] neighbors))
                {
                    ChunkOperationDispatcher.DispatchMeshGeneration(ref chunk, neighbors);
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
