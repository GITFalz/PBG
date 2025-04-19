public class DoubleInputOperationConnectorNode : ConnectorNode
{
    public string Name => DoubleInputNodePrefab.Name;
    public UIDoubleInputNodePrefab DoubleInputNodePrefab;

    public InputGateConnector InputGateConnector1;
    public InputGateConnector InputGateConnector2;

    public OutputGateConnector OutputGateConnector;

    public DoubleInputOperations Operation;
    public DoubleInputOperationType Type;

    public float Value1 = 1.0f;
    public float Value2 = 1.0f;

    public DoubleInputOperationConnectorNode(UIDoubleInputNodePrefab doubleInputNodePrefab, DoubleInputOperationType type)
    {
        DoubleInputNodePrefab = doubleInputNodePrefab;
        InputGateConnector1 = new InputGateConnector(doubleInputNodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(doubleInputNodePrefab.InputButton2, this);
        OutputGateConnector = new OutputGateConnector(doubleInputNodePrefab.OutputButton, this);

        DoubleInputNodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        DoubleInputNodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));
        DoubleInputNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        DoubleInputNodePrefab.Value1InputField.SetOnTextChange(() => SetValue(ref Value1, DoubleInputNodePrefab.Value1InputField, 1.0f));
        DoubleInputNodePrefab.Value2InputField.SetOnTextChange(() => SetValue(ref Value2, DoubleInputNodePrefab.Value2InputField, 1.0f));

        DoubleInputNodePrefab.Value1TextField.SetOnHold(() => SetSlideValue(ref Value1, DoubleInputNodePrefab.Value1InputField, 0.1f));
        DoubleInputNodePrefab.Value2TextField.SetOnHold(() => SetSlideValue(ref Value2, DoubleInputNodePrefab.Value2InputField, 0.1f));

        DoubleInputNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);

        Operation = DoubleInputOperations.GetOperation(type);
        Type = type;
    }

    public override string GetLine()
    {
        string line = $"    float {VariableName} = ";

        string value1 = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.Node.VariableName
            : Value1.ToString();
        
        string value2 = InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null
            ? InputGateConnector2.OutputGateConnector.Node.VariableName
            : Value2.ToString();

        line += $"{Operation.GetFunction(value1, value2)};";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector2.OutputGateConnector.Node);

        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            connectedNodes.Add(OutputGateConnector.InputGateConnector.Node);
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes() 
    { 
        List<ConnectorNode> inputNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector2.OutputGateConnector.Node);
        return inputNodes; 
    }

    public override List<ConnectorNode> GetOutputNodes() 
    { 
        List<ConnectorNode> outputNodes = [];
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            outputNodes.Add(OutputGateConnector.InputGateConnector.Node);
        return outputNodes;
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        return [InputGateConnector1, InputGateConnector2];
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [DoubleInputNodePrefab];
    }

    public override UIController GetUIController()
    {
        return DoubleInputNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            $"NodeType: DoubleInputOperation " +
            $"Values: {NoSpace(Value1)} {NoSpace(Value2)} {NoSpace((int)Type)} " + 
            $"Inputs: {NoSpace(InputGateConnector1.Name)} {NoSpace(InputGateConnector2.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(DoubleInputNodePrefab.Collection.Offset)}";
    }
}