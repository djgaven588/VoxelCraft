using VoxelCraft.Rendering;

namespace VoxelCraft
{
    /// <summary>
    /// A single chunk and the data related to it.
    /// Including:
    /// - Position
    /// - Current Operation
    /// - Size constants
    /// - Generated mesh
    /// - Chunk data
    /// </summary>
    public class ChunkData
    {
        public const byte CHUNK_SIZE = 32;
        public const byte CHUNK_SIZE_MINUS_ONE = CHUNK_SIZE - 1;

        public const ushort CHUNK_SIZE_SQR = CHUNK_SIZE * CHUNK_SIZE;
        public const ushort CHUNK_SIZE_CUBE = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

        public const byte CHUNK_LOG_SIZE = 5;
        public const byte CHUNK_LOG_SIZE_2 = CHUNK_LOG_SIZE * 2;

        public bool RegenerateMesh;
        public bool RegenerateLighting;

        public Coordinate ChunkPosition;
        public ChunkOperation CurrentOperation;

        public Mesh Mesh;
        public BlockData[] BlockData;
        public byte[] LightingData;

        public void ModifyBlock(Coordinate pos, BlockData data, bool positionIsWorld = false, bool regenMesh = true)
        {
            // If we are using a world position, translate to local, either way we will convert the position to an index
            BlockData[positionIsWorld ? pos.WorldToBlock().BlockToIndex() : pos.BlockToIndex()] = data;
            RegenerateMesh = RegenerateMesh || regenMesh;
            RegenerateLighting = RegenerateMesh || regenMesh;

            World.MarkNearbyChunksForRegen(ChunkPosition, positionIsWorld ? pos.WorldToBlock() : pos);
        }

        public void MarkForRegen()
        {
            RegenerateMesh = true;
            RegenerateLighting = true;
        }

        public ChunkData(Coordinate position)
        {
            RegenerateMesh = true;

            ChunkPosition = position;

            BlockData = new BlockData[CHUNK_SIZE_CUBE];
            LightingData = new byte[CHUNK_SIZE_CUBE];
        }

        public enum ChunkOperation
        {
            New,
            Unloading,
            Loading,
            Generating_Terrain,
            Terrain_Complete,
            Generating_Structures,
            Ready,
            Generating_Mesh
        }
    }

}
