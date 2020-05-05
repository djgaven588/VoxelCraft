using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Diagnostics;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    internal class EntryPoint
    {
        static void Main(string[] args)
        {
            Debug.Log("Hello World!");

            windowWidth = 640;
            windowHeight = 480;
            aspectRatio = (double)windowWidth / windowHeight;

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)aspectRatio, 0.01f, 1000);

            new WindowHandler(new GameWindowSettings()
            {
                IsMultiThreaded = false,
                RenderFrequency = 60,
                UpdateFrequency = 50
            },
            new NativeWindowSettings()
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 0),
                Title = "Voxel Craft",
                Size = new Vector2i(windowWidth, windowHeight)
            }, OnLoad, OnClosed, OnResize, null, OnRender).Run();
        }

        private static Material material;
        private static Material skyboxMat;

        private static int windowWidth;
        private static int windowHeight;
        private static double aspectRatio;

        private static Matrix4 projectionMatrix;

        private static Quaterniond cameraRot;
        private static Vector3d cameraPos = new Vector3d(0, 0, 5);
        private static ChunkData chunk = new ChunkData(new Coordinate(0));
        private static Material chunkMat;

        //private static World world;

        private static void OnLoad()
        {
            material = new Material(RenderDataHandler.GenerateProgram("./Engine/Rendering/Shaders/vertex.txt", "./Engine/Rendering/Shaders/fragment.txt", StandardMeshVertexData.ShaderAttributes), RenderDataHandler.LoadTexture("./Artwork/Tree.png"));
            skyboxMat = new SkyboxMaterial(RenderDataHandler.GenerateProgram("./Engine/Rendering/Shaders/skyboxVert.txt", "./Engine/Rendering/Shaders/skyboxFrag.txt", PositionOnlyMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadCubeMap(new string[] {
                    "./Artwork/Skybox/right.png", "./Artwork/Skybox/left.png",
                    "./Artwork/Skybox/top.png", "./Artwork/Skybox/bottom.png",
                    "./Artwork/Skybox/front.png", "./Artwork/Skybox/back.png"
                }));

            chunkMat = new Material(RenderDataHandler.GenerateProgram("chunkVertex.txt", "chunkFragment.txt", ChunkBlockVertexData.ShaderAttributes), RenderDataHandler.LoadTextureArray(new string[] { "./Artwork/GrassTop.png", "./Artwork/GrassSide.png", "./Artwork/Dirt.png" }, 16, 16));

            chunk.GeneratedMesh = Mesh.GenerateMesh(ChunkBlockVertexData.Attributes);
            ChunkTerrainGenerator.GenerateTerrain(ref chunk);
            ChunkMeshGenerator generator = new ChunkMeshGenerator();
            generator.RunJob(ref chunk, null);
            
            generator.FinishMeshGeneration(chunk.GeneratedMesh);

            GL.ClearColor(Color4.CornflowerBlue);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.FrontFace(FrontFaceDirection.Cw);
        }

        private static void OnClosed()
        {

        }

        private static void OnResize(ResizeEventArgs e)
        {
            windowWidth = e.Width;
            windowHeight = e.Height;
            aspectRatio = (double)windowWidth / windowHeight;

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)aspectRatio, 0.01f, 1000);
        }

        private static double XRot;
        private static double YRot;
        private static void OnRender(FrameEventArgs args)
        {
            double xAxis = InputManager.IsKeyDown(Key.D) && !InputManager.IsKeyDown(Key.A) ? 1 : !InputManager.IsKeyDown(Key.D) && InputManager.IsKeyDown(Key.A) ? -1 : 0;
            double yAxis = InputManager.IsKeyDown(Key.Space) && !InputManager.IsKeyDown(Key.ShiftLeft) ? 1 : !InputManager.IsKeyDown(Key.Space) && InputManager.IsKeyDown(Key.ShiftLeft) ? -1 : 0;
            double zAxis = -(InputManager.IsKeyDown(Key.W) && !InputManager.IsKeyDown(Key.S) ? 1 : !InputManager.IsKeyDown(Key.W) && InputManager.IsKeyDown(Key.S) ? -1 : 0);

            double yRotation = InputManager.IsKeyDown(Key.E) && !InputManager.IsKeyDown(Key.Q) ? 1 : !InputManager.IsKeyDown(Key.E) && InputManager.IsKeyDown(Key.Q) ? -1 : 0;
            double xRotation = InputManager.IsKeyDown(Key.Z) && !InputManager.IsKeyDown(Key.X) ? 1 : !InputManager.IsKeyDown(Key.Z) && InputManager.IsKeyDown(Key.X) ? -1 : 0;

            XRot += xRotation * args.Time;
            YRot += yRotation * args.Time;

            cameraRot = Quaterniond.FromEulerAngles(0, 0, XRot) * Quaterniond.FromEulerAngles(0, YRot, 0);
            cameraPos += cameraRot.Inverted() * new Vector3d(xAxis, yAxis, zAxis) * args.Time * 4;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 viewMatrix = Mathmatics.CreateViewMatrix(cameraPos, cameraRot);

            Graphics.UpdateCameraMatrix(viewMatrix * projectionMatrix);

            // Quad
            Graphics.QueueDraw(material, PrimitiveMeshes.Quad, Mathmatics.CreateTransformationMatrix(new Vector3d(0, 0, 1.5), Quaterniond.Identity, Vector3d.One));

            Graphics.QueueDraw(chunkMat, chunk.GeneratedMesh, Mathmatics.CreateTransformationMatrix(Vector3d.Zero, Quaterniond.Identity, Vector3d.One));

            Graphics.HandleQueue();

            // Skybox
            Graphics.DrawNow(skyboxMat, PrimitiveMeshes.Skybox, new Matrix4(new Matrix3(viewMatrix)) * projectionMatrix, Matrix4.Identity);
        }
    }
}
