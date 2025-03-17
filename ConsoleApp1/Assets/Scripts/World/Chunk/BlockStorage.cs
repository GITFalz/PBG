using OpenTK.Mathematics;

public class BlockStorage
{
    public static BlockStorage Empty = new();

    public Block[]?[] Blocks = [];
    public Vector3i[] SubPositions = [];

    public BlockStorage() { }

    public BlockStorage(Vector3i position)
    {
        Blocks = [ null, null, null, null, null, null, null, null ];
        SubPositions =
        [
            position,
            position + new Vector3i(16, 0, 0),
            position + new Vector3i(0, 0, 16),
            position + new Vector3i(16, 0, 16),
            position + new Vector3i(0, 16, 0),
            position + new Vector3i(16, 16, 0),
            position + new Vector3i(0, 16, 16),
            position + new Vector3i(16, 16, 16),
        ];
    }

    public void SetBlock(Vector3i position, Block block)
    {
        SetBlock(position.X, position.Y, position.Z, block);
    }
    
    public void SetBlock(int x, int y, int z, Block block)
    {
        int blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        int arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        SetBlock(blockIndex, arrayIndex, block);
    }

    public void SetBlock(int blockIndex, int arrayIndex, Block block)
    {
        if (Blocks[arrayIndex] == null)
        {
            Blocks[arrayIndex] = new Block[4096];
            Array.Fill(Blocks[arrayIndex], new Block(false, 0));
        }
        
        Blocks[arrayIndex][blockIndex] = block;
    }
    
    public Block GetBlock(int x, int y, int z, out int blockIndex, out int arrayIndex)
    {
        blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        Block[]? blocks = arrayIndex >= Blocks.Length ? null : Blocks[arrayIndex];
        
        if (blocks == null)
            return new Block(false, 0);
        
        return blocks[blockIndex];
    }

    public Block GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z, out _, out _);
    }

    public Block GetBlock(Vector3i position, out int blockIndex, out int arrayIndex)
    {
        return GetBlock(position.X, position.Y, position.Z, out blockIndex, out arrayIndex);
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

    public Block this[int index]
    {
        get
        {
            int blockIndex = (index & 15) | ((index >> 1) & 0xF0) | ((index >> 2) & 0xF00);
            int arrayIndex = ((index >> 4) & 1) | ((index >> 8) & 2) | ((index >> 12) & 4);
            Block[]? array = Blocks[arrayIndex];
            return array == null ? Block.Air : array[blockIndex];
        }
        set
        {
            int blockIndex = (index & 15) | ((index >> 1) & 0xF0) | ((index >> 2) & 0xF00);
            int arrayIndex = ((index >> 4) & 1) | ((index >> 8) & 2) | ((index >> 12) & 4);
            var array = Blocks[arrayIndex];

            if (array == null)
            {   
                if (value.IsAir())
                    return;
                    
                array = new Block[4096];
                Array.Fill(array, Block.Air);
                Blocks[arrayIndex] = array;
            }   

            array = Blocks[arrayIndex];
            if (array == null)
                return;

            array[blockIndex] = value;
        }
    }
    
    public void Clear()
    {
        Blocks = [];
    }
}