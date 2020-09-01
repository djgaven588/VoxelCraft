using OpenTK.Mathematics;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.Standard.Materials
{
    public class UIMaterial : Material
    {
        public UIMaterial(int programID, int textureID) : base(programID, textureID)
        {
            _colorUniform = GetUniformLocation("color");
        }

        private readonly int _colorUniform;
        private Color4 _color;

        public void ChangeColor(Color4 color)
        {
            _color = color;
        }

        public override void BeforeRenderGroup()
        {
            LoadColor4(_colorUniform, _color);
            base.BeforeRenderGroup();
        }
    }
}
