using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.UI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UIVertexData
    {
        public Vector3 Position;
        public Vector2 TextureCoord;

        public UIVertexData(Vector3 pos, Vector2 tex)
        {
            Position = pos;
            TextureCoord = tex;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, VertexAttribPointerType.Float, 0, false, sizeof(UIVertexData), 0, false),
            new VertexAttributeEntry(2, VertexAttribPointerType.Float, 0, false, sizeof(UIVertexData), Vector3.SizeInBytes, false)
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position"),
            new VertexShaderAttributeEntry("textureCoords")
        };
    }
}
