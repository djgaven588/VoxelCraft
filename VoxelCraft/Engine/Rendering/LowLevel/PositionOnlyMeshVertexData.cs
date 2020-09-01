using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace VoxelCraft.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionOnlyMeshVertexData
    {
        public Vector3 Position;

        public PositionOnlyMeshVertexData(Vector3 pos)
        {
            Position = pos;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, VertexAttribPointerType.Float, 0, false, sizeof(PositionOnlyMeshVertexData), 0, false),
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position")
        };
    }
}
