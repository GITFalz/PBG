using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class MinMaxInputOperationConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIMinMaxInputNodePrefab NodePrefab;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public MinMaxInputOperations Operation;
    public MinMaxInputOperationType Type;

    public float Min {
        get {
            return _min; 
        } set {
            _min = value;
            NodePrefab.MinInputField.SetText(NoSpace(_min));
        }
    }
    public float Max {
        get {
            return _max; 
        } set {
            _max = value;
            NodePrefab.MaxInputField.SetText(NoSpace(_max));
        }
    }

    private float _min = 0;
    private float _max = 1;

    private int _minIndex = -1;
    private int _maxIndex = -1;

    public MinMaxInputOperationConnectorNode(UIMinMaxInputNodePrefab singleInputNodePrefab, MinMaxInputOperationType type)
    {
        NodePrefab = singleInputNodePrefab;
        InputGateConnector = new InputGateConnector(NodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);
        NodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.MinInputField.SetOnTextChange(() => SetValue(ref _min, NodePrefab.MinInputField, 0, _minIndex));
        NodePrefab.MaxInputField.SetOnTextChange(() => SetValue(ref _max, NodePrefab.MaxInputField, 1, _maxIndex));

        NodePrefab.MinTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.MaxTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.MinTextField.SetOnHold(() => SetSlideValue(ref _min, NodePrefab.MinInputField, 5f, _minIndex));
        NodePrefab.MaxTextField.SetOnHold(() => SetSlideValue(ref _max, NodePrefab.MaxInputField, 5f, _maxIndex));

        NodePrefab.MinTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.MaxTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
        NodePrefab.Collection.SetOnClick(() => SelectNode(this));

        Operation = MinMaxInputOperations.GetOperation(type);
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
        string minValue = _minIndex != -1 ? $"values[{_minIndex}]" : NoSpace(Min);
        string maxValue = _maxIndex != -1 ? $"values[{_maxIndex}]" : NoSpace(Max);

        string line = $"    float {OutputGateConnector.VariableName} = ";
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            line += $"{Operation.GetFunction(minValue, maxValue, InputGateConnector.OutputGateConnector.VariableName)};";
        }
        else
        {
            line += $"{Operation.GetFunction(minValue, maxValue, "0.0")};";
        }
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
            "NodeType: MinMaxInputOperation\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Float: " + NoSpace(Min) + " 0\n" +
            "        Float: " + NoSpace(Max) + " 1\n" +
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
        _minIndex = index;
        values.Add(Min);
        index++;

        _maxIndex = index;
        values.Add(Max);
        index++;
    }
}