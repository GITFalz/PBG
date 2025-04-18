public class SingleInputOperationConnectorNode : ConnectorNode
{
    public string Name => SingleInputNodePrefab.Name;
    public UISingleInputNodePrefab SingleInputNodePrefab;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public SingleInputOperations Operation;

    public float Min = 0;
    public float Max = 1;

    public SingleInputOperationConnectorNode(UISingleInputNodePrefab singleInputNodePrefab, SingleInputOperationType type)
    {
        SingleInputNodePrefab = singleInputNodePrefab;
        InputGateConnector = new InputGateConnector(SingleInputNodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(SingleInputNodePrefab.OutputButton, this);
        SingleInputNodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        SingleInputNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));
        SingleInputNodePrefab.MinInputField.SetOnTextChange(SetMin);
        SingleInputNodePrefab.MaxInputField.SetOnTextChange(SetMax);
        Operation = SingleInputOperations.GetOperation(type);
    }

    private void SetMin()
    {
        string minText = SingleInputNodePrefab.MinInputField.Text;
        if (minText.Length == 0 || minText.EndsWith(".") || minText.EndsWith(","))
            minText += "0";
        if (float.TryParse(minText, out float minValue))
            Min = minValue;
        else
            Min = 0;

        Compile();
    }

    private void SetMax()
    {
        string maxText = SingleInputNodePrefab.MaxInputField.Text;
        if (maxText.Length == 0 || maxText.EndsWith(".") || maxText.EndsWith(","))
            maxText += "0";
        if (float.TryParse(maxText, out float maxValue))
            Max = maxValue;
        else
            Max = 1;

        Compile();
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
}