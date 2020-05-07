using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Collections.Generic;

namespace VoxelCraft.Rendering
{
    public static class Graphics
    {
        private static Matrix4 ViewProjectionMatrix;

        private static Matrix4 ViewMatrix;
        private static Matrix4 ProjectionMatrix;

        private static Dictionary<Mesh, Dictionary<Material, Queue<Matrix4>>> renderingQueue = new Dictionary<Mesh, Dictionary<Material, Queue<Matrix4>>>();
        private static SkyboxMaterial SkyboxMaterial;

        private static Camera Camera;

        public static void Initialize(Color4 clearColor)
        {
            GL.ClearColor(clearColor);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.FrontFace(FrontFaceDirection.Cw);

            //GL.Enable(EnableCap.Blend);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
            //GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
        }

        public static void BeforeRender()
        {
            if (Camera != null)
            {
                UpdateCameraMatrix(Mathmatics.CreateViewMatrix(Camera.cameraPos, Camera.cameraRot));
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
            foreach (KeyValuePair<Mesh, Dictionary<Material, Queue<Matrix4>>> entry in renderingQueue)
            {
                Mesh meshToRender = entry.Key;
                Dictionary<Material, Queue<Matrix4>> instances = entry.Value;
                GL.BindVertexArray(meshToRender.VAOBuffer);

                foreach (KeyValuePair<Material, Queue<Matrix4>> instanceEntry in instances)
                {
                    Material mat = instanceEntry.Key;
                    Matrix4[] rendInstances = new Matrix4[instanceEntry.Value.Count];
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
                DrawNow(SkyboxMaterial, PrimitiveMeshes.Skybox, new Matrix4(new Matrix3(ViewMatrix)) * ProjectionMatrix, Matrix4.Identity);
            }
        }

        public static void ClearQueue()
        {
            renderingQueue.Clear();
        }

        public static void UpdateCameraMatrix(Matrix4 newMatrix)
        {
            ViewMatrix = newMatrix;
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
        }

        public static void UpdateProjectionMatrix(Matrix4 newMatrix)
        {
            ProjectionMatrix = newMatrix;
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
        }

        public static void DrawNow(Material material, Mesh mesh, Matrix4 viewProjectionMatrix, Matrix4 transformMatrix)
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

        public static void DrawNow(Material material, Mesh mesh, Matrix4 transformMatrix)
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

        public static void DrawNowInstanced(Material material, Mesh mesh, Matrix4[] transformMatrix)
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

        public static void QueueDraw(Material material, Mesh mesh, Matrix4 transformMatrix)
        {
            if (!renderingQueue.TryGetValue(mesh, out _))
            {
                renderingQueue.Add(mesh, new Dictionary<Material, Queue<Matrix4>>());
            }

            if (!renderingQueue[mesh].TryGetValue(material, out _))
            {
                renderingQueue[mesh].Add(material, new Queue<Matrix4>());
            }

            renderingQueue[mesh][material].Enqueue(transformMatrix);
        }

        public static void QueueDrawInstanced(Material material, Mesh mesh, Matrix4[] transformMatrix)
        {
            if (!renderingQueue.TryGetValue(mesh, out _))
            {
                renderingQueue.Add(mesh, new Dictionary<Material, Queue<Matrix4>>());
            }

            if (!renderingQueue[mesh].TryGetValue(material, out _))
            {
                renderingQueue[mesh].Add(material, new Queue<Matrix4>());
            }

            for (int i = 0; i < transformMatrix.Length; i++)
            {
                renderingQueue[mesh][material].Enqueue(transformMatrix[i]);
            }
        }
    }
}
