namespace VoxelCraft
{
    public struct BlockData
    {
        public ushort BlockID;

        // Rotation (5 bits), dont block face generation (1 bit), ignored by chunk renderer (1 bit), unused (1 bit)
        public byte ExtraData;

        public BlockData(ushort id, byte extra)
        {
            BlockID = id;
            ExtraData = extra;
        }

        public BlockData(ushort id, byte rotation, bool transparent, bool ignored)
        {
            BlockID = id;
            ExtraData = (byte)(rotation | (transparent ? 1 : 0) << 5 | (ignored ? 1 : 0) << 6);
        }
    }
}
