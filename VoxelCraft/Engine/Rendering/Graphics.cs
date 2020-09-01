using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Numerics;
using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering.Standard;

namespace VoxelCraft.Rendering
{
    public static class Graphics
    {
        private static Matrix4x4 ViewProjectionMatrix;

        private static Matrix4x4 ViewMatrix;
        private static Matrix4x4 ProjectionMatrix;

        private static readonly Dictionary<Mesh, Dictionary<Material, Queue<Matrix4x4>>> renderingQueue = new Dictionary<Mesh, Dictionary<Material, Queue<Matrix4x4>>>();
        private static SkyboxMaterial SkyboxMaterial;

        private static Camera Camera;

        public static void Initialize(System.Drawing.Color clearColor)
        {
            GL.ClearColor(clearColor);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.FrontFace(FrontFaceDirection.Cw);
        }

        public static void BeforeRender()
        {
            if (Camera != null)
            {
                UpdateCameraMatrix(Mathmatics.CreateViewMatrix(Camera.Position, Camera.Rotation));
                UIOrthographic = Matrix4x4.CreateOrthographic(EntryPoint.WindowWidth, EntryPoint.WindowHeight, 0.01f, 1000f);
            }
            else
            {
                Debug.Log("Graphics.UseCamera needs to be called, there is no active camera!");
            }
        }

        public static void UseSkybox(SkyboxMaterial skyboxMat)
        {
            SkyboxMaterial = skyboxMat;
        }

        public static void UseCamera(Camera camera)
        {
            Camera = camera;
        }

        public static void HandleQueue()
        {
            foreach (KeyValuePair<Mesh, Dictionary<Material, Queue<Matrix4x4>>> entry in renderingQueue)
            {
                Mesh meshToRender = entry.Key;
                Dictionary<Material, Queue<Matrix4x4>> instances = entry.Value;
                GL.BindVertexArray(meshToRender.VAOBuffer);

                foreach (KeyValuePair<Material, Queue<Matrix4x4>> instanceEntry in instances)
                {
                    Material mat = instanceEntry.Key;
                    Matrix4x4[] rendInstances = new Matrix4x4[instanceEntry.Value.Count];
                    instanceEntry.Value.CopyTo(rendInstances, 0);

                    GL.UseProgram(mat.ProgramID);

                    mat.LoadMatrix4(mat.ViewProjectionID, ViewProjectionMatrix);

                    mat.BeforeRenderGroup();

                    while (instanceEntry.Value.Count > 0)
                    {
                        mat.BeforeRenderIndividual();

                        mat.LoadMatrix4(mat.WorldTransform, instanceEntry.Value.Dequeue());
                        GL.DrawElements(BeginMode.Triangles, meshToRender.IndiceCount, DrawElementsType.UnsignedInt, 0);

                        mat.AfterRenderIndividual();
                    }

                    mat.AfterRenderGroup();

                    GL.UseProgram(0);
                }

                GL.BindVertexArray(0);
            }

            if (SkyboxMaterial != null)
            {
                Matrix4x4 rot = Matrix4x4.CreateRotationY(Mathmatics.ConvertToRadians(Camera.Rotation.Y)) * Matrix4x4.CreateRotationX(Mathmatics.ConvertToRadians(Camera.Rotation.X)) * Matrix4x4.CreateRotationZ(Mathmatics.ConvertToRadians(Camera.Rotation.Z));
                DrawNow(SkyboxMaterial, PrimitiveMeshes.Skybox, rot * ProjectionMatrix, Matrix4x4.Identity);
            }
        }

        public static void ClearQueue()
        {
            renderingQueue.Clear();
        }

        public static void UpdateCameraMatrix(Matrix4x4 newMatrix)
        {
            ViewMatrix = newMatrix;
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
        }

        public static void UpdateProjectionMatrix(Matrix4x4 newMatrix)
        {
            ProjectionMatrix = newMatrix;
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
        }

        public static void DrawNow(Material material, Mesh mesh, Matrix4x4 viewProjectionMatrix, Matrix4x4 transformMatrix)
        {
            material.LoadMatrix4(material.ViewProjectionID, viewProjectionMatrix);

            GL.UseProgram(material.ProgramID);
            GL.BindVertexArray(mesh.VAOBuffer);

            material.BeforeRenderGroup();
            material.BeforeRenderIndividual();

            material.LoadMatrix4(material.WorldTransform, transformMatrix);
            GL.DrawElements(BeginMode.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, 0);

            material.AfterRenderIndividual();
            material.AfterRenderGroup();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public static void DrawNow(Material material, Mesh mesh, Matrix4x4 transformMatrix)
        {
            material.LoadMatrix4(material.ViewProjectionID, ViewProjectionMatrix);

            GL.UseProgram(material.ProgramID);
            GL.BindVertexArray(mesh.VAOBuffer);

            material.BeforeRenderGroup();
            material.BeforeRenderIndividual();

            material.LoadMatrix4(material.WorldTransform, transformMatrix);
            GL.DrawElements(BeginMode.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, 0);

            material.AfterRenderIndividual();
            material.AfterRenderGroup();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public static void DrawNowInstanced(Material material, Mesh mesh, Matrix4x4[] transformMatrix)
        {
            material.LoadMatrix4(material.ViewProjectionID, ViewProjectionMatrix);

            GL.UseProgram(material.ProgramID);
            GL.BindVertexArray(mesh.VAOBuffer);

            material.BeforeRenderGroup();

            for (int i = 0; i < transformMatrix.Length; i++)
            {
                material.BeforeRenderIndividual();
                material.LoadMatrix4(material.WorldTransform, transformMatrix[i]);
                GL.DrawElements(BeginMode.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, 0);
            }

            material.AfterRenderIndividual();
            material.AfterRenderGroup();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public static void QueueDraw(Material material, Mesh mesh, Matrix4x4 transformMatrix)
        {
            if (!renderingQueue.TryGetValue(mesh, out _))
            {
                renderingQueue.Add(mesh, new Dictionary<Material, Queue<Matrix4x4>>());
            }

            if (!renderingQueue[mesh].TryGetValue(material, out _))
            {
                renderingQueue[mesh].Add(material, new Queue<Matrix4x4>());
            }

            renderingQueue[mesh][material].Enqueue(transformMatrix);
        }

        public static void QueueDrawInstanced(Material material, Mesh mesh, Matrix4x4[] transformMatrix)
        {
            if (!renderingQueue.TryGetValue(mesh, out _))
            {
                renderingQueue.Add(mesh, new Dictionary<Material, Queue<Matrix4x4>>());
            }

            if (!renderingQueue[mesh].TryGetValue(material, out _))
            {
                renderingQueue[mesh].Add(material, new Queue<Matrix4x4>());
            }

            for (int i = 0; i < transformMatrix.Length; i++)
            {
                renderingQueue[mesh][material].Enqueue(transformMatrix[i]);
            }
        }

        private static Matrix4x4 UIOrthographic;

        public static Matrix4x4 GetUIMatrix(Vector2 offset, float scale, UIPosition position)
        {
            float width = EntryPoint.WindowWidth;
            float height = EntryPoint.WindowHeight;
            float uiScale = width > height ? width / 1920 : height / 1080;

            return Mathmatics.CreateTransformationMatrix(
                new Vector3(
                    width * -0.5f + width * position.Center.X + (position.Offset.X + offset.X) * uiScale,
                    height * 0.5f - height * position.Center.Y - (position.Offset.X + offset.Y) * uiScale, 
                    0),
                Vector3.Zero, Vector3.One * uiScale * scale) * UIOrthographic;
        }
    }
}
