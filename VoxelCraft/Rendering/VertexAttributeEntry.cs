using OpenToolkit.Graphics.OpenGL4;

namespace VoxelCraft.Rendering
{
    public struct VertexAttributeEntry
    {
        public readonly int Size;
        public readonly VertexAttribPointerType FloatType;
        public readonly VertexAttribIntegerType IntegerType;
        public readonly bool Normalized;
        public readonly int Stride;
        public readonly int Offset;
        public readonly bool IsInteger;

        public VertexAttributeEntry(int size, VertexAttribPointerType floatType, VertexAttribIntegerType intType, bool normalized, int stride, int offset, bool isInt)
        {
            Size = size;
            FloatType = floatType;
            IntegerType = intType;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
            IsInteger = isInt;
        }
    }
}
