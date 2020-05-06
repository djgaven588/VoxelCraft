using OpenToolkit.Graphics.OpenGL4;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class ChunkMaterial : Material
    {
        public ChunkMaterial(int programID, int textureID) : base(programID, textureID)
        {

        }

        public override void BeforeRenderGroup()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, _textureID);
        }
    }
}
