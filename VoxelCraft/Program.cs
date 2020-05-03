using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Drawing.Drawing2D;
using System.IO;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    class Program
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
                IsMultiThreaded = false, RenderFrequency = 60, UpdateFrequency = 50
            },
            new NativeWindowSettings()
            {
                API = ContextAPI.OpenGL, APIVersion = new Version(4, 0), Title = "Voxel Craft", Size = new Vector2i(windowWidth, windowHeight)
            }, OnLoad, null, OnResize, null, OnRender).Run();
        }

        private static Mesh testMesh;
        private static Mesh skyboxMesh;
        private static Material material;
        private static Material skyboxMat;

        private static int windowWidth;
        private static int windowHeight;
        private static double aspectRatio;

        private static Matrix4 projectionMatrix;

        private static void OnLoad()
        {
            testMesh = Mesh.GenerateMesh(0, 0, StandardMeshVertexData.Attributes);

            testMesh.UploadMeshData(new StandardMeshVertexData[] { 
                new StandardMeshVertexData(new Vector3d(-0.25,  0.25, 0), new Vector2d(0, 0)),
                new StandardMeshVertexData(new Vector3d(0.25,   0.25, 0), new Vector2d(1, 0)),
                new StandardMeshVertexData(new Vector3d(0.25,  -0.25, 0), new Vector2d(1, 1)),
                new StandardMeshVertexData(new Vector3d(-0.25, -0.25, 0), new Vector2d(0, 1))
            }, 4, new uint[] { 0, 1, 2, 2, 3, 0 }, 6);

            skyboxMesh = Mesh.GenerateMesh(0, 0, PositionOnlyMeshVertexData.Attributes);

            skyboxMesh.UploadMeshData(new PositionOnlyMeshVertexData[] {
                // Front
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),

                // Back
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, 1)),

                // Left
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, 1)),

                // Right
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, -1)),

                // Top
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, 1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, 1, -1)),

                // Bottom
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, 1)),
                new PositionOnlyMeshVertexData(new Vector3d(-1, -1, -1)),
                new PositionOnlyMeshVertexData(new Vector3d(1, -1, -1)),
            }, 24, new uint[] {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,

                8, 9, 10, 10, 11, 8,
                12, 13, 14, 14, 15, 12,

                16, 17, 18, 18, 19, 16,
                20, 21, 22, 22, 23, 20
            }, 36);

            material = new Material(RenderDataHandler.GenerateProgram("./Rendering/Shaders/vertex.txt", "./Rendering/Shaders/fragment.txt", StandardMeshVertexData.ShaderAttributes), RenderDataHandler.LoadTexture("Tree.png"));
            skyboxMat = new SkyboxMaterial(RenderDataHandler.GenerateProgram("./Rendering/Shaders/skyboxVert.txt", "./Rendering/Shaders/skyboxFrag.txt", PositionOnlyMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadCubeMap(new string[] { 
                    "./Artwork/Skybox/right.png", "./Artwork/Skybox/left.png", 
                    "./Artwork/Skybox/top.png", "./Artwork/Skybox/bottom.png", 
                    "./Artwork/Skybox/front.png", "./Artwork/Skybox/back.png"
                }));

            GL.ClearColor(Color4.CornflowerBlue);
        }

        private static void OnResize(ResizeEventArgs e)
        {
            windowWidth = e.Width;
            windowHeight = e.Height;
            aspectRatio = (double)windowWidth / windowHeight;

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)aspectRatio, 0.01f, 1000);
        }

        private static double totalTime = 0;
        private static void OnRender(FrameEventArgs args)
        {
            totalTime += args.Time;
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 viewMatrix = Mathmatics.CreateViewMatrix(Vector3d.Zero, Quaterniond.FromEulerAngles(new Vector3d(0, Mathmatics.ConvertToRadians(totalTime * 45), 0)));

            Graphics.UpdateCameraMatrix(viewMatrix * projectionMatrix);

            // Quad
            Graphics.RenderNow(material, testMesh, Mathmatics.CreateTransformationMatrix(new Vector3d(0, 0, -1.5), Quaterniond.Identity, Vector3d.One));

            // Skybox
            Graphics.RenderNow(skyboxMat, skyboxMesh, new Matrix4(new Matrix3(viewMatrix)) * projectionMatrix, Matrix4.Identity);
        }
    }
}
