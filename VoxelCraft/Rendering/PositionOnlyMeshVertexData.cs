using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace VoxelCraft.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionOnlyMeshVertexData
    {
        public Vector3d Position;

        public PositionOnlyMeshVertexData(Vector3d pos)
        {
            Position = pos;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, VertexAttribPointerType.Double, 0, false, sizeof(PositionOnlyMeshVertexData), 0, false),
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position")
        };
    }
}
