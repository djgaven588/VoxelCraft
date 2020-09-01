using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.Standard.Materials
{
    public class UIMaterial : Material
    {
        public UIMaterial(int programID, int textureID) : base(programID, textureID)
        {
            _colorUniform = GetUniformLocation("color");
        }

        private int _colorUniform;
        private Color4 _color;

        public void ChangeColor(Color4 color)
        {
            _color = color;
        }

        public override void BeforeRenderGroup()
        {
            LoadColor4(_colorUniform, _color);
            Debug.Log(_color);
            base.BeforeRenderGroup();
        }
    }
}
