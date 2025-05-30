using OpenTK.Mathematics;

public class CWorldOutputNode : CWorldGetterNode
{
    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty"); 
    public int InputNodeIndex = 0;

    public CWorldOutputNode() : base() { }
    public CWorldOutputNode(string name) : base(name) { }

    public override void Init(Vector2 position)
    {
        InputNode.Init(position);
        CachedValue = InputNode.GetCachedValue(InputNodeIndex);
    }

    public override Block GetBlock(int y, int index = 0)
    {
        return InputNode.GetBlock(y, InputNodeIndex);
    }

    public override CWorldNode Copy()
    {
        return new CWorldOutputNode(Name)
        {
            InputNodeIndex = InputNodeIndex,
        };
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }
} 