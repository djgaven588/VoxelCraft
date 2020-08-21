using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.UI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UIVertexData
    {
        public Vector3 Position;
        public Vector2 TextureCoord;
        public Color4 Color;

        public UIVertexData(Vector3 pos, Vector2 tex, Color4 color)
        {
            Position = pos;
            TextureCoord = tex;
            Color = color;
        }

        public unsafe static VertexAttributeEntry[] Attributes = new VertexAttributeEntry[]
        {
            new VertexAttributeEntry(3, VertexAttribPointerType.Float, 0, false, sizeof(UIVertexData), 0, false),
            new VertexAttributeEntry(2, VertexAttribPointerType.Float, 0, false, sizeof(UIVertexData), Vector3.SizeInBytes, false),
            new VertexAttributeEntry(4, VertexAttribPointerType.Float,  0, false, sizeof(UIVertexData), Vector3.SizeInBytes + Vector2.SizeInBytes, false)
        };

        public static VertexShaderAttributeEntry[] ShaderAttributes = new VertexShaderAttributeEntry[]
        {
            new VertexShaderAttributeEntry("position"),
            new VertexShaderAttributeEntry("textureCoords"),
            new VertexShaderAttributeEntry("color")
        };
    }
}
