using OpenTK.Mathematics;

public abstract class BlockStorage
{
    public abstract Block this[int index] { get; set; }

    public abstract void SetBlock(Vector3i position, Block block);
    public abstract void SetBlock(int x, int y, int z, Block block);
    public abstract Block GetRemoveBlock(Vector3i position, Block block);
    public abstract Block GetBlock(Vector3i position);
    public abstract Block GetBlock(int x, int y, int z);

    public abstract void Clear();
}

public class FullBlockStorage : BlockStorage
{
    public static FullBlockStorage Empty = new();

    public Block[]? Blocks = null;

    public override Block this[int index]
    {
        get
        {
            return Blocks == null ? Block.Air : Blocks[index];
        }
        set
        {
            if (Blocks == null)
            {   
                if (value.IsAir())
                    return;
                    
                Blocks = new Block[32768];
                Array.Fill(Blocks, Block.Air);
            }   

            Blocks[index] = value;
        }
    }

    public override void SetBlock(Vector3i position, Block block)
    {
        SetBlock(position.X, position.Y, position.Z, block);
    }

    public override void SetBlock(int x, int y, int z, Block block)
    {
        int index = (x & 31) + (z & 31) * 32 + (y & 31) * 1024;
        this[index] = block;
    }

    public override Block GetRemoveBlock(Vector3i position, Block block)
    {
        int index = GetIndex(position.X, position.Y, position.Z);
        Block oldBlock = this[index];
        this[index] = block;
        return oldBlock;
    }

    public override Block GetBlock(Vector3i position)
    {
        return GetBlock(position.X, position.Y, position.Z);
    }

    public override Block GetBlock(int x, int y, int z)
    {
        int index = (x & 31) + (z & 31) * 32 + (y & 31) * 1024;
        return this[index];
    }

    public int GetIndex(int x, int y, int z)
    {
        return (x & 31) + (z & 31) * 32 + (y & 31) * 1024;
    }

    public override void Clear()
    {
        Blocks = null;
    }
}

public class CornerBlockStorage : BlockStorage
{
    public static CornerBlockStorage Empty = new();

    public Block[]?[] Blocks;
    public Vector3i[] SubPositions;

    public CornerBlockStorage() 
    { 
        Blocks = []; 
        SubPositions = []; 
    }

    public CornerBlockStorage(Vector3i position)
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

    public void SetPosition(Vector3i position)
    {
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

    public override void SetBlock(Vector3i position, Block block)
    {
        SetBlock(position.X, position.Y, position.Z, block);
    }
    
    public override void SetBlock(int x, int y, int z, Block block)
    {
        int blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        int arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        SetBlock(blockIndex, arrayIndex, block);
    }

    public void SetBlock(int blockIndex, int arrayIndex, Block block)
    {
        lock (this)
        {
            if (Blocks[arrayIndex] == null)
            {
                Blocks[arrayIndex] = new Block[4096];
                Array.Fill(Blocks[arrayIndex], new Block(BlockState.Air, 0));
            }
            
            Blocks[arrayIndex][blockIndex] = block;
        }
    }

    public override Block GetRemoveBlock(Vector3i position, Block block)
    {
        Block oldBlock = GetBlock(position.X, position.Y, position.Z, out int blockIndex, out int arrayIndex);
        SetBlock(blockIndex, arrayIndex, block);
        return oldBlock;
    }
    
    public Block GetBlock(int x, int y, int z, out int blockIndex, out int arrayIndex)
    {
        blockIndex = (x & 15) + (z & 15) * 16 + (y & 15) * 256;
        arrayIndex = (x >> 4) + (z >> 4) * 2 + (y >> 4) * 4;
        
        Block[]? blocks = arrayIndex >= Blocks.Length ? null : Blocks[arrayIndex];
        
        if (blocks == null)
            return new Block(BlockState.Air, 0);
        
        return blocks[blockIndex];
    }

    public override Block GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z, out _, out _);
    }

    public Block GetBlock(Vector3i position, out int blockIndex, out int arrayIndex)
    {
        return GetBlock(position.X, position.Y, position.Z, out blockIndex, out arrayIndex);
    }
    
    public override Block GetBlock(Vector3i position)
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

    public override Block this[int index]
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
    
    public override void Clear()
    {
        Blocks = [ null, null, null, null, null, null, null, null ];
    }
}