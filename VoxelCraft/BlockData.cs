namespace VoxelCraft
{
    public struct BlockData
    {
        public ushort BlockID;

        // Rotation (5 bits), dont block face generation (1 bit), ignored by chunk renderer (1 bit), unused (1 bit)
        public byte ExtraData;
    }
}
