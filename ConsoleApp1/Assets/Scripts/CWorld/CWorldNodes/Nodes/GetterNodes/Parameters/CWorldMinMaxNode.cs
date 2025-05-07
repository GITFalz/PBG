using OpenTK.Mathematics;

public class CWorldMinMaxNode : CWorldGetterNode
{
    public float Min = 0;
    public float Max = 1;

    public MinMaxInputOperations Operation;
    public MinMaxInputOperationType Type;

    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty");  

    public CWorldMinMaxNode(MinMaxInputOperationType type) : base()
    {
        Operation = MinMaxInputOperations.GetOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        InputNode.Init(position);
    }

    public override float GetValue()
    {
        return Operation.GetValue(Min, Max, InputNode.GetValue());
    }

    public string GetFunction(string valueName)
    {
        return Operation.GetFunction(Min, Max, valueName);
    }

    public override CWorldNode Copy()
    {
        return new CWorldMinMaxNode(Type)
        {
            Name = Name,
            Min = Min,
            Max = Max,
        };
    }
}