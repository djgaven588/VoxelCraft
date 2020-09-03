using VoxelCraft.Engine.Rendering.Standard.Materials;
using VoxelCraft.Engine.Rendering.UI;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering.Standard
{
    public static class StandardMaterials
    {
        public static UIMaterial WhiteText { get; private set; }
        public static UIMaterial BlackText { get; private set; }

        public static Material ChunkMaterial { get; private set; }

        public static void Init()
        {
            int uiShader = RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes);
            WhiteText = new UIMaterial(
                uiShader,
                StandardFonts.Arial.TextureId); 
            
            BlackText = new UIMaterial(
                uiShader,
                StandardFonts.Arial.TextureId);

            ChunkMaterial = new ChunkMaterial(
                RenderDataHandler.GenerateProgram("chunkVertex.txt", "chunkFragment.txt", ChunkBlockVertexData.ShaderAttributes),
                RenderDataHandler.LoadTextureArray(new string[] {
                    "./Artwork/GrassTop.png", "./Artwork/GrassSide.png",
                    "./Artwork/Dirt.png", "./Artwork/Wood.png",
                    "./Artwork/WoodTop.png", "./Artwork/Leaf.png",
                    "./Artwork/Stone.png"
                }, 16, 16));

            WhiteText.ChangeColor(OpenTK.Mathematics.Color4.White);
            BlackText.ChangeColor(OpenTK.Mathematics.Color4.Black);
        }
    }
}
