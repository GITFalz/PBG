using System.Diagnostics.CodeAnalysis;

public struct Block
{
    // Structure of block data: |0000|0   00   0|00   00|0000  |0000|0000|0000|0000|
    //                          ^------^ ^--^ ^----^ ^-------^ ^-------------------^
    // 5 bits, unused
    // 2 bit, state (0 = air, 1 = solid, 2 = liquid) (to be expanded)
    // 3 bits, rotation
    // 6 bits, occlusion
    // 16 bits, block id       

    public const uint ID_MASK = 0x0000FFFF;         // binary: 0000 0000 0000 0000 1111 1111 1111 1111
    public const uint OCCLUSION_MASK = 0x003F0000;  // binary: 0000 0000 0011 1111 0000 0000 0000 0000
    public const uint ROTATION_MASK = 0x01C00000;   // binary: 0000 0001 1100 0000 0000 0000 0000 0000
    public const uint STATE_MASK = 0x06000000;      // binary: 0000 0110 0000 0000 0000 0000 0000 0000

    public static Block Air = new Block(BlockState.Air, 0);
    public static Block Solid = new Block(BlockState.Solid, 0);
    public static Block Liquid = new Block(BlockState.Liquid, 0);

    public uint blockData = 0;
    public uint ID => BlockId();

    public Block(uint blockData) : this(BlockState.Air, blockData) { }
    public Block(BlockState blockState, uint blockData)
    {
        this.blockData = blockData;
        switch (blockState)
        {
            case BlockState.Air:
                SetAir();
                break;
            case BlockState.Solid:
                SetSolid();
                break;
            case BlockState.Liquid:
                SetLiquid();
                break;
        }
    }

    public uint BlockId()
    {
        return blockData & ID_MASK; // 0b 0000 0000 0000 0000 1111 1111 1111 1111
    }

    public void SetBlockId(ushort id)
    {
        blockData = (blockData & ~ID_MASK) | id; // 0b 0000 0000 0000 0000 1111 1111 1111 1111
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
        return (byte)((blockData & OCCLUSION_MASK) >> 16);
    }

    public void ResetOcclusion()
    {
        blockData &= ~OCCLUSION_MASK;
    }

    public bool Occluded(int side)
    {
        return (Occlusion() & (1 << side)) != 0;
    }

    public bool FullyOccluded()
    {
        return (blockData & OCCLUSION_MASK) == OCCLUSION_MASK;
    }

    public void SetOcclusion(int side)
    {
        blockData |= 1u << (16 + side);
    }

    public byte Rotation()
    {
        return (byte)((blockData & ROTATION_MASK) >> 22);
    }

    public uint State()
    {
        return (blockData & STATE_MASK) >> 25;
    }
    
    public bool IsAir()
    {
        return State() == 0;
    }

    public bool IsSolid()
    {
        return State() == 1;
    }

    public bool IsLiquid()
    {
        return State() == 2;
    }

    public void SetAir()
    {
        blockData = blockData & ~STATE_MASK; // 0b 0000 0000 0000 0000 0000 0000 0000 0000
    }

    public void SetSolid()
    {
        blockData = (blockData & ~STATE_MASK) | 0x02000000; // 0b 0000 0010 0000 0000 0000 0000 0000 0000
    }

    public void SetLiquid()
    {
        blockData = (blockData & ~STATE_MASK) | 0x04000000; // 0b 0000 0100 0000 0000 0000 0000 0000 0000
    }

    public override string ToString()
    {
        return $"Block: {BlockId()}, State: {State()}, Occlusion: {Occlusion()}, Rotation: {Rotation()}";
    }
}

public enum BlockState
{
    Air = 0,
    Solid = 1,
    Liquid = 2,
}