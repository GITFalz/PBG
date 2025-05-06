using System.Diagnostics.CodeAnalysis;
using System.Text;

public static class BlockManager
{
    public static Dictionary<uint, CWorldBlock> Blocks = new Dictionary<uint, CWorldBlock>();
    public static List<CWorldBlock> BlockPriorityList = new List<CWorldBlock>();

    public static CWorldBlock GetBlock(uint index)
    {
        if (Blocks.TryGetValue(index, out var block))
            return block;
        return CWorldBlock.Base;
    }

    public static bool GetBlock(uint index, [NotNullWhen(true)] out CWorldBlock? block)
    {
        if (Blocks.TryGetValue(index, out block))
            return true;
            
        block = null;
        return false;
    }

    public static bool Exists(CWorldBlock block)
    {
        return Blocks.ContainsKey(block.index);
    }
    
    public static bool Exists(uint index)
    {
        return Blocks.ContainsKey(index);
    }

    public static bool Add(CWorldBlock block)
    {
        if (Exists(block))
            Blocks.Remove(block.index);

        return Blocks.TryAdd(block.index, block);
    }
    
    public static bool Add(uint index)
    {
        return Blocks.TryAdd(index, new CWorldBlock(index));
    }

    public static bool SetIndices(uint index, int uv, int value)
    {
        if (uv >= 6 || !Blocks.TryGetValue(index, out var block))
            return false;
        
        block.SetIndices(uv, value);
        return true;
    }

    public static bool SetPriority(uint index, int priority)
    {
        if (!Blocks.TryGetValue(index, out var block))
            return false;

        block.priority = priority;
        return true;
    }

    public static List<CWorldBlock> GetSortedPriorityList()
    {
        List<uint> sortedKeys = [.. Blocks.Keys];
        sortedKeys.Sort((a, b) => Blocks[a].priority.CompareTo(Blocks[b].priority));

        List<CWorldBlock> sortedValues = [];
        foreach (uint key in sortedKeys)
        {
            sortedValues.Add(Blocks[key]);
        }

        BlockPriorityList = sortedValues;
        return sortedValues;
    }
}

public class CWorldBlock
{
    public static CWorldBlock Base = new();

    public string blockName;
    public uint index;
    public int priority;
    public BlockState state;
    public UVmaps blockUVs = UVmaps.DefaultIndexUVmap;
    public BlockChecker BlockChecker;

    public CWorldBlock()
    {
        blockName = "";
        index = 0;
        BlockChecker = new();
    }
    public CWorldBlock(uint index)
    {
        blockName = "";
        this.index = index;
        BlockChecker = new();
    }
    
    public CWorldBlock(string name, uint index, int priority, BlockState blockState, UVmaps blockUVs, BlockChecker blockChecker)
    {
        blockName = name;
        this.index = index;
        this.priority = priority;
        this.state = blockState;
        this.blockUVs = blockUVs;
        BlockChecker = blockChecker;
    }

    public Block GetBlock()
    {
        return new Block(state, index);
    }

    public void SetIndices(int index, int value)
    {
        blockUVs.textureIndices[index] = value;
    }

    public int[] GetIndices()
    {
        return blockUVs.textureIndices;
    }

    public override string ToString()
    {
        return $"Block: {blockName} Index: {index} Priority: {priority}\nUvs: {blockUVs}";
    }
}

public struct UVmaps
{
    public int[] textureIndices;

    public UVmaps(int[] textureIndices)
    {
        this.textureIndices = textureIndices;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        foreach (var i in textureIndices)
        {
            stringBuilder.Append($"{i}, ");
        }
        return stringBuilder.ToString();
    }

    public static UVmaps DefaultIndexUVmap => new UVmaps(new int[] {0, 0, 0, 0, 0, 0});
}