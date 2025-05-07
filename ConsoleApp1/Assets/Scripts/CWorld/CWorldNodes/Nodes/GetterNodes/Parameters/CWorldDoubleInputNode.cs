using OpenTK.Mathematics;

public class CWorldDoubleInputNode : CWorldGetterNode
{
    public float Value1 
    {
        get => InputNode1.GetValue();
        set => InputNode1.SetValue(value);
    }

    public float Value2 
    {
        get => InputNode2.GetValue();
        set => InputNode2.SetValue(value);
    }

    public DoubleInputOperations Operation;
    public DoubleInputOperationType Type;

    public CWorldGetterNode InputNode1 = new CWorldEmptyNode("Empty"); 
    public CWorldGetterNode InputNode2 = new CWorldEmptyNode("Empty"); 

    public CWorldDoubleInputNode(DoubleInputOperationType type) : base()
    {
        Operation = DoubleInputOperations.GetOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        InputNode1.Init(position);
        InputNode2.Init(position);
    }

    public override float GetValue() 
    {
        return Operation.GetValue(InputNode1.GetValue(), InputNode2.GetValue());
    }

    public override CWorldNode Copy()
    {
        return new CWorldDoubleInputNode(Type)
        {
            Name = Name,
            Value1 = Value1,
            Value2 = Value2,
        };;
    }
}