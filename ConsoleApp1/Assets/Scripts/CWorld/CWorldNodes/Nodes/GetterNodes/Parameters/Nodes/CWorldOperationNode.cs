using OpenTK.Mathematics;

public class CWorldOperationNode : CWorldParameterNode
{
    public float Value1 
    {
        get => InputNode1.GetCachedValue(InputNode1Index);
        set => InputNode1.SetValue(value);
    }

    public float Value2 
    {
        get => InputNode2.GetCachedValue(InputNode2Index);
        set => InputNode2.SetValue(value);
    }

    public OperationOperations Operation;
    public OperationOperationType Type;

    public CWorldGetterNode InputNode1 = new CWorldEmptyNode("Empty"); 
    public int InputNode1Index = 0;
    public CWorldGetterNode InputNode2 = new CWorldEmptyNode("Empty"); 
    public int InputNode2Index = 0;

    public CWorldOperationNode(OperationOperationType type) : base()
    {
        Operation = OperationOperations.GetOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        InputNode1.Init(position);
        InputNode2.Init(position);
        CachedValue = Operation.GetValue(InputNode1.GetCachedValue(InputNode1Index), InputNode2.GetCachedValue(InputNode2Index));
    }

    public override CWorldNode Copy()
    {
        return new CWorldOperationNode(Type)
        {
            Name = Name,
            Value1 = Value1,
            Value2 = Value2,
            InputNode1Index = InputNode1Index,
            InputNode2Index = InputNode2Index,
        };;
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (InputNode1.IsntEmpty())
        {
            string inputName1 = nodeNameMap[InputNode1];
            ((CWorldOperationNode)copiedNode).InputNode1 = (CWorldGetterNode)copiedNodes[inputName1];
        }
        if (InputNode2.IsntEmpty())
        {
            string inputName2 = nodeNameMap[InputNode2];
            ((CWorldOperationNode)copiedNode).InputNode2 = (CWorldGetterNode)copiedNodes[inputName2];
        }
    }
}