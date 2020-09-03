using OpenTK.Mathematics;
using System;
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
        protected Color4 _color;

        public void ChangeColor(Color4 color)
        {
            _color = color;
        }

        public override void BeforeRenderGroup()
        {
            LoadColor4(_colorUniform, _color);
            base.BeforeRenderGroup();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) && obj is UIMaterial material &&
                   _color == material._color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _color);
        }
    }
}
