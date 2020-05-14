using System;

namespace VoxelCraft
{
    public static class ChunkStructureGenerator
    {
        public static void GenerateStructures(ref ChunkData chunk, in ChunkData[] neighbors)
        {
            Random random = new Random(chunk.ChunkPosition.GetDeterministicHashcode());
            for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    if (random.Next(0, 100) == 0)
                    {
                        bool groundBlockFound = false;
                        for (int y = 0; y < ChunkData.CHUNK_SIZE; y++)
                        {
                            int location = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE_SQR;
                            if(groundBlockFound && chunk.Data[location].BlockID == 0)
                            {
                                int height = random.Next(4, 7);
                                for (int i = 0; i < height + 1; i++)
                                {
                                    if (i < height - 1)
                                    {
                                        PlaceBlock(true, 3, 0, x, y + i, z, neighbors, chunk);
                                    }

                                    if(i >= height - 2)
                                    {
                                        for (int xOff = -2; xOff <= 2; xOff++)
                                        {
                                            for (int zOff = -2; zOff <= 2; zOff++)
                                            {
                                                PlaceBlock(true, 4, 1 << 5, x + xOff, y + i, z + zOff, neighbors, chunk);
                                            }
                                        }
                                    }
                                }
                                break;
                            }

                            groundBlockFound = chunk.Data[location].BlockID == 1;
                        }
                    }
                }
            }
        }

        private static void PlaceBlock(bool destructive, ushort id, byte extra, int x, int y, int z, ChunkData[] neighbors, ChunkData self)
        {
            CorrectCoordinates(ref x, ref y, ref z, out int neighbor);

            ChunkData chunkToModify = neighbor == 13 ? self : neighbors[neighbor];

            if (chunkToModify == null)
            {
                return;
            }

            int index = x + y * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE_SQR;
            if (destructive || (!destructive && chunkToModify.Data[index].BlockID == 0))
            {
                chunkToModify.Data[index].BlockID = id;
                chunkToModify.Data[index].ExtraData = extra;
                chunkToModify.RegenerateMesh = true;
            }
        }

        private static void CorrectCoordinates(ref int x, ref int y, ref int z, out int neighborIndex)
        {
            Coordinate chunkOffset = new Coordinate(1, 1, 1);
            if (x < 0)
            {
                x += ChunkData.CHUNK_SIZE;
                chunkOffset.X--;
            }
            else if (x > ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                x -= ChunkData.CHUNK_SIZE;
                chunkOffset.X++;
            }

            if (y < 0)
            {
                y += ChunkData.CHUNK_SIZE;
                chunkOffset.Y--;
            }
            else if (y > ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                y -= ChunkData.CHUNK_SIZE;
                chunkOffset.Y++;
            }

            if (z < 0)
            {
                z += ChunkData.CHUNK_SIZE;
                chunkOffset.Z--;
            }
            else if (z > ChunkData.CHUNK_SIZE_MINUS_ONE)
            {
                z -= ChunkData.CHUNK_SIZE;
                chunkOffset.Z++;
            }

            neighborIndex = chunkOffset.X + chunkOffset.Y * 3 + chunkOffset.Z * 9;
        }
    }
}
