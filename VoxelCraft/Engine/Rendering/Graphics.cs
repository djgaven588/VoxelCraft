using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Collections.Generic;

namespace VoxelCraft.Rendering
{
    public static class Graphics
    {
        private static Matrix4 ViewProjectionMatrix;
        private static Dictionary<Mesh, Dictionary<Material, Queue<Matrix4>>> renderingQueue = new Dictionary<Mesh, Dictionary<Material, Queue<Matrix4>>>();

        public static void HandleQueue()
        {
            foreach (KeyValuePair<Mesh, Dictionary<Material, Queue<Matrix4>>> entry in renderingQueue)
            {
                GL.BindVertexArray(entry.Key.VAOBuffer);

                foreach (KeyValuePair<Material, Queue<Matrix4>> instanceEntry in entry.Value)
                {
                    GL.UseProgram(instanceEntry.Key.ProgramID);

                    instanceEntry.Key.LoadMatrix4(instanceEntry.Key.ViewProjectionID, ViewProjectionMatrix);

                    instanceEntry.Key.BeforeRenderGroup();

                    while (instanceEntry.Value.Count > 0)
                    {
                        instanceEntry.Key.BeforeRenderIndividual();
                        instanceEntry.Key.LoadMatrix4(instanceEntry.Key.WorldTransform, instanceEntry.Value.Dequeue());
                        GL.DrawElements(BeginMode.Triangles, entry.Key.IndiceCount, DrawElementsType.UnsignedInt, 0);
                        instanceEntry.Key.AfterRenderIndividual();
                    }

                    instanceEntry.Key.AfterRenderGroup();

                    GL.UseProgram(0);
                }

                GL.BindVertexArray(0);
            }
        }

        public static void ClearQueue()
        {
            renderingQueue.Clear();
        }

        public static void UpdateCameraMatrix(Matrix4 newMatrix)
        {
            ViewProjectionMatrix = newMatrix;
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
