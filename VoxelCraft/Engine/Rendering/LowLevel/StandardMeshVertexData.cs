using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace VoxelCraft.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StandardMeshVertexData
    {
        public Vector3 Position;
        public Vector2 TextureCoord;

        public StandardMeshVertexData(Vector3 pos, Vector2 tex)
        {
            Position = pos;
            TextureCoord = tex;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, VertexAttribPointerType.Float, 0, false, sizeof(StandardMeshVertexData), 0, false),
            new VertexAttributeEntry(2, VertexAttribPointerType.Float, 0, false, sizeof(StandardMeshVertexData), Vector3.SizeInBytes, false)
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position"),
            new VertexShaderAttributeEntry("textureCoords")
        };
    }
}
