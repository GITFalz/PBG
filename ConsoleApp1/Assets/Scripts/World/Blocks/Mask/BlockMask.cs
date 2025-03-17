using OpenTK.Mathematics;

public class BlockChecker
{
    public BlockMask[] BlockMasks;

    public BlockChecker(params BlockMask[] blockMasks)
    {
        BlockMasks = blockMasks;
    }

    public static readonly Dictionary<MaskType, Func<Block, Block, bool>> BlockTests = new Dictionary<MaskType, Func<Block, Block, bool>>
    {
        { MaskType.Id, (b1, b2) => b1.Equal(b2) },
        { MaskType.State, (b1, b2) => b1.State() == b2.State() },
    };
}

public struct BlockMask
{
    public Vector3i Offset;
    public BlockTest[] Blocks;

    public BlockMask(Vector3i offset, params BlockTest[] blocks)
    {
        Offset = offset;
        Blocks = blocks;
    }

    public bool IsValid(Block block)
    {
        if (Blocks.Length == 0)
            return true;
            
        foreach (var b in Blocks)
        {
            if (b.IsValid(block))
                return true;
        }
        return false;
    }
}

public struct BlockTest
{
    public Block Block;
    public MaskType Type;

    public BlockTest(Block block, MaskType type)
    {
        Block = block; Type = type;
    }

    public bool IsValid(Block block)
    {
        return BlockChecker.BlockTests[Type](Block, block);
    }
}

public enum MaskType
{
    Id,
    State,
}