using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

public class Block
{
    public int blockData;

    public Block(short blockData, byte state)
    {
        this.blockData = blockData;
    }

    public override string ToString()
    {
        return $"BlockData: {blockData}, State: {0}";
    }
}