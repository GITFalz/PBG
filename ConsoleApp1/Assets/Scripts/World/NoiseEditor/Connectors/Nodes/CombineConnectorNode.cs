using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class CombineConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UICombineNodePrefab NodePrefab;

    public InputGateConnector InputGateConnector1;
    public InputGateConnector InputGateConnector2;

    public OutputGateConnector OutputGateConnector;

    public float Value1 {
        get {
            return _value1; 
        } set {
            _value1 = value;
            NodePrefab.Value1InputField.SetText(NoSpace(_value1));
        }
    }
    public float Value2 {
        get {
            return _value2; 
        } set {
            _value2 = value;
            NodePrefab.Value2InputField.SetText(NoSpace(_value2));
        }
    }

    private float _value1 = 1.0f;
    private float _value2 = 1.0f;

    private int _value1Index = -1;
    private int _value2Index = -1;

    public CombineConnectorNode(UICombineNodePrefab combineNodePrefab)
    {
        NodePrefab = combineNodePrefab;
        InputGateConnector1 = new InputGateConnector(NodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(NodePrefab.InputButton2, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);

        NodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        NodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.Value1InputField.SetOnTextChange(() => SetValue(ref _value1, NodePrefab.Value1InputField, 1.0f, _value1Index));
        NodePrefab.Value2InputField.SetOnTextChange(() => SetValue(ref _value2, NodePrefab.Value2InputField, 1.0f, _value2Index));

        NodePrefab.Value1TextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.Value2TextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.Value1TextField.SetOnHold(() => SetSlideValue(ref _value1, NodePrefab.Value1InputField, 5f, _value1Index));
        NodePrefab.Value2TextField.SetOnHold(() => SetSlideValue(ref _value2, NodePrefab.Value2InputField, 5f, _value2Index));

        NodePrefab.Value1TextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.Value2TextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

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
        string line = $"    float {VariableName} = ";

        string value1 = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.Node.VariableName
            : ( _value1Index != -1 ? $"values[{_value1Index}]" : NoSpace(Value1));

        line += $"{value1};";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector2.OutputGateConnector.Node);

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
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector2.OutputGateConnector.Node);
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
        return [InputGateConnector1, InputGateConnector2];
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
            "NodeType: Combine\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Float: " + NoSpace(Value1) + " 0\n" +
            "        Float: " + NoSpace(Value2) + " 1\n" +
            "    }\n" +
            "    Inputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(InputGateConnector1.Name) + " 0\n" +
            "        Name: " + NoSpace(InputGateConnector2.Name) + " 1\n" +
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
        if (!InputGateConnector1.IsConnected)
        {
            _value1Index = index;
            values.Add(Value1);
            index++;
        }
        else
        {
            _value1Index = -1;
        }

        if (!InputGateConnector2.IsConnected)
        {
            _value2Index = index;
            values.Add(Value2);
            index++;
        }
        else
        {
            _value2Index = -1;
        }
    }
}