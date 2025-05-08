using OpenTK.Windowing.Common;

public class RangeConnectorNode : ConnectorNode
{
    public string Name => RangeNodePrefab.Name;
    public UIRangeNodePrefab RangeNodePrefab;

    public InputGateConnector StartGateConnector;
    public InputGateConnector HeightGateConnector;

    public OutputGateConnector OutputGateConnector;

    public int Start {
        get {
            return _start; 
        } set {
            _start = value;
            RangeNodePrefab.StartInputField.SetText(NoSpace(_start));
        }
    }
    public int Height {
        get {
            return _height; 
        } set {
            _height = value;
            RangeNodePrefab.HeightInputField.SetText(NoSpace(_height));
        }
    }

    public bool Flipped {
        get => RangeNodePrefab.IsFlipped; 
        set {
            RangeNodePrefab.IsFlipped = value;
            RangeNodePrefab.FlippedText.SetText(value ? "Flip: True" : "Flip: False").UpdateCharacters();
        }
    }

    private int _start = 20;
    private int _height = 20;

    private int _startIndex = -1;
    private int _heightIndex = -1;

    public RangeConnectorNode(UIRangeNodePrefab doubleInputNodePrefab)
    {
        RangeNodePrefab = doubleInputNodePrefab;
        StartGateConnector = new InputGateConnector(doubleInputNodePrefab.StartButton, this);
        HeightGateConnector = new InputGateConnector(doubleInputNodePrefab.HeightButton, this);
        OutputGateConnector = new OutputGateConnector(doubleInputNodePrefab.OutputButton, this);

        RangeNodePrefab.StartButton.SetOnClick(() => InputConnectionTest(StartGateConnector));
        RangeNodePrefab.HeightButton.SetOnClick(() => InputConnectionTest(HeightGateConnector));
        RangeNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        RangeNodePrefab.StartInputField.SetOnTextChange(() => SetValue(ref _start, RangeNodePrefab.StartInputField, 20, _startIndex)); 
        RangeNodePrefab.HeightInputField.SetOnTextChange(() => SetValue(ref _height, RangeNodePrefab.HeightInputField, 20, _heightIndex));

        RangeNodePrefab.StartTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        RangeNodePrefab.HeightTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        RangeNodePrefab.StartTextField.SetOnHold(() => SetSlideValue(ref _start, RangeNodePrefab.StartInputField, 10, _startIndex)); 
        RangeNodePrefab.HeightTextField.SetOnHold(() => SetSlideValue(ref _height, RangeNodePrefab.HeightInputField, 10, _heightIndex));

        RangeNodePrefab.StartTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        RangeNodePrefab.HeightTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        RangeNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
    }

    public override string GetLine()
    {
        string line = $"    float {VariableName} = ";

        string value1 = StartGateConnector.IsConnected && StartGateConnector.OutputGateConnector != null
            ? StartGateConnector.OutputGateConnector.Node.VariableName
            : ( _startIndex != -1 ? $"values[{_startIndex}]" : NoSpace(Start));
        
        string value2 = HeightGateConnector.IsConnected && HeightGateConnector.OutputGateConnector != null
            ? HeightGateConnector.OutputGateConnector.Node.VariableName
            : ( _heightIndex != -1 ? $"values[{_heightIndex}]" : NoSpace(Height));

        line += $"max({value1}, {value2});";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (StartGateConnector.IsConnected && StartGateConnector.OutputGateConnector != null)
            connectedNodes.Add(StartGateConnector.OutputGateConnector.Node);

        if (HeightGateConnector.IsConnected && HeightGateConnector.OutputGateConnector != null)
            connectedNodes.Add(HeightGateConnector.OutputGateConnector.Node);

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
        if (StartGateConnector.IsConnected && StartGateConnector.OutputGateConnector != null)
            inputNodes.Add(StartGateConnector.OutputGateConnector.Node);

        if (HeightGateConnector.IsConnected && HeightGateConnector.OutputGateConnector != null)
            inputNodes.Add(HeightGateConnector.OutputGateConnector.Node);
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
        return [StartGateConnector, HeightGateConnector];
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [RangeNodePrefab];
    }

    public override UIController GetUIController()
    {
        return RangeNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        string flipped = RangeNodePrefab.IsFlipped ? "1" : "0";
        return 
            $"NodeType: Range " +
            $"Values: {NoSpace(Start)} {NoSpace(Height)} {flipped} " + 
            $"Inputs: {NoSpace(StartGateConnector.Name)} {NoSpace(HeightGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(RangeNodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        if (!StartGateConnector.IsConnected)
        {
            _startIndex = index;
            values.Add(Start);
            index++;
        }
        else
        {
            _startIndex = -1;
        }

        if (!HeightGateConnector.IsConnected)
        {
            _heightIndex = index;
            values.Add(Height);
            index++;
        }
        else
        {
            _heightIndex = -1;
        }
    }
}