using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace VoxelCraft
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StandardMeshVertexData
    {
        public Vector3d Position;
        public Vector2d TextureCoord;

        public StandardMeshVertexData(Vector3d pos, Vector2d tex)
        {
            Position = pos;
            TextureCoord = tex;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, OpenToolkit.Graphics.OpenGL4.VertexAttribPointerType.Double, false, sizeof(StandardMeshVertexData), 0),
            new VertexAttributeEntry(2, OpenToolkit.Graphics.OpenGL4.VertexAttribPointerType.Double, false, sizeof(StandardMeshVertexData), Vector3d.SizeInBytes)
        };
    }
}
