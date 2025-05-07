using OpenTK.Windowing.Common;

public class MinMaxInputOperationConnectorNode : ConnectorNode
{
    public string Name => MinMaxInputNodePrefab.Name;
    public UIMinMaxInputNodePrefab MinMaxInputNodePrefab;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public MinMaxInputOperations Operation;
    public MinMaxInputOperationType Type;

    public float Min {
        get {
            return _min; 
        } set {
            _min = value;
            MinMaxInputNodePrefab.MinInputField.SetText(NoSpace(_min));
        }
    }
    public float Max {
        get {
            return _max; 
        } set {
            _max = value;
            MinMaxInputNodePrefab.MaxInputField.SetText(NoSpace(_max));
        }
    }

    private float _min = 0;
    private float _max = 1;

    private int _minIndex = -1;
    private int _maxIndex = -1;

    public MinMaxInputOperationConnectorNode(UIMinMaxInputNodePrefab singleInputNodePrefab, MinMaxInputOperationType type)
    {
        MinMaxInputNodePrefab = singleInputNodePrefab;
        InputGateConnector = new InputGateConnector(MinMaxInputNodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(MinMaxInputNodePrefab.OutputButton, this);
        MinMaxInputNodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        MinMaxInputNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        MinMaxInputNodePrefab.MinInputField.SetOnTextChange(() => SetValue(ref _min, MinMaxInputNodePrefab.MinInputField, 0, _minIndex));
        MinMaxInputNodePrefab.MaxInputField.SetOnTextChange(() => SetValue(ref _max, MinMaxInputNodePrefab.MaxInputField, 1, _maxIndex));

        MinMaxInputNodePrefab.MinTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        MinMaxInputNodePrefab.MaxTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        MinMaxInputNodePrefab.MinTextField.SetOnHold(() => SetSlideValue(ref _min, MinMaxInputNodePrefab.MinInputField, 5f, _minIndex));
        MinMaxInputNodePrefab.MaxTextField.SetOnHold(() => SetSlideValue(ref _max, MinMaxInputNodePrefab.MaxInputField, 5f, _maxIndex));

        MinMaxInputNodePrefab.MinTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        MinMaxInputNodePrefab.MaxTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
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
        string minValue = _minIndex != -1 ? $"values[{_minIndex}]" : NoSpace(Min);
        string maxValue = _maxIndex != -1 ? $"values[{_maxIndex}]" : NoSpace(Max);

        string line = $"    float {VariableName} = ";
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            line += $"{Operation.GetFunction(minValue, maxValue, InputGateConnector.OutputGateConnector.Node.VariableName)};";
        }
        else
        {
            line += $"{Operation.GetFunction(minValue, maxValue, "0.0")};";
        }
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

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _minIndex = index;
        values.Add(Min);
        index++;

        _maxIndex = index;
        values.Add(Max);
        index++;
    }
}