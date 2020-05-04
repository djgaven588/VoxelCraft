using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelCraft
{
    public static class BlockDatabase
    {
        public static TextureData[] BlockTextures = new TextureData[] {
            new TextureData(), // Air, no textures of course
            new TextureData(new ushort[] { 1, 1, 1, 1, 0, 2}), // Grass
            new TextureData(new ushort[] { 2, 2, 2, 2, 2, 2}), // Dirt
        };

        public struct TextureData
        {
            public ushort[] Textures;

            public TextureData(ushort[] texs)
            {
                Textures = texs;
            }
        }
    }
}
