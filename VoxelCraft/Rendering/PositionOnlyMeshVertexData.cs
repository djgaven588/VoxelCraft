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
            new VertexAttributeEntry(3, OpenToolkit.Graphics.OpenGL4.VertexAttribPointerType.Double, false, sizeof(PositionOnlyMeshVertexData), 0),
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position")
        };
    }
}
