using System.Diagnostics.CodeAnalysis;

public struct Block
{
    // Structure of block data: |0000|00   0   0|00   00|0000  |0000|0000|0000|0000|
    //                          ^-------^ ^-^ ^----^ ^-------^ ^-------------------^
    // 6 bits, unused
    // 1 bit, state (0 = air, 1 = solid) (to be expanded)
    // 3 bits, rotation
    // 6 bits, occlusion
    // 16 bits, block id       

    public int blockData = 0;
    public short ID => BlockId();

    public static Block Air = new Block(false, 0);
    public static Block Solid = new Block(true, 0);

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
        return (short)(blockData & 0x0000FFFF);
    }

    public void SetBlockId(short id)
    {
        blockData |= (int)id;
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

    public void ResetOcclusion()
    {
        blockData &= ~0x3F0000;
    }

    public bool Occluded(int side)
    {
        return (Occlusion() & (1 << side)) != 0;
    }

    public bool FullyOccluded()
    {
        return (blockData & 0x3F0000) == 0x3F0000; // binary: 0011 1111 0000 0000 0000 0000
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

    public byte State()
    {
        return (byte)((blockData >> 25) & 7);
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

    public string ToBits()
    {
        return Convert.ToString(blockData, 2).PadLeft(32, '0');
    }

    public override string ToString()
    {
        return $"State: {IsSolid()}, ID: {BlockId()}, Rotation: {Rotation()}, Occlusion: {Occlusion()}";
    }
}