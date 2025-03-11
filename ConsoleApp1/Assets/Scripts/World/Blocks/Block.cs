public struct Block
{
    // Structure of block data: |0000|00   0   0|00   00|0000  |0000|0000|0000|0000|
    //                          ^-------^ ^-^ ^----^ ^-------^ ^-------------------^
    // 6 bits, unused
    // 1 bit, state (0 = air, 1 = solid)
    // 3 bits, rotation
    // 6 bits, occlusion
    // 16 bits, block id       

    public int blockData = 0;
    public short ID => BlockId();

    public static Block Air = new Block(false, 0);

    public Block(int blockData) : this(false, blockData) { }
    public Block(bool isSolid, int blockData)
    {
        this.blockData = blockData;
        if (isSolid)
            SetSolid();
        else
            SetAir();
    }

    public short BlockId()
    {
        return (short)(blockData & 0xFFFF);
    }

    public bool Equal(Block block)
    {
        return BlockId() == block.BlockId();
    }

    public bool Equal(short blockId)
    {
        return BlockId() == blockId;
    }

    public byte Occlusion()
    {
        return (byte)((blockData >> 16) & 0x3F); // binary: 0000 0000 0011 1111
    }

    public bool Occluded(int side)
    {
        return (Occlusion() & (1 << side)) != 0;
    }

    public void SetOcclusion(int side)
    {
        blockData |= 1 << (16 + side);
    }

    public byte Rotation()
    {
        return (byte)((blockData >> 22) & 0x07); // binary: 0000 0111
    }
    
    public bool IsAir()
    {
        return (blockData & 0x02000000) == 0; // binary: 0000 0010 0000 0000 0000 0000 0000 0000
    }

    public bool IsSolid()
    {
        return !IsAir();
    }

    public void SetAir()
    {
        blockData &= ~0x02000000; // binary: 0000 0010 0000 0000 0000 0000 0000 0000 (inverted)
    }

    public void SetSolid()
    {
        blockData |= 0x02000000; // binary: 0000 0010 0000 0000 0000 0000 0000 0000
    }

    // When generating mesh
    public bool IsInvalid(Block block, int side)
    {
        return !IsSolid() || Occluded(side) || !Equal(block);
    }

    public override string ToString()
    {
        return $"State: {IsSolid()}, ID: {BlockId()}, Rotation: {Rotation()}, Occlusion: {Occlusion()}";
    }
}