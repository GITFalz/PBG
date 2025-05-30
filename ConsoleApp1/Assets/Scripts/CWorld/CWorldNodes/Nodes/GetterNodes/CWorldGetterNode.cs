using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public abstract class CWorldGetterNode : CWorldNode
{
    protected float CachedValue = 0;

    public CWorldGetterNode() : base() { }
    public CWorldGetterNode(string name) : base(name) { }
    public abstract float GetCachedValue(int index);
    public virtual void SetValue(float value) { } // Only used for the empty node

    /// <summary>
    /// If no range node has been assigned, it will default to this
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public virtual Block GetBlock(int y, int index = 0)
    {
        return y <= Mathf.Lerp(0, 120, GetCachedValue(index)) ? Block.Stone : Block.Air;
    }

    public virtual bool GetBlock(int y, [NotNullWhen(true)] out Block block, int index = 0)
    {
        block = GetBlock(y, index);
        return true;
    }
} 