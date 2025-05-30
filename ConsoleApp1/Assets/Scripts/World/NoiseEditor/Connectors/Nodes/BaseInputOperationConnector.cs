using OpenTK.Mathematics;

public class BaseInputConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIBaseInputNodePrefab NodePrefab;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public BaseInputOperations Operation;
    public BaseInputOperationType Type;

    public BaseInputConnectorNode(UIBaseInputNodePrefab doubleInputNodePrefab, BaseInputOperationType type)
    {
        NodePrefab = doubleInputNodePrefab;
        InputGateConnector = new InputGateConnector(doubleInputNodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(doubleInputNodePrefab.OutputButton, this);

        NodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.Collection.SetOnClick(() => SelectNode(this));

        Operation = BaseInputOperations.GetOperation(type);
        Type = type;
    }

    public override void Select()
    {
        NodePrefab.SelectionImage.SetVisibility(true);
    }

    public override void Deselect()
    {
        NodePrefab.SelectionImage.SetVisibility(false);
    }

    public override void Move(Vector2 delta)
    {
        NodePrefab.Collection.SetOffset(NodePrefab.Collection.Offset + (delta.X, delta.Y, 0, 0));
        NodePrefab.Collection.Align();
        NodePrefab.Collection.UpdateTransformation();
    }

    public override string GetLine()
    {
        string line = $"    float {VariableName} = ";
        string value = InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null ? InputGateConnector.OutputGateConnector.Node.VariableName : "0";
        line += $"{Operation.GetFunction(value)};";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector.OutputGateConnector.Node);

        if (OutputGateConnector.IsConnected)
        {
            foreach (var input in OutputGateConnector.InputGateConnectors)
            {
                connectedNodes.Add(input.Node);
            }
        }
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes()
    {
        List<ConnectorNode> inputNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector.OutputGateConnector.Node);

        return inputNodes;
    }

    public override List<ConnectorNode> GetOutputNodes()
    {
        List<ConnectorNode> outputNodes = [];
        if (OutputGateConnector.IsConnected)
        {
            foreach (var input in OutputGateConnector.InputGateConnectors)
            {
                outputNodes.Add(input.Node);
            }
        }
        return outputNodes;
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        return [InputGateConnector];
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [NodePrefab];
    }

    public override UIController GetUIController()
    {
        return NodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return
            "NodeType: BaseInput\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Type: " + NoSpace((int)Type) + "\n" +
            "    }\n" +
            "    Inputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(InputGateConnector.Name) + "\n" +
            "    }\n" +
            "    Outputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(OutputGateConnector.Name) + "\n" +
            "    }\n" +
            "    Prefab:\n" +
            "    {\n" + 
            "        Name: " + NoSpace(Name) + "\n" +
            "        Offset: " + NoSpace(NodePrefab.Collection.Offset) + "\n" +
            "    }\n" +
            "}\n";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {

    }
}