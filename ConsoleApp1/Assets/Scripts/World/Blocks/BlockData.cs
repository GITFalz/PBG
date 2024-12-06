namespace ConsoleApp1.Assets.Scripts.World.Blocks;

public class BlockData
{
    public int id;
    public string name;
    public BlockType blockType;
}

public enum BlockType
{
    GRASS,
    DIRT,
    STONE,
    AIR,
}