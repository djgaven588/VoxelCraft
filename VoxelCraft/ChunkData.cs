using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class ChunkData
    {
        public const byte CHUNK_SIZE = 32;
        public const ushort CHUNK_SIZE_CUBE = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const byte CHUNK_SIZE_MINUS_ONE = CHUNK_SIZE - 1;
        public const byte CHUNK_LOG_SIZE = 5;
        public const byte CHUNK_LOG_SIZE_2 = CHUNK_LOG_SIZE * 2;

        public bool IsDirty;
        public ChunkOperation CurrentChunkOperation;

        public enum ChunkOperation
        {
            None,
            Generating,
            Loading,
            Unloading
        }

        public bool StructuresGenerated;

        public Mesh GeneratedMesh;

        public Coordinate ChunkPosition;

        public BlockData[] BlockData;

        public ChunkData(Coordinate position)
        {
            IsDirty = true;

            ChunkPosition = position;

            BlockData = new BlockData[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        }
    }

}
