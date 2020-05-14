using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using VoxelCraft.Rendering;
using Graphics = VoxelCraft.Rendering.Graphics;

namespace VoxelCraft
{
    internal class EntryPoint
    {
        public const bool DebugGraphics = false;

        static void Main(string[] args)
        {
            Debug.Log("Starting...");

            windowWidth = 640;
            windowHeight = 480;
            aspectRatio = (double)windowWidth / windowHeight;

            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)aspectRatio, 0.01f, 1000);

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
                Size = new OpenToolkit.Mathematics.Vector2i(windowWidth, windowHeight)
            }, OnLoad, null, OnClosing, OnResize, null, OnRender).Run();
        }

        private static int windowWidth;
        private static int windowHeight;
        private static double aspectRatio;

        private static Matrix4x4 projectionMatrix;

        private static void OnLoad()
        {

            SkyboxMaterial skyboxMat = new SkyboxMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Shaders/skyboxVert.txt", "./Engine/Rendering/Shaders/skyboxFrag.txt", PositionOnlyMeshVertexData.ShaderAttributes),
                RenderDataHandler.LoadCubeMap(new string[] {
                    "./Artwork/Skybox/right.png", "./Artwork/Skybox/left.png",
                    "./Artwork/Skybox/top.png", "./Artwork/Skybox/bottom.png",
                    "./Artwork/Skybox/front.png", "./Artwork/Skybox/back.png"
                }));

            Graphics.UseSkybox(skyboxMat);

            World.Initialize();

            Graphics.Initialize(Color.AliceBlue);

            if (DebugGraphics)
            {
#pragma warning disable CS0162
                Debug.EnableOpenGLDebug();
#pragma warning restore CS0162
            }
        }

        private static void OnResize(ResizeEventArgs e)
        {
            windowWidth = e.Width;
            windowHeight = e.Height;
            aspectRatio = (double)windowWidth / windowHeight;

            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)aspectRatio, 0.01f, 1000);
        }

        private static void OnRender(FrameEventArgs args)
        {
            Graphics.BeforeRender();

            World.UpdateWorld(args.Time);

            Graphics.BeforeRender();

            Graphics.UpdateProjectionMatrix(projectionMatrix);

            Graphics.HandleQueue();
        }

        private static void OnClosing(CancelEventArgs e)
        {
            ChunkOperationDispatcher.ShutdownThreads();
        }
    }
}
