using System.Runtime.CompilerServices;
using System.Threading;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class ChunkMeshGenerator
    {
        private readonly ChunkBlockVertexData[] VertexBuffer = new ChunkBlockVertexData[ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * 6 * 4];
        private readonly uint[] IndiciesBuffer = new uint[ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * 12];

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
                    if ((k == 0 && !(blockPosZ == ChunkData.CHUNK_SIZE - 1 ?
                                (!useNeighbors || neighbors[0] == null || (useNeighbors && (neighbors[0].BlockData[i - ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i + ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE].ExtraData >> 5 & 1) == 1)) ||

                       (k == 1 && !(blockPosZ == 0 ?
                                (!useNeighbors || neighbors[1] == null || (useNeighbors && (neighbors[1].BlockData[i + ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i - ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE].ExtraData >> 5 & 1) == 1)) ||

                        (k == 2 && !(blockPosX == ChunkData.CHUNK_SIZE - 1 ?
                                (!useNeighbors || neighbors[2] == null || (useNeighbors && (neighbors[2].BlockData[i - (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i + 1].ExtraData >> 5 & 1) == 1)) ||

                        (k == 3 && !(blockPosX == 0 ?
                                (!useNeighbors || neighbors[3] == null || (useNeighbors && (neighbors[3].BlockData[i + (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i - 1].ExtraData >> 5 & 1) == 1)) ||

                        (k == 4 && !(blockPosY == ChunkData.CHUNK_SIZE - 1 ?
                                (!useNeighbors || neighbors[4] == null || (useNeighbors && (neighbors[4].BlockData[i - ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i + ChunkData.CHUNK_SIZE].ExtraData >> 5 & 1) == 1)) ||

                        (k == 5 && !(blockPosY == 0 ?
                                (!useNeighbors || neighbors[5] == null || (useNeighbors && (neighbors[5].BlockData[i + ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1)].ExtraData >> 5 & 1) == 1)) :
                                (chunk.BlockData[i - ChunkData.CHUNK_SIZE].ExtraData >> 5 & 1) == 1)))
                    {
                        continue;
                    }

                    IndiciesBuffer[indiciesCount++] = vertexCount;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 1;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 2;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 2;
                    IndiciesBuffer[indiciesCount++] = vertexCount + 3;
                    IndiciesBuffer[indiciesCount++] = vertexCount;


                    for (uint j = 0; j < 4; j++)
                    {
                        // X, Y, Z (6 bits each), normal index (3 bits), vertex index (2 bits, for texture mapping), texture index
                        VertexBuffer[vertexCount++].Data =    (Face_Data[(k * 4) + j].X + blockPosX)
                                                           | ((Face_Data[(k * 4) + j].Y + blockPosY) << 6)
                                                           | ((Face_Data[(k * 4) + j].Z + blockPosZ) << 12)
                                                           | (k << 18)
                                                           | (j << 21)
                                                           | ((uint)BlockDatabase.BlockTextures[chunk.BlockData[i].BlockID].Textures[k] << 23);
                    }
                }
            }
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
