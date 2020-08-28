using System.Collections.Generic;

namespace VoxelCraft
{
    public static class BlockDatabase
    {
        public static Dictionary<string, BlockData> NameToBlockData = new Dictionary<string, BlockData>();
        public static BlockData[] IDToBlockData;

        public static TextureData[] BlockTextures = new TextureData[] {
            new TextureData(), // Air, no textures of course
            new TextureData(new ushort[] { 1, 1, 1, 1, 0, 2}), // Grass
            new TextureData(new ushort[] { 2, 2, 2, 2, 2, 2}), // Dirt
            new TextureData(new ushort[] { 3, 3, 3, 3, 4, 4}), // Wood
            new TextureData(new ushort[] { 5, 5, 5, 5, 5, 5}), // Leaf
        };


        public struct TextureData
        {
            public ushort[] Textures;

            public TextureData(ushort[] texs)
            {
                Textures = texs;
            }
        }

        static BlockDatabase()
        {
            IDToBlockData = new BlockData[]
            {
                new BlockData(0, 0, true, true),    // Air
                new BlockData(1, 0, false, false),  // Grass
                new BlockData(2, 0, false, false),  // Dirt
                new BlockData(3, 0, false, false),  // Wood
                new BlockData(4, 0, true, false),   // Leaf
            };

            NameToBlockData.Add("Air", IDToBlockData[0]);
            NameToBlockData.Add("Grass", IDToBlockData[1]);
            NameToBlockData.Add("Dirt", IDToBlockData[2]);
            NameToBlockData.Add("Wood", IDToBlockData[3]);
            NameToBlockData.Add("Leaf", IDToBlockData[4]);
        }
    }
}
