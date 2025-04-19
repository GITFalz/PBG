public class MinMaxInputOperationConnectorNode : ConnectorNode
{
    public string Name => MinMaxInputNodePrefab.Name;
    public UIMinMaxInputNodePrefab MinMaxInputNodePrefab;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public MinMaxInputOperations Operation;
    public MinMaxInputOperationType Type;

    public float Min = 0;
    public float Max = 1;

    public MinMaxInputOperationConnectorNode(UIMinMaxInputNodePrefab singleInputNodePrefab, MinMaxInputOperationType type)
    {
        MinMaxInputNodePrefab = singleInputNodePrefab;
        InputGateConnector = new InputGateConnector(MinMaxInputNodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(MinMaxInputNodePrefab.OutputButton, this);
        MinMaxInputNodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        MinMaxInputNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        MinMaxInputNodePrefab.MinInputField.SetOnTextChange(() => SetValue(ref Min, MinMaxInputNodePrefab.MinInputField, 0));
        MinMaxInputNodePrefab.MaxInputField.SetOnTextChange(() => SetValue(ref Max, MinMaxInputNodePrefab.MaxInputField, 1));

        MinMaxInputNodePrefab.MinTextField.SetOnHold(() => SetSlideValue(ref Min, MinMaxInputNodePrefab.MinInputField, 0.1f));
        MinMaxInputNodePrefab.MaxTextField.SetOnHold(() => SetSlideValue(ref Max, MinMaxInputNodePrefab.MaxInputField, 0.1f));
        
        MinMaxInputNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);

        Operation = MinMaxInputOperations.GetOperation(type);
        Type = type;
    }

    public float GetValue(float value)
    {
        return Operation.GetValue(Min, Max, value);
    }

    public override string GetLine()
    {
        string line = $"    float {VariableName} = ";
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            line += $"{Operation.GetFunction(Min, Max, InputGateConnector.OutputGateConnector.Node.VariableName)};";
        }
        else
        {
            line += $"{Operation.GetFunction(Min, Max, "0.0")};";
        }
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector.OutputGateConnector.Node);
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            connectedNodes.Add(OutputGateConnector.InputGateConnector.Node);
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
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            outputNodes.Add(OutputGateConnector.InputGateConnector.Node);
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
        return [MinMaxInputNodePrefab];
    }

    public override UIController GetUIController()
    {
        return MinMaxInputNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            $"NodeType: MinMaxInputOperation " +
            $"Values: {NoSpace(Min)} {NoSpace(Max)} {NoSpace((int)Type)} " +
            $"Inputs: {NoSpace(InputGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(MinMaxInputNodePrefab.Collection.Offset)}";
    }
}