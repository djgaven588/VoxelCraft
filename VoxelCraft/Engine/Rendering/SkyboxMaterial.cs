using OpenToolkit.Graphics.OpenGL4;

namespace VoxelCraft.Rendering
{
    public class SkyboxMaterial : Material
    {
        public SkyboxMaterial(int programID, int textureID) : base(programID, textureID)
        {

        }

        public override void BeforeRenderGroup()
        {
            if (_textureID != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, _textureID);
            }

            GL.DepthFunc(DepthFunction.Lequal);
        }

        public override void AfterRenderGroup()
        {
            GL.DepthFunc(DepthFunction.Less);
        }
    }
}
