using System;
using System.Collections.Generic;
using System.Text;
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
            WhiteText = new UIMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes),
                StandardFonts.Arial.TextureId);

            BlackText = new UIMaterial(
                RenderDataHandler.GenerateProgram("./Engine/Rendering/Standard/Shaders/UIVertex.txt", "./Engine/Rendering/Standard/Shaders/UIFragment.txt", UIVertexData.ShaderAttributes),
                StandardFonts.Arial.TextureId);

            ChunkMaterial = new ChunkMaterial(
                RenderDataHandler.GenerateProgram("chunkVertex.txt", "chunkFragment.txt", ChunkBlockVertexData.ShaderAttributes),
                RenderDataHandler.LoadTextureArray(new string[] {
                    "./Artwork/GrassTop.png", "./Artwork/GrassSide.png",
                    "./Artwork/Dirt.png", "./Artwork/Wood.png",
                    "./Artwork/WoodTop.png", "./Artwork/Leaf.png" }, 16, 16));

            WhiteText.ChangeColor(OpenToolkit.Mathematics.Color4.White);
            BlackText.ChangeColor(OpenToolkit.Mathematics.Color4.Black);
        }
    }
}
