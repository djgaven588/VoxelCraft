using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace VoxelCraft.Rendering
{
    public static class Graphics
    {
        private static Matrix4 ViewProjectionMatrix;

        public static void HandleQueue()
        {

        }
        
        public static void UpdateCameraMatrix(Matrix4 newMatrix)
        {
            ViewProjectionMatrix = newMatrix;
        }

        public static void RenderNow(Material material, Mesh mesh, Matrix4 viewProjectionMatrix, Matrix4 transformMatrix)
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

        public static void RenderNow(Material material, Mesh mesh, Matrix4 transformMatrix)
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

        public static void RenderNowInstanced(Material material, Mesh mesh, Matrix4[] transformMatrix)
        {
            material.LoadMatrix4(material.ViewProjectionID, ViewProjectionMatrix);

            GL.UseProgram(material.ProgramID);
            GL.BindVertexArray(mesh.VAOBuffer);

            material.BeforeRenderGroup();
            material.BeforeRenderIndividual();

            for (int i = 0; i < transformMatrix.Length; i++)
            {
                material.LoadMatrix4(material.WorldTransform, transformMatrix[i]);
                GL.DrawElements(BeginMode.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, 0);
            }

            material.AfterRenderIndividual();
            material.AfterRenderGroup();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
    }
}
