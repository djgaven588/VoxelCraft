using Noise;
using OpenToolkit.Mathematics;

namespace VoxelCraft
{
    public static class ChunkTerrainGenerator
    {
        private static readonly OpenSimplexNoise Noise;

        static ChunkTerrainGenerator()
        {
            Noise = new OpenSimplexNoise(0);
        }

        public static void GenerateTerrain(ref ChunkData chunk)
        {
            /*
            Random random = new Random(0);
            for (int i = 0; i < ChunkData.CHUNK_SIZE_CUBE; i++)
            {
                bool isAir = random.Next(0, 2) == 0;
                chunk.BlockData[i].BlockID = (ushort)(isAir ? 0 : 1);
                chunk.BlockData[i].ExtraData = (byte)(isAir ? 3 << 5 : 0);
            }
            return;*/
            Vector3d worldPos = chunk.ChunkPosition.ChunkToWorld().ToVector();

            for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    int value = (int)GetNoise(x, z, worldPos, 0.03d, 3, 5, 6, 40);


                    value -= (int)worldPos.Y;

                    for (int y = ChunkData.CHUNK_SIZE - 1; y >= 0; y--)
                    {
                        int chunkIndex = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE;
                        if (y > value)
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 0, ExtraData = 3 << 5 };
                        }
                        else if (y == value)
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 1, ExtraData = 0 };
                        }
                        else
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 2, ExtraData = 0 };
                        }
                    }
                }
            }
        }

        private static double GetNoise(double x, double z, Vector3d chunkOffset, double scale, double amplitude, double exponential, double exponentialDownscale, double offset)
        {
            double value = ((Noise.Evaluate((x + chunkOffset.X) * scale, (z + chunkOffset.Z) * scale) + 1) / 2) * amplitude;

            value += (value * exponential * value * exponential) / exponentialDownscale + offset;

            return value;
        }
    }
}
