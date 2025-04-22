public class CWorldOutputNode : CWorldGetterNode
{
    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty"); 

    public CWorldOutputNode() : base() { }
    public CWorldOutputNode(string name) : base(name) { }

    public override float GetValue()
    {
        return InputNode.GetValue();
    }

    public override CWorldNode Copy()
    {
        return new CWorldOutputNode(Name);
    }
} 