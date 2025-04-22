public class CWorldEmptyNode : CWorldGetterNode
{
    public float Value = 0f;

    public CWorldEmptyNode() : base() { }
    public CWorldEmptyNode(string name) : base(name) { }
    public CWorldEmptyNode(float value) : base() { Value = value; }

    public override void SetValue(float value)
    {
        Value = value;
    }

    public override float GetValue()
    {
        return Value;
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