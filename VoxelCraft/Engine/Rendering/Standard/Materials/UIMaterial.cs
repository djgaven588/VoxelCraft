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
            colorUniform = GetUniformLocation("color");
        }

        private int colorUniform;

        public void ChangeColor(Color4 color)
        {
            LoadColor4(colorUniform, color);
        }

        public override void BeforeRenderGroup()
        {
            if (_textureID != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _textureID);
            }
        }
    }
}
