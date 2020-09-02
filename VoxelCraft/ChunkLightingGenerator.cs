using System.Collections.Generic;
using System.Diagnostics;

namespace VoxelCraft
{
    public static class ChunkLightingGenerator
    {
        private static byte LightingFalloff = 1;
        private static double _totalTime = 0;
        private static int _totalGenerations = 0;
        private static int totalCount = 0;

        public static void GenerateLighting(ChunkData chunk, ChunkData[] neighbors)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int count = 0;

            for (int i = 0; i < chunk.LightingData.Length; i++)
            {
                chunk.LightingData[i] = 0;
            }

            Queue<(ChunkData, uint)> toVisit = new Queue<(ChunkData, uint)>(ChunkData.CHUNK_SIZE_SQR);

            void PropogateLight(uint x, uint y, uint z, uint index, ChunkData chunk)
            {
                count++;

                {
                    (bool valid, ChunkData chunkToEdit, uint neighborIndex) = NearbyBlock(x, y, z, index, 5, true, chunk, neighbors);

                    if (valid && chunkToEdit != null && (chunkToEdit.BlockData[neighborIndex].ExtraData >> 5 & 1) == 1 && chunkToEdit.LightingData[neighborIndex] + LightingFalloff < chunk.LightingData[index])
                    {
                        int cost = (chunkToEdit.BlockData[neighborIndex].ExtraData >> 6 & 1) == 1 ? 0 : LightingFalloff;
                        chunkToEdit.LightingData[neighborIndex] = (byte)(chunk.LightingData[index] - cost);
                        chunkToEdit.UploadLighting = true;
                        toVisit.Enqueue((chunkToEdit, neighborIndex));
                    }
                }

                {
                    (bool valid, ChunkData chunkToEdit, uint neighborIndex) = NearbyBlock(x, y, z, index, 0, true, chunk, neighbors);

                    LightCheck();

                    (valid, chunkToEdit, neighborIndex) = NearbyBlock(x, y, z, index, 1, true, chunk, neighbors);

                    LightCheck();

                    (valid, chunkToEdit, neighborIndex) = NearbyBlock(x, y, z, index, 2, true, chunk, neighbors);

                    LightCheck();

                    (valid, chunkToEdit, neighborIndex) = NearbyBlock(x, y, z, index, 3, true, chunk, neighbors);

                    LightCheck();

                    (valid, chunkToEdit, neighborIndex) = NearbyBlock(x, y, z, index, 4, true, chunk, neighbors);

                    LightCheck();

                    void LightCheck()
                    {
                        if (valid && chunkToEdit != null && (chunkToEdit.BlockData[neighborIndex].ExtraData >> 5 & 1) == 1 && chunkToEdit.LightingData[neighborIndex] + LightingFalloff < chunk.LightingData[index])
                        {
                            chunkToEdit.LightingData[neighborIndex] = (byte)(chunk.LightingData[index] - LightingFalloff);
                            chunkToEdit.UploadLighting = true;
                            toVisit.Enqueue((chunkToEdit, neighborIndex));
                        }
                    }
                }
            }

            if (true)//chunk.ChunkPosition.Y + 1 == Region.REGION_SIZE)
            {
                for (uint x = 0; x < ChunkData.CHUNK_SIZE; x++)
                {
                    for (uint z = 0; z < ChunkData.CHUNK_SIZE; z++)
                    {
                        uint y = ChunkData.CHUNK_SIZE_MINUS_ONE;
                        uint index = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE_SQR;
                        chunk.LightingData[index] = 15;
                        PropogateLight(x, y, z, index, chunk);
                    }
                }
            }

            while (toVisit.Count > 0) 
            {
                (ChunkData ch, uint index) = toVisit.Dequeue();
                uint x = index & ChunkData.CHUNK_SIZE_MINUS_ONE;
                uint y = index >> ChunkData.CHUNK_LOG_SIZE & ChunkData.CHUNK_SIZE_MINUS_ONE;
                uint z = index >> ChunkData.CHUNK_LOG_SIZE_2;

                PropogateLight(x, y, z, index, ch);
            }

            stopWatch.Stop();

            _totalTime += stopWatch.Elapsed.TotalMilliseconds;
            _totalGenerations++;
            totalCount += count;

            Debug.Log($"Lighting: {stopWatch.Elapsed.TotalMilliseconds}, Propogations: {count}");
            Debug.Log($"Average MS: {_totalTime / _totalGenerations}, {totalCount / _totalGenerations}");
        }

        private static (bool valid, ChunkData chunk, uint index) NearbyBlock(uint x, uint y, uint z, uint i, uint face, bool useNeighbors, in ChunkData chunk, in ChunkData[] neighbors)
        {
            if (face == 0)
            {
                if (z == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i - ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i + ChunkData.CHUNK_SIZE_SQR);
                }
            }
            else if (face == 1)
            {
                if (z == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i + ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i - ChunkData.CHUNK_SIZE_SQR);
                }
            }
            else if (face == 2)
            {
                if (x == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i - (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i + 1);
                }
            }
            else if (face == 3)
            {
                if (x == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i + (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i - 1);
                }
            }
            else if (face == 4)
            {
                if (y == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i - ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i + ChunkData.CHUNK_SIZE);
                }
            }
            else if (face == 5)
            {
                if (y == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, null, 0);
                    }
                    else
                    {
                        return (true, neighbors[face], i + ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1));
                    }
                }
                else
                {
                    return (true, chunk, i - ChunkData.CHUNK_SIZE);
                }
            }

            return (false, null, 0);
        }
    }
}
