public class BlockManager
{
    public static Dictionary<int, CWorldBlock> Blocks = new Dictionary<int, CWorldBlock>();

    public static CWorldBlock GetBlock(int index)
    {
        return Blocks.GetValueOrDefault(index);
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
}

[System.Serializable]
public class CWorldBlock
{
    public string blockName;
    public int index;
    public int priority;
    public UVmaps blockUVs = UVmaps.DefaultIndexUVmap;

    public CWorldBlock()
    {
        blockName = "";
        index = 0;
    }
    public CWorldBlock(int index)
    {
        blockName = "";
        this.index = index;
    }
    
    public CWorldBlock(string name, int index, int priority, UVmaps blockUVs)
    {
        blockName = name;
        this.index = index;
        this.priority = priority;
        this.blockUVs = blockUVs;
    }

    public void SetUv(int index, int value)
    {
        blockUVs.textureIndices[index] = value;
    }

    public int[] GetUVs()
    {
        return blockUVs.textureIndices;
    }
}

public struct UVmaps
{
    public int[] textureIndices;

    public UVmaps(int[] textureIndices)
    {
        this.textureIndices = textureIndices;
    }

    public static UVmaps DefaultIndexUVmap => new UVmaps(new int[] {0, 0, 0, 0, 0, 0});
}