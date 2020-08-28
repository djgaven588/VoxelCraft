using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Rendering;
using Graphics = VoxelCraft.Rendering.Graphics;

namespace VoxelCraft
{
    internal class EntryPoint
    {
        public const bool DebugGraphics = false;

        private static WindowHandler Window;

        static void Main(string[] args)
        {
            Debug.Log("Starting...");

            WindowWidth = 640;
            WindowHeight = 480;
            AspectRatio = (double)WindowWidth / WindowHeight;

            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)AspectRatio, 0.01f, 1000);

            Window = new WindowHandler(new GameWindowSettings()
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
                Size = new OpenToolkit.Mathematics.Vector2i(WindowWidth, WindowHeight)
            }, OnLoad, null, OnClosing, OnResize, null, OnRender);

            Window.Run();
        }

        public static int WindowWidth { get; private set; }
        public static int WindowHeight { get; private set; }
        public static double AspectRatio { get; private set; }

        private static Matrix4x4 projectionMatrix;

        private static void OnLoad()
        {
            
            SkyboxMaterial skyboxMat = new SkyboxMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/skyboxVert.txt", "./Engine/Rendering/Standard/Shaders/skyboxFrag.txt", PositionOnlyMeshVertexData.ShaderAttributes),
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
            WindowWidth = e.Width;
            WindowHeight = e.Height;
            AspectRatio = (double)WindowWidth / WindowHeight;

            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Mathmatics.ConvertToRadians(60), (float)AspectRatio, 0.01f, 1000);
        }

        private static void OnRender(FrameEventArgs args)
        {
            World.BeforeRender(args.Time);
            Graphics.BeforeRender();

            World.UpdateWorld(args.Time);

            Graphics.UpdateProjectionMatrix(projectionMatrix);

            Graphics.HandleQueue();
        }

        private static void OnClosing(CancelEventArgs e)
        {
            ChunkOperationDispatcher.ShutdownThreads();
        }
    }
}
