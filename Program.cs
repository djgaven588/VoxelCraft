using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.IO;

namespace VoxelCraft
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Log("Hello World!");

            new WindowHandler(new GameWindowSettings()
            {
                IsMultiThreaded = false, RenderFrequency = 60, UpdateFrequency = 50
            },
            new NativeWindowSettings()
            {
                API = ContextAPI.OpenGL, APIVersion = new Version(4, 0), Title = "Voxel Craft", Size = new Vector2i(648, 480)
            }, OnLoad, null, null, null, OnRender).Run();
        }

        private static Mesh testMesh;
        private static int shaderProgram;
        private static int testTexture;
        private static void OnLoad()
        {
            testMesh = Mesh.GenerateMesh(0, 0, StandardMeshVertexData.Attributes);/*new VertexAttributeEntry[] { 
                    new VertexAttributeEntry(3, VertexAttribPointerType.Double, false, Vector3d.SizeInBytes, 0) 
                });*/

            testMesh.UploadMeshData(new StandardMeshVertexData[] { 
                new StandardMeshVertexData(new Vector3d(-0.25,  0.25, 0), new Vector2d(0, 0)),
                new StandardMeshVertexData(new Vector3d(0.25,   0.25, 0), new Vector2d(1, 0)),
                new StandardMeshVertexData(new Vector3d(0.25,  -0.25, 0), new Vector2d(1, 1)),
                new StandardMeshVertexData(new Vector3d(-0.25, -0.25, 0), new Vector2d(0, 1))
            }, 4, new uint[] { 0, 1, 2, 2, 3, 0 }, 6);

            testTexture = RenderDataHandler.LoadTexture("Tree.png");

            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, File.ReadAllText("vertex.txt"));
            GL.CompileShader(vertShader);
            Debug.Log(GL.GetShaderInfoLog(vertShader));

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, File.ReadAllText("fragment.txt"));
            GL.CompileShader(fragShader);
            Debug.Log(GL.GetShaderInfoLog(fragShader));

            shaderProgram = GL.CreateProgram();

            GL.AttachShader(shaderProgram, vertShader);
            GL.AttachShader(shaderProgram, fragShader);

            GL.BindAttribLocation(shaderProgram, 0, "position");
            GL.BindAttribLocation(shaderProgram, 1, "textureCoord");

            GL.LinkProgram(shaderProgram);
            GL.ValidateProgram(shaderProgram);

            GL.DetachShader(shaderProgram, vertShader);
            GL.DetachShader(shaderProgram, fragShader);
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            GL.ClearColor(Color4.CornflowerBlue);
        }

        private static void OnRender(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgram);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, testTexture);

            GL.BindVertexArray(testMesh.VAOBuffer);

            GL.DrawElements(BeginMode.Triangles, testMesh.IndiceCount, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            GL.UseProgram(0);
        }
    }
}
