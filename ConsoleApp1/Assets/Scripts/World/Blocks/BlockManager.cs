using System.Text;

public static class BlockManager
{
    public static Dictionary<int, CWorldBlock> Blocks = new Dictionary<int, CWorldBlock>();

    public static CWorldBlock GetBlock(int index)
    {
        if (Blocks.TryGetValue(index, out var block))
            return block;
        return CWorldBlock.Base;
    }

    public static bool Exists(CWorldBlock block)
    {
        return Blocks.ContainsKey(block.index);
    }
    
    public static bool Exists(int index)
    {
        return Blocks.ContainsKey(index);
    }

    public static bool Add(CWorldBlock block)
    {
        if (Exists(block))
            Blocks.Remove(block.index);

        return Blocks.TryAdd(block.index, block);
    }
    
    public static bool Add(int index)
    {
        return Blocks.TryAdd(index, new CWorldBlock(index));
    }

    public static bool SetUv(int index, int uv, int value)
    {
        if (uv >= 6 || !Blocks.TryGetValue(index, out var block))
            return false;
        
        block.SetUv(uv, value);
        return true;
    }

    public static bool SetPriority(int index, int priority)
    {
        if (!Blocks.TryGetValue(index, out var block))
            return false;

        block.priority = priority;
        return true;
    }

    public static List<CWorldBlock> GetSortedPriorityList()
    {
        List<int> sortedKeys = [.. Blocks.Keys];
        sortedKeys.Sort();

        Console.WriteLine("Sorted Array:");
        List<CWorldBlock> sortedValues = [];
        foreach (int key in sortedKeys)
        {
            sortedValues.Add(Blocks[key]);
            Console.WriteLine(Blocks[key]);
        }

        return sortedValues;
    }
}

public class CWorldBlock
{
    public static CWorldBlock Base = new();

    public string blockName;
    public int index;
    public int priority;
    public UVmaps blockUVs = UVmaps.DefaultIndexUVmap;
    public BlockChecker BlockChecker;

    public CWorldBlock()
    {
        blockName = "";
        index = 0;
        BlockChecker = new();
    }
    public CWorldBlock(int index)
    {
        blockName = "";
        this.index = index;
        BlockChecker = new();
    }
    
    public CWorldBlock(string name, int index, int priority, UVmaps blockUVs, BlockChecker blockChecker)
    {
        blockName = name;
        this.index = index;
        this.priority = priority;
        this.blockUVs = blockUVs;
        BlockChecker = blockChecker;
    }

    public void SetUv(int index, int value)
    {
        blockUVs.textureIndices[index] = value;
    }

    public int[] GetUVs()
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