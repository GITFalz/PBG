using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

public class Block
{
    public int blockData;
    public byte state;

    public Block(short blockData, byte state)
    {
        this.blockData = blockData;
        this.state = state;
    }

    public override string ToString()
    {
        return $"BlockData: {blockData}, State: {state}";
    }
}