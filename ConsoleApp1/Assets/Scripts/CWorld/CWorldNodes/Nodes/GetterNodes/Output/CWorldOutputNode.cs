using OpenTK.Mathematics;

public class CWorldOutputNode : CWorldGetterNode
{
    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty"); 

    public CWorldOutputNode() : base() { }
    public CWorldOutputNode(string name) : base(name) { }

    public override void Init(Vector2 position)
    {
        InputNode.Init(position);
        CachedValue = InputNode.CachedValue;
    }

    public override Block GetBlock(int y)
    {
        return InputNode.GetBlock(y);
    }

    public override CWorldNode Copy()
    {
        return new CWorldOutputNode(Name);
    }
} 