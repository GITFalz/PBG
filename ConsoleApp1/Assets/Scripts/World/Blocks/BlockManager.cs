namespace ConsoleApp1.Assets.Scripts.World.Blocks;

public class BlockManager
{
    public static Dictionary<int, BlockData> blocks = new Dictionary<int, BlockData>();

    public void Init()
    {
        if (blocks.Count > 0)
            return;
        
        blocks.Add(0, new BlockData()
        {
            id = 0,
            name = "Grass",
            blockType = BlockType.GRASS
        });
        
        blocks.Add(1, new BlockData()
        {
            id = 1,
            name = "Dirt",
            blockType = BlockType.DIRT
        });
        
        blocks.Add(2, new BlockData()
        {
            id = 2,
            name = "Stone",
            blockType = BlockType.STONE
        });
        
        blocks.Add(-1, new BlockData()
        {
            id = -1,
            name = "Air",
            blockType = BlockType.AIR
        });
    }

    public void ReInit()
    {
        blocks.Clear();
        Init();
    }
    
    public static BlockData GetBlockData(int id)
    {
        return !blocks.TryGetValue(id, out var data) ? blocks[-1] : data;
    }
}