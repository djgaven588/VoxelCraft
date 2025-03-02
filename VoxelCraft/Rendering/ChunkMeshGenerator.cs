﻿using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class ChunkMeshGenerator
    {
        private readonly ChunkBlockVertexData[] VertexBuffer = new ChunkBlockVertexData[ChunkData.CHUNK_SIZE_CUBE * 6 * 4];
        private readonly uint[] IndiciesBuffer = new uint[ChunkData.CHUNK_SIZE_CUBE * 12];

        private uint vertexCount = 0;
        private uint indiciesCount = 0;

        public void RunJob(ref ChunkData chunk, in ChunkData[] neighbors)
        {
            vertexCount = 0;
            indiciesCount = 0;

            uint blockPosX;
            uint blockPosY;
            uint blockPosZ;

            bool useNeighbors = neighbors != null && neighbors.Length == 6;

            //ChunkLightingGenerator.GenerateLighting(chunk, neighbors);

            for (uint i = 0; i < chunk.BlockData.Length; i++)
            {
                if ((chunk.BlockData[i].ExtraData >> 6 & 1) == 1 || chunk.BlockData[i].BlockID >= BlockDatabase.BlockTextures.Length)
                {
                    continue;
                }

                blockPosX = i & ChunkData.CHUNK_SIZE_MINUS_ONE;
                blockPosY = i >> ChunkData.CHUNK_LOG_SIZE & ChunkData.CHUNK_SIZE_MINUS_ONE;
                blockPosZ = i >> ChunkData.CHUNK_LOG_SIZE_2;

                for (uint k = 0; k < 6; k++)
                {
                    (bool validBlock, BlockData neighborDat, byte neighborLight) = NearbyBlock(blockPosX, blockPosY, blockPosZ, i, k, useNeighbors, chunk, neighbors);

                    if(!validBlock || (neighborDat.ExtraData >> 5 & 1) == 0)
                    {
                        continue;
                    }

                    IndiciesBuffer[indiciesCount++] = vertexCount;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 1;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 2;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 2;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 3;
                    IndiciesBuffer[indiciesCount++] = vertexCount;

                    uint lighting = (k == 4 ? 3u : k == 5 ? 1u : 2u);
                    uint realLighting = (uint)(neighborLight < 16 ? neighborLight : 15);

                    for (uint j = 0; j < 4; j++)
                    {
                        // X, Y, Z (6 bits each), lighting (2 bits), vertex index (2 bits, for texture mapping), texture index
                        VertexBuffer[vertexCount].Data = (Face_Data[(k * 4) + j].X + blockPosX)
                                                           | ((Face_Data[(k * 4) + j].Y + blockPosY) << 6)
                                                           | ((Face_Data[(k * 4) + j].Z + blockPosZ) << 12)
                                                           | (lighting << 18) //(k << 18)
                                                           | (j << 20)
                                                           | ((uint)BlockDatabase.BlockTextures[chunk.BlockData[i].BlockID].Textures[k] << 22);

                        VertexBuffer[vertexCount++].Lighting = realLighting;
                    }
                }
            }
        }

        private static (bool valid, BlockData data, byte lighting) NearbyBlock(uint x, uint y, uint z, uint i, uint face, bool useNeighbors, in ChunkData chunk, in ChunkData[] neighbors)
        {
            if(face == 0)
            {
                if(z == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if(!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i - ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i - ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i + ChunkData.CHUNK_SIZE_SQR], chunk.LightingData[i + ChunkData.CHUNK_SIZE_SQR]);
                }
            }
            else if (face == 1)
            {
                if (z == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i + ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i + ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i - ChunkData.CHUNK_SIZE_SQR], chunk.LightingData[i - ChunkData.CHUNK_SIZE_SQR]);
                }
            }
            else if (face == 2)
            {
                if (x == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i - (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i - (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i + 1], chunk.LightingData[i + 1]);
                }
            }
            else if (face == 3)
            {
                if (x == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i + (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i + (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i - 1], chunk.LightingData[i - 1]);
                }
            }
            else if (face == 4)
            {
                if (y == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i - ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i - ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i + ChunkData.CHUNK_SIZE], chunk.LightingData[i + ChunkData.CHUNK_SIZE]);
                }
            }
            else if (face == 5)
            {
                if (y == 0)
                {
                    if (!useNeighbors || neighbors[face] == null)
                    {
                        return (false, default(BlockData), 0);
                    }
                    else
                    {
                        return (true, neighbors[face].BlockData[i + ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)], neighbors[face].LightingData[i + ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)]);
                    }
                }
                else
                {
                    return (true, chunk.BlockData[i - ChunkData.CHUNK_SIZE], chunk.LightingData[i - ChunkData.CHUNK_SIZE]);
                }
            }

            return (false, default(BlockData), 0);
        }

        public void FinishMeshGeneration(Mesh mesh)
        {
            mesh.UploadMeshData(VertexBuffer, (int)vertexCount, IndiciesBuffer, (int)indiciesCount);
        }

        private static readonly CoordinateUint[] Face_Data = new CoordinateUint[]
        {
        // North
                new CoordinateUint(1, 0, 1),
                new CoordinateUint(1, 1, 1),
                new CoordinateUint(0, 1, 1),
                new CoordinateUint(0, 0, 1),
        // South
                new CoordinateUint(0, 0, 0),
                new CoordinateUint(0, 1, 0),
                new CoordinateUint(1, 1, 0),
                new CoordinateUint(1, 0, 0),

        // East
                new CoordinateUint(1, 0, 0),
                new CoordinateUint(1, 1, 0),
                new CoordinateUint(1, 1, 1),
                new CoordinateUint(1, 0, 1),

        // West
                new CoordinateUint(0, 0, 1),
                new CoordinateUint(0, 1, 1),
                new CoordinateUint(0, 1, 0),
                new CoordinateUint(0, 0, 0),

        // Top
                new CoordinateUint(0, 1, 0),
                new CoordinateUint(0, 1, 1),
                new CoordinateUint(1, 1, 1),
                new CoordinateUint(1, 1, 0),

        // Bottom
                new CoordinateUint(1, 0, 0),
                new CoordinateUint(1, 0, 1),
                new CoordinateUint(0, 0, 1),
                new CoordinateUint(0, 0, 0)
        };
    }
}
