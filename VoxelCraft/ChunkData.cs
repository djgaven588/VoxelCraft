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

        public Coordinate ChunkPosition;
        public ChunkOperation CurrentOperation;

        public Mesh Mesh;
        public BlockData[] Data;

        public ChunkData(Coordinate position)
        {
            RegenerateMesh = true;

            ChunkPosition = position;

            Data = new BlockData[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
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
