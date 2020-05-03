using OpenToolkit.Graphics.OpenGL4;

namespace VoxelCraft
{
    public struct Mesh
    {
        public int VAOBuffer { get; private set; }
        public int VertexBuffer { get; private set; }
        public int IndiceBuffer { get; private set; }

        public int VertexCount { get; private set; }
        public int IndiceCount { get; private set; }

        public VertexAttributeEntry[] AttributeData { get; private set; }

        public Mesh(int vaoBuff, int vertexBuff, int indicieBuffer, int vertexCount, int indiceCount, VertexAttributeEntry[] attributes)
        {
            VAOBuffer = vaoBuff;
            VertexBuffer = vertexBuff;
            IndiceBuffer = indicieBuffer;
            VertexCount = vertexCount;
            IndiceCount = indiceCount;
            AttributeData = attributes;

            GL.BindVertexArray(VAOBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            for (int i = 0; i < AttributeData.Length; i++)
            {
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, AttributeData[i].Size, AttributeData[i].Type, AttributeData[i].Normalized, AttributeData[i].Stride, AttributeData[i].Offset);
            }

            GL.BindVertexArray(0);
        }

        public unsafe void UploadMeshData<T>(T[] vertices, int vertexCount, uint[] indicies, int indiciesCount) where T: unmanaged
        {
            GL.BindVertexArray(VAOBuffer);

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

        public void RemoveMesh()
        {
            RenderDataHandler.DeleteVBO(VertexBuffer);
            RenderDataHandler.DeleteVBO(IndiceBuffer);
            RenderDataHandler.DeleteVAO(VAOBuffer);

            VertexBuffer = -1;
            VAOBuffer = -1;
            IndiceBuffer = -1;
        }

        public static Mesh GenerateMesh(int vertCount, int indicieCount, VertexAttributeEntry[] attributes)
        {
            return new Mesh(RenderDataHandler.GenerateVAO(), RenderDataHandler.GenerateVBO(), RenderDataHandler.GenerateVBO(), vertCount, indicieCount, attributes);
        }
    }
}
