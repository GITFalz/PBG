using OpenTK.Mathematics;

public class BlockStorage
{
    public List<Block[]?> Blocks;
    public List<Vector3i> SubPositions;
    public int[] BlockCount;

    public BlockStorage(Vector3i position)
    {
        Blocks = new List<Block[]?>() { null, null, null, null, null, null, null, null };
        BlockCount = [0, 0, 0, 0, 0, 0, 0, 0];
        SubPositions = new List<Vector3i>()
        {
            position,
            position + new Vector3i(16, 0, 0),
            position + new Vector3i(0, 0, 16),
            position + new Vector3i(16, 0, 16),
            position + new Vector3i(0, 16, 0),
            position + new Vector3i(16, 16, 0),
            position + new Vector3i(0, 16, 16),
            position + new Vector3i(16, 16, 16),
        };
    }

    public void SetBlock(Vector3i position, Block block)
    {
        SetBlock(position.X, position.Y, position.Z, block);
    }
    
    public void SetBlock(int x, int y, int z, Block block)
    {
        int blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        int arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        if (BlockCount[arrayIndex] == 0 || Blocks[arrayIndex] == null)
            Blocks[arrayIndex] = Enumerable.Range(0, 4096).Select(_ => new Block(false, 0)).ToArray(); 
        
        Blocks[arrayIndex][blockIndex] = block;
        BlockCount[arrayIndex]++;
    }
    
    public Block GetBlock(int x, int y, int z)
    {
        int blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        int arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        Block[]? blocks = arrayIndex >= Blocks.Count ? null : Blocks[arrayIndex];
        
        if (BlockCount[arrayIndex] == 0 || blocks == null)
            return new Block(false, 0);
        
        return blocks[blockIndex];
    }
    
    public Block GetBlock(Vector3i position)
    {
        return GetBlock(position.X, position.Y, position.Z);
    }

    public Block[] GetFullBlockArray()
    {
        Block[] blocks = new Block[32768];

        int index = 0;
        for (int y = 0; y < 32; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    blocks[index] = GetBlock(x, y, z);
                    index++;
                }
            }
        }

        return blocks;
    }
    
    public void Clear()
    {
        Blocks.Clear();
    }
}