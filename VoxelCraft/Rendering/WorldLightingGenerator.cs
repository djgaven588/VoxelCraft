using System.Collections.Concurrent;

namespace VoxelCraft.Rendering
{
    public class WorldLightingGenerator
    {
        private static ConcurrentQueue<PropogationEntry> _propagationPoints = new ConcurrentQueue<PropogationEntry>();
        private static ConcurrentQueue<PropogationEntry> _priorityPropagationPoints = new ConcurrentQueue<PropogationEntry>();
        private static ConcurrentQueue<PropogationEntry> _unpropagationPoints = new ConcurrentQueue<PropogationEntry>();

        public static bool ShouldSendLighting => _priorityPropagationPoints.Count + _propagationPoints.Count + _unpropagationPoints.Count < 4096 || UpdatesSinceLastSend > 1000000;
        public static bool ShouldAddNewChunk => _priorityPropagationPoints.Count <= 128 * ChunkOperationDispatcher.MAX_LIGHT_GENERATORS && (_propagationPoints.Count + _unpropagationPoints.Count <= 512 * ChunkOperationDispatcher.MAX_LIGHT_GENERATORS);

        public static int UpdatesSinceLastSend = 0;

        public const int NormalSpreadCost = 1;
        public const int ExtraSpreadCost = 3;
        public const int SunlightLevel = 15;

        private struct PropogationEntry
        {
            public Coordinate Chunk;
            public ChunkData Data;
            public ushort Index;
            public byte Value;
            public byte X;
            public byte Y;
            public byte Z;
            public bool Sunlight;
        }

        public static void RunLighting(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                if(_unpropagationPoints.TryDequeue(out PropogationEntry unPropEntry))
                {
                    UnpropogateLight(unPropEntry);
                }
                else if (_priorityPropagationPoints.TryDequeue(out PropogationEntry priorityProp))
                {
                    PropogateLight(priorityProp);
                }
                else if (_propagationPoints.TryDequeue(out PropogationEntry propEntry))
                {
                    PropogateLight(propEntry);
                }
                else
                {
                    break;
                }
            }
        }

        public static void StartLightForChunk(ChunkData chunk)
        {
            if(chunk.ChunkPosition.Y != Region.REGION_SIZE - 1)
            {
                return;
            }

            for (byte x = 0; x < ChunkData.CHUNK_SIZE; x++)
            {
                for (byte z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    _priorityPropagationPoints.Enqueue(new PropogationEntry() { X = x, Y = ChunkData.CHUNK_SIZE_MINUS_ONE, Z = z, Data = chunk, Chunk = chunk.ChunkPosition, Index = (ushort)(x + ChunkData.CHUNK_SIZE_MINUS_ONE * ChunkData.CHUNK_SIZE + z * ChunkData.CHUNK_SIZE_SQR), Sunlight = true, Value = SunlightLevel });
                }
            }
        }

        private static void PropogateLight(PropogationEntry entry)
        {
            (ChunkData chunk, BlockData data, ushort index, byte x, byte y, byte z, byte light) = GetBlock(entry, 5);

            int newLightValue = entry.Value - NormalSpreadCost;
            int newLightExtraCost = entry.Value - ExtraSpreadCost;

            {
                if (((data.ExtraData >> 5 & 3) == 0) || chunk == null)
                {
                    return;
                }

                bool isExtraCost = (data.ExtraData >> 6 & 1) == 0;
                int newValue = (isExtraCost ? newLightExtraCost : newLightValue);

                if (light <= newValue)
                {
                    bool isSunlight = entry.Sunlight && (data.ExtraData >> 6 & 1) == 1;
                    newValue = !isSunlight ? newValue : entry.Value;
                    chunk.LightingData[index] = (byte)newValue;
                    chunk.LightUpdateRequired = true;
                    if (newValue > (isExtraCost ? ExtraSpreadCost : NormalSpreadCost))
                    {
                        _priorityPropagationPoints.Enqueue(new PropogationEntry() { X = x, Y = y, Z = z, Data = chunk, Value = (byte)newValue, Sunlight = isSunlight, Chunk = chunk.ChunkPosition, Index = index });
                    }

                    UpdatesSinceLastSend++;
                }
            }

            for (byte i = 0; i < 5; i++)
            {
                (chunk, data, index, x, y, z, light) = GetBlock(entry, i);
                Handle();
            }

            void Handle()
            {
                if (((data.ExtraData >> 5 & 3) == 0) || chunk == null)
                {
                    return;
                }

                bool isExtraCost = (data.ExtraData >> 6 & 1) == 0;
                int newValue = (isExtraCost ? newLightExtraCost : newLightValue);

                if (light < newValue)
                {
                    chunk.LightingData[index] = (byte)newValue;
                    chunk.LightUpdateRequired = true;

                    if (newValue > (isExtraCost ? ExtraSpreadCost : NormalSpreadCost))
                    {
                        PropogationEntry ent = new PropogationEntry() { X = x, Y = y, Z = z, Data = chunk, Value = (byte)newValue, Sunlight = false, Chunk = chunk.ChunkPosition, Index = index };
                        _propagationPoints.Enqueue(ent);
                    }

                    UpdatesSinceLastSend++;
                }
            }
        }

        private static void UnpropogateLight(PropogationEntry entry)
        {

        }

        private static (ChunkData, BlockData, ushort, byte, byte, byte, byte) GetBlock(PropogationEntry entry, byte side)
        {
            (Coordinate coord, int index, int x, int y, int z) = GetNeighborBlock(side, entry.Chunk, entry.Index, entry.X, entry.Y, entry.Z);
            ChunkData chunk = entry.Data;
            if (chunk != null && coord == chunk.ChunkPosition)
            {
                return (chunk, chunk.BlockData[index], (ushort)index, (byte)x, (byte)y, (byte)z, chunk.LightingData[index]);
            }
            else if(World.LoadedChunks.TryGetValue(coord, out chunk))
            {
                return (chunk, chunk.BlockData[index], (ushort)index, (byte)x, (byte)y, (byte)z, chunk.LightingData[index]);
            }
            else
            {
                return (null, default, 0, 0, 0, 0, 0);
            }
        }

        private static (Coordinate coord, int index, int x, int y, int z) GetNeighborBlock(uint side, Coordinate coord, ushort i, byte x, byte y, byte z)
        {
            if (side == 0)
            {
                if (z == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    return (coord + Coordinate.Forward, i - (ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1)), x, y, 0);
                }
                else
                {
                    return (coord, i + ChunkData.CHUNK_SIZE_SQR, x, y, z + 1);
                }
            }
            else if (side == 1)
            {
                if (z == 0)
                {
                    return (coord - Coordinate.Forward, i + ChunkData.CHUNK_SIZE_SQR * (ChunkData.CHUNK_SIZE - 1), x, y, ChunkData.CHUNK_SIZE_MINUS_ONE);
                }
                else
                {
                    return (coord, i - ChunkData.CHUNK_SIZE_SQR, x, y, z - 1);
                }
            }
            else if (side == 2)
            {
                if (x == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    return (coord - Coordinate.Right, i - (ChunkData.CHUNK_SIZE - 1), 0, y, z);
                }
                else
                {
                    return (coord, i + 1, x + 1, y, z);
                }
            }
            else if (side == 3)
            {
                if (x == 0)
                {
                    return (coord + Coordinate.Right, i + (ChunkData.CHUNK_SIZE - 1), ChunkData.CHUNK_SIZE_MINUS_ONE, y, z);
                }
                else
                {
                    return (coord, i - 1, x - 1, y, z);
                }
            }
            else if (side == 4)
            {
                if (y == ChunkData.CHUNK_SIZE_MINUS_ONE)
                {
                    return (coord + Coordinate.Up, i - ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1), x, 0, z);
                }
                else
                {
                    return (coord, i + ChunkData.CHUNK_SIZE, x, y + 1, z);
                }
            }
            else if (side == 5)
            {
                if (y == 0)
                {
                    return (coord - Coordinate.Up, i + ChunkData.CHUNK_SIZE * (ChunkData.CHUNK_SIZE - 1), x, ChunkData.CHUNK_SIZE_MINUS_ONE, z);
                }
                else
                {
                    return (coord, i - ChunkData.CHUNK_SIZE, x, y - 1, z);
                }
            }

            return (coord, i, 0, 0, 0);
        }
    }
}
