using Noise;
using System.Numerics;

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
            Vector3 worldPos = chunk.ChunkPosition.ChunkToWorld().ToVector();

            for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    int value = (int)GetValue(x, z, worldPos);//GetNoise(x, z, worldPos, 0.03d, 3, 5, 6, 40);


                    value -= (int)worldPos.Y;

                    for (int y = ChunkData.CHUNK_SIZE - 1; y >= 0; y--)
                    {
                        int chunkIndex = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE;
                        if (y > value)
                        {
                            chunk.Data[chunkIndex] = new BlockData() { BlockID = 0, ExtraData = 3 << 5 };
                        }
                        else if (y == value)
                        {
                            chunk.Data[chunkIndex] = new BlockData() { BlockID = 1, ExtraData = 0 };
                        }
                        else
                        {
                            chunk.Data[chunkIndex] = new BlockData() { BlockID = 2, ExtraData = 0 };
                        }
                    }
                }
            }
        }

        private static double GetNoise(double x, double z, Vector3 chunkOffset, double scale, double amplitude, double exponential, double exponentialDownscale, double offset)
        {
            double value = ((Noise.Evaluate((x + chunkOffset.X) * scale, (z + chunkOffset.Z) * scale) + 1) / 2) * amplitude;

            value += (value * exponential * value * exponential) / exponentialDownscale + offset;

            return value;
        }

        private static double GetValue(double x, double z, Vector3 chunkOffset)
        {
            double octave1 = GetNoise(x, z, chunkOffset, 0.03d, 3, 5, 6, 40);
            double octave2 = GetNoise(x, z, chunkOffset, 0.07d, 7, 1, 1, 40);

            return octave1 + octave2 * 0.1;
        }
    }
}
