namespace VoxelCraft
{
    public class Region
    {
        public const byte REGION_SIZE = 8;
        public const byte RegionSize_2 = REGION_SIZE * REGION_SIZE;
        public const ushort RegionSize_3 = REGION_SIZE * REGION_SIZE * REGION_SIZE;
        public const byte REGION_LOG_SIZE = 3;

        public Coordinate Position;
        public ChunkData[] StoredChunks;
    }
}
