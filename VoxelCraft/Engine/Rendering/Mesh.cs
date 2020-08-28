using OpenToolkit.Graphics.OpenGL4;
using System;

namespace VoxelCraft.Rendering
{
    public class Mesh
    {
        public int VAOBuffer { get; private set; }
        public int VertexBuffer { get; private set; }
        public int IndiceBuffer { get; private set; }

        public int VertexCount { get; private set; }
        public int IndiceCount { get; private set; }

        public VertexAttributeEntry[] AttributeData { get; private set; }

        public Mesh(int vaoBuff, int vertexBuff, int indicieBuffer, VertexAttributeEntry[] attributes)
        {
            VAOBuffer = vaoBuff;
            VertexBuffer = vertexBuff;
            IndiceBuffer = indicieBuffer;

            VertexCount = 0;
            IndiceCount = 0;

            AttributeData = attributes;

            GL.BindVertexArray(VAOBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            for (int i = 0; i < AttributeData.Length; i++)
            {
                GL.EnableVertexAttribArray(i);

                if (AttributeData[i].IsInteger)
                {
                    GL.VertexAttribIPointer(i, AttributeData[i].Size, AttributeData[i].IntegerType, AttributeData[i].Stride, new IntPtr(AttributeData[i].Offset));
                }
                else
                {
                    GL.VertexAttribPointer(i, AttributeData[i].Size, AttributeData[i].FloatType, AttributeData[i].Normalized, AttributeData[i].Stride, AttributeData[i].Offset);
                }
            }

            GL.BindVertexArray(0);
        }

        public unsafe void UploadMeshData<T>(T[] vertices, int vertexCount, uint[] indicies, int indiciesCount) where T : unmanaged
        {
            GL.BindVertexArray(VAOBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indiciesCount, indicies, BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(T) * vertexCount, vertices, BufferUsageHint.DynamicDraw);

            GL.BindVertexArray(0);

            UpdateCounts(vertexCount, indiciesCount);
        }

        public void UpdateCounts(int vertexCount, int indiceCount)
        {
            VertexCount = vertexCount;
            IndiceCount = indiceCount;
        }

        public void CleanUp()
        {
            RenderDataHandler.DeleteVBO(VertexBuffer);
            RenderDataHandler.DeleteVBO(IndiceBuffer);
            RenderDataHandler.DeleteVAO(VAOBuffer);

            VertexBuffer = -1;
            VAOBuffer = -1;
            IndiceBuffer = -1;
        }

        public static Mesh GenerateMesh(VertexAttributeEntry[] attributes)
        {
            return new Mesh(RenderDataHandler.GenerateVAO(), RenderDataHandler.GenerateVBO(), RenderDataHandler.GenerateVBO(), attributes);
        }

        public override bool Equals(object obj)
        {
            return obj is Mesh mesh &&
                   VAOBuffer == mesh.VAOBuffer &&
                   VertexBuffer == mesh.VertexBuffer &&
                   IndiceBuffer == mesh.IndiceBuffer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VAOBuffer, VertexBuffer, IndiceBuffer);
        }
    }
}
