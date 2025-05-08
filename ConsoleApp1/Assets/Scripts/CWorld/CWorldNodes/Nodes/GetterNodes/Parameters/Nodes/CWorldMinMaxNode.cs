using OpenTK.Mathematics;

public class CWorldMinMaxNode : CWorldParameterNode
{
    public static CWorldMinMaxNode Clamp => new CWorldMinMaxNode(MinMaxInputOperationType.Clamp);
    public static CWorldMinMaxNode Ignore => new CWorldMinMaxNode(MinMaxInputOperationType.Ignore);
    public static CWorldMinMaxNode Lerp => new CWorldMinMaxNode(MinMaxInputOperationType.Lerp);
    public static CWorldMinMaxNode Slide => new CWorldMinMaxNode(MinMaxInputOperationType.Slide);
    public static CWorldMinMaxNode Smooth => new CWorldMinMaxNode(MinMaxInputOperationType.Smooth);


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
        float min = Min;
        float max = Max;
        float value = InputNode.GetValue();
        return Operation.GetValue(min, max, value);
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

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (InputNode.IsEmpty())
            return;

        string inputName = nodeNameMap[InputNode];
        ((CWorldMinMaxNode)copiedNode).InputNode = (CWorldGetterNode)copiedNodes[inputName];
    }
}