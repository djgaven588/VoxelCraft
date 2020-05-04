using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelCraft
{
    public static class ChunkTerrainGenerator
    {
        static ChunkTerrainGenerator()
        {

        }

        public static void GenerateTerrain(ref ChunkData chunk)
        {
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
