using OpenToolkit.Mathematics;
using System;

namespace VoxelCraft
{
    public static class ChunkTerrainGenerator
    {
        static ChunkTerrainGenerator()
        {

        }

        public static void GenerateTerrain(ref ChunkData chunk)
        {
            Random random = new Random(0);
            for (int i = 0; i < ChunkData.CHUNK_SIZE_CUBE; i++)
            {
                bool isAir = random.Next(0, 2) == 0;
                chunk.BlockData[i].BlockID = (ushort)(isAir ? 0 : 1);
                chunk.BlockData[i].ExtraData = (byte)(isAir ? 3 << 5 : 0);
            }
            return;
            Vector3d worldPos = Coordinate.ToVector(chunk.ChunkPosition);

            int index = 0;
            for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    for (int y = ChunkData.CHUNK_SIZE - 1; y >= 0; y--)
                    {
                        int chunkIndex = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE;
                        if (y > 5)
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 0, ExtraData = 3 << 5 };
                        }
                        else if(y == 5)
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 1, ExtraData = 0 };
                        }
                        else
                        {
                            chunk.BlockData[chunkIndex] = new BlockData() { BlockID = 2, ExtraData = 0 };
                        }
                    }

                    index++;
                }
            }
        }
    }
}
