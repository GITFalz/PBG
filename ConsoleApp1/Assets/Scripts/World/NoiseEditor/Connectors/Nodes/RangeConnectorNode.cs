using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class RangeConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIRangeNodePrefab NodePrefab;

    public InputGateConnector StartGateConnector;
    public InputGateConnector HeightGateConnector;

    public OutputGateConnector OutputGateConnector;

    public int Start {
        get {
            return _start; 
        } set {
            _start = value;
            NodePrefab.StartInputField.SetText(NoSpace(_start));
        }
    }
    public int Height {
        get {
            return _height; 
        } set {
            _height = value;
            NodePrefab.HeightInputField.SetText(NoSpace(_height));
        }
    }

    public bool Flipped {
        get => NodePrefab.IsFlipped; 
        set {
            NodePrefab.IsFlipped = value;
            NodePrefab.FlippedText.SetText(value ? "Flip: True" : "Flip: False").UpdateCharacters();
        }
    }

    private int _start = 20;
    private int _height = 20;

    private int _startIndex = -1;
    private int _heightIndex = -1;

    public RangeConnectorNode(UIRangeNodePrefab doubleInputNodePrefab)
    {
        NodePrefab = doubleInputNodePrefab;
        StartGateConnector = new InputGateConnector(doubleInputNodePrefab.StartButton, this);
        HeightGateConnector = new InputGateConnector(doubleInputNodePrefab.HeightButton, this);
        OutputGateConnector = new OutputGateConnector(doubleInputNodePrefab.OutputButton, this);

        NodePrefab.StartButton.SetOnClick(() => InputConnectionTest(StartGateConnector));
        NodePrefab.HeightButton.SetOnClick(() => InputConnectionTest(HeightGateConnector));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.StartInputField.SetOnTextChange(() => SetValue(ref _start, NodePrefab.StartInputField, 20, _startIndex)); 
        NodePrefab.HeightInputField.SetOnTextChange(() => SetValue(ref _height, NodePrefab.HeightInputField, 20, _heightIndex));

        NodePrefab.StartTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.HeightTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.StartTextField.SetOnHold(() => SetSlideValue(ref _start, NodePrefab.StartInputField, 10, _startIndex)); 
        NodePrefab.HeightTextField.SetOnHold(() => SetSlideValue(ref _height, NodePrefab.HeightInputField, 10, _heightIndex));

        NodePrefab.StartTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.HeightTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        NodePrefab.Collection.SetOnClick(() => SelectNode(this));
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
        string line = $"    float {OutputGateConnector.VariableName} = ";

        string value1 = StartGateConnector.IsConnected && StartGateConnector.OutputGateConnector != null
            ? StartGateConnector.OutputGateConnector.VariableName
            : ( _startIndex != -1 ? $"values[{_startIndex}]" : NoSpace(Start));
        
        string value2 = HeightGateConnector.IsConnected && HeightGateConnector.OutputGateConnector != null
            ? HeightGateConnector.OutputGateConnector.VariableName
            : ( _heightIndex != -1 ? $"values[{_heightIndex}]" : NoSpace(Height));

        line += $"max({value1}, {value2});";
        return line;
    }

    public override int GetIndex(OutputGateConnector output)
    {
        if (output == OutputGateConnector)
            return 0;
        return -1;
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
        return [NodePrefab];
    }

    public override UIController GetUIController()
    {
        return NodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        string flipped = NodePrefab.IsFlipped ? "1" : "0";
        return 
            "NodeType: Range\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Int: " + NoSpace(Start) + " 0\n" +
            "        Int: " + NoSpace(Height) + " 1\n" +
            "        Bool: " + NoSpace(flipped) + "\n" +
            "    }\n" +
            "    Inputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(StartGateConnector.Name) + " 0\n" +
            "        Name: " + NoSpace(HeightGateConnector.Name) + " 1\n" +
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