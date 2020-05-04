using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace VoxelCraft.Rendering
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
            new VertexAttributeEntry(3, VertexAttribPointerType.Double, 0, false, sizeof(StandardMeshVertexData), 0, false),
            new VertexAttributeEntry(2, VertexAttribPointerType.Double, 0, false, sizeof(StandardMeshVertexData), Vector3d.SizeInBytes, false)
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position"),
            new VertexShaderAttributeEntry("textureCoord")
        };
    }
}
