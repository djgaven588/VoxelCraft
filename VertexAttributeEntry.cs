using OpenToolkit.Graphics.OpenGL4;

namespace VoxelCraft
{
    public struct VertexAttributeEntry
    {
        public readonly int Size;
        public readonly VertexAttribPointerType Type;
        public readonly bool Normalized;
        public readonly int Stride;
        public readonly int Offset;

        public VertexAttributeEntry(int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            Size = size;
            Type = type;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
        }
    }
}
