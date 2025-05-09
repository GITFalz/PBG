using OpenTK.Mathematics;

public class CWorldEmptyNode : CWorldGetterNode
{
    public float Value = 0f;

    public CWorldEmptyNode() : base() { }
    public CWorldEmptyNode(string name) : base(name) { }
    public CWorldEmptyNode(float value) : base() { Value = value; }

    public override void SetValue(float value)
    {
        Value = value; CachedValue = value;
    }

    public override void Init(Vector2 position)
    {
        CachedValue = Value;
    }

    public override Block GetBlock(int y)
    {
        return Block.Air;
    }

    public override CWorldNode Copy()
    {
        return new CWorldEmptyNode()
        {
            Name = Name,
            Value = Value,
        };;
    }
}