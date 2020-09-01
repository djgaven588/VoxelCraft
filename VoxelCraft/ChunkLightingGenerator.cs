using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelCraft
{
    public static class ChunkLightingGenerator
    {
        private static byte LightingFalloff = 1;

        public static void GenerateLighting(ChunkData chunk)
        {
            for (int i = 0; i < chunk.LightingData.Length; i++)
            {
                chunk.LightingData[i] = 1;
            }

            bool[] visited = new bool[ChunkData.CHUNK_SIZE_CUBE];
            Queue<int> toVisit = new Queue<int>();

            void PropogateLight(int x, int y, int z, int index)
            {
                if (visited[index])
                    return;

                visited[index] = true;

                if (y > 0 && chunk.LightingData[index - ChunkData.CHUNK_SIZE] + LightingFalloff <= chunk.LightingData[index] && visited[index - ChunkData.CHUNK_SIZE] == false && (chunk.BlockData[index - ChunkData.CHUNK_SIZE].ExtraData >> 5 & 1) == 1)
                {
                    chunk.LightingData[index - ChunkData.CHUNK_SIZE] = (byte)(chunk.LightingData[index] - LightingFalloff);
                    toVisit.Enqueue(index - ChunkData.CHUNK_SIZE);
                }

                if(x > 0 && x < ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if(chunk.LightingData[index - 1] + LightingFalloff <= chunk.LightingData[index] && visited[index - 1] == false && (chunk.BlockData[index - 1].ExtraData >> 5 & 1) == 1)
                    {
                        chunk.LightingData[index - 1] = (byte)(chunk.LightingData[index] - LightingFalloff);
                        toVisit.Enqueue(index - 1);
                    }
                    if (chunk.LightingData[index + 1] + LightingFalloff <= chunk.LightingData[index] && visited[index + 1] == false && (chunk.BlockData[index + 1].ExtraData >> 5 & 1) == 1)
                    {
                        chunk.LightingData[index + 1] = (byte)(chunk.LightingData[index] - LightingFalloff);
                        toVisit.Enqueue(index + 1);
                    }
                }

                if (z > 0 && z < ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (chunk.LightingData[index - ChunkData.CHUNK_SIZE_SQR] + LightingFalloff <= chunk.LightingData[index] && visited[index - ChunkData.CHUNK_SIZE_SQR] == false && (chunk.BlockData[index - ChunkData.CHUNK_SIZE_SQR].ExtraData >> 5 & 1) == 1)
                    {
                        chunk.LightingData[index - ChunkData.CHUNK_SIZE_SQR] = (byte)(chunk.LightingData[index] - LightingFalloff);
                        toVisit.Enqueue(index - ChunkData.CHUNK_SIZE_SQR);
                    }
                    if (chunk.LightingData[index + ChunkData.CHUNK_SIZE_SQR] + LightingFalloff <= chunk.LightingData[index] && visited[index + ChunkData.CHUNK_SIZE_SQR] == false && (chunk.BlockData[index + ChunkData.CHUNK_SIZE_SQR].ExtraData >> 5 & 1) == 1)
                    {
                        chunk.LightingData[index + ChunkData.CHUNK_SIZE_SQR] = (byte)(chunk.LightingData[index] - LightingFalloff);
                        toVisit.Enqueue(index + ChunkData.CHUNK_SIZE_SQR);
                    }
                }
            }

            for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    for (int y = ChunkData.CHUNK_SIZE_MINUS_ONE; y >= 0; y--)
                    {
                        int index = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE_SQR;

                        if (chunk.BlockData[index].BlockID != 0)
                        {
                            break;
                        }

                        chunk.LightingData[index] = 15;
                        PropogateLight(x, y, z, index);
                    }
                }
            }

            while (toVisit.Count > 0) 
            {
                int index = toVisit.Dequeue();
                int x = index & ChunkData.CHUNK_SIZE_MINUS_ONE;
                int y = index >> ChunkData.CHUNK_LOG_SIZE & ChunkData.CHUNK_SIZE_MINUS_ONE;
                int z = index >> ChunkData.CHUNK_LOG_SIZE_2;

                PropogateLight(x, y, z, index);
            }
        }
    }
}
