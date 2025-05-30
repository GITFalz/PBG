using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class DirectSampleConnectorNode : ConnectorNode 
{
    public string Name => NodePrefab.Name;
    public UIDirectSampleNodePrefab NodePrefab;

    public InputGateConnector InputGateConnector1;
    public InputGateConnector InputGateConnector2;
    public OutputGateConnector OutputGateConnector;

    public float Scale
    {
        get
        {
            return _scale;
        }
        set
        {
            _scale = value;
            NodePrefab.ScaleInputField.SetText(NoSpace(_scale));
        }
    }
    public Vector2 Offset
    {
        get
        {
            return _offset;
        }
        set
        {
            _offset = value;
            NodePrefab.OffsetXInputField.SetText(NoSpace(_offset.X));
            NodePrefab.OffsetYInputField.SetText(NoSpace(_offset.Y));
        }
    }

    public float OffsetX {
        get {
            return _offsetX; 
        } set {
            _offsetX = value;
            _offset.X = _offsetX;
            NodePrefab.OffsetXInputField.SetText(NoSpace(_offsetX));
        }
    }
    public float OffsetY {
        get {
            return _offsetY; 
        } set {
            _offsetY = value;
            _offset.Y = _offsetY;
            NodePrefab.OffsetYInputField.SetText(NoSpace(_offsetY));
        }
    }

    private float _scale = 1.0f;
    private Vector2 _offset = (0.0f, 0.0f);
    private float _offsetX = 0.0f;
    private float _offsetY = 0.0f;

    private int _scaleIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public DirectSampleConnectorNode(UIDirectSampleNodePrefab sampleNodePrefab)
    {
        NodePrefab = sampleNodePrefab;
        InputGateConnector1 = new InputGateConnector(sampleNodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(sampleNodePrefab.InputButton2, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);

        NodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        NodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref _scale, NodePrefab.ScaleInputField, 1.0f, _scaleIndex));
        NodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref _offset.X, NodePrefab.OffsetXInputField, 0.0f, _offsetXIndex));
        NodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref _offset.Y, NodePrefab.OffsetYInputField, 0.0f, _offsetYIndex));

        NodePrefab.ScaleTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref _scale, NodePrefab.ScaleInputField, 5f, _scaleIndex));
        NodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref _offset.X, NodePrefab.OffsetXInputField, 5f, _offsetXIndex));
        NodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref _offset.Y, NodePrefab.OffsetYInputField, 5f, _offsetYIndex));

        NodePrefab.ScaleTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.OffsetXTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.OffsetYTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        sampleNodePrefab.Collection.SetOnClick(() => SelectNode(this));
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
        string scaleValue = _scaleIndex != -1 ? $"values[{_scaleIndex}]" : Scale.ToString();
        string variableName = OutputGateConnector.VariableName;

        string offsetX = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.VariableName
            : ( _offsetXIndex != -1 ? $"values[{_offsetXIndex}]" : NoSpace(OffsetX));
        
        string offsetY = InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null
            ? InputGateConnector2.OutputGateConnector.VariableName
            : ( _offsetYIndex != -1 ? $"values[{_offsetYIndex}]" : NoSpace(OffsetY));

        string scale = $"vec2({variableName}Scale, {variableName}Scale)";
        string offset = $"vec2({offsetX}, {offsetY})";

        return $"    float {variableName}Scale = {scaleValue}; float {variableName} = ";//{Operation.GetFunction(scale, offset)};";
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
            "NodeType: Sample\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Float: " + NoSpace(Scale) + "\n" +
            "        Vector2: " + NoSpace(Offset) + "\n" +
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
        _scaleIndex = index;
        values.Add(Scale);
        index++;

        if (!InputGateConnector1.IsConnected)
        {
            _offsetXIndex = index;
            values.Add(OffsetX);
            index++;
        }
        else
        {
            _offsetXIndex = -1;
        }

        if (!InputGateConnector2.IsConnected)
        {
            _offsetYIndex = index;
            values.Add(OffsetY);
            index++;
        }
        else
        {
            _offsetYIndex = -1;
        }
    }
}