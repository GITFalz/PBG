using OpenTK.Mathematics;

public class CWorldBaseInputNode : CWorldParameterNode
{
    public float Value
    {
        get => InputNode.CachedValue;
        set => InputNode.SetValue(value);
    }

    public BaseInputOperations Operation;
    public BaseInputOperationType Type;

    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty"); 

    public CWorldBaseInputNode(BaseInputOperationType type) : base()
    {
        Operation = BaseInputOperations.GetOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        InputNode.Init(position);
        CachedValue = Operation.GetValue(InputNode.CachedValue);
    }

    public override CWorldNode Copy()
    {
        return new CWorldBaseInputNode(Type)
        {
            Name = Name,
            Value = Value,
        };;
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (InputNode.IsntEmpty())
        {
            string inputName1 = nodeNameMap[InputNode];
            ((CWorldBaseInputNode)copiedNode).InputNode = (CWorldGetterNode)copiedNodes[inputName1];
        }
    }
}