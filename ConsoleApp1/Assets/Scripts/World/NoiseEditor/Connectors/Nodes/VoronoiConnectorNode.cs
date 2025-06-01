using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class VoronoiConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIVoronoiPrefab NodePrefab;

    public InputGateConnector InputGateConnector1;
    public InputGateConnector InputGateConnector2;
    public OutputGateConnector Output;
    public OutputGateConnector OutputCellXConnector;
    public OutputGateConnector OutputCellYConnector;

    public VoronoiOperation Operation;
    public VoronoiOperationType Type;
    public int Flag = 0;
    

    public bool Region => NodePrefab.Region;
     
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            NodePrefab.ValueInputField.SetText(NoSpace(_value));
        }
    }

    public float Threshold
    {
        get
        {
            return _threshold;
        }
        set
        {
            _threshold = value;
            NodePrefab.ThresholdInputField.SetText(NoSpace(_threshold));
        }
    }

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
            _offsetX = _offset.X;
            _offsetY = _offset.Y;
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

    private float _value = 0.0f;
    private float _threshold = 0.0f;
    private float _scale = 1.0f;
    private Vector2 _offset = (0.0f, 0.0f);
    private float _offsetX = 0.0f;
    private float _offsetY = 0.0f;

    private int _valueIndex = -1;
    private int _thresholdIndex = -1;
    private int _scaleIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public VoronoiConnectorNode(UIVoronoiPrefab voronoiNodePrefab, VoronoiOperationType type) : base()
    {
        NodePrefab = voronoiNodePrefab;
        InputGateConnector1 = new InputGateConnector(voronoiNodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(voronoiNodePrefab.InputButton2, this);
        Output = new OutputGateConnector(NodePrefab.OutputButton, this);
        OutputCellXConnector = new OutputGateConnector(NodePrefab.OutputCellXButton, this);
        OutputCellYConnector = new OutputGateConnector(NodePrefab.OutputCellYButton, this);

        NodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        NodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));

        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(Output));
        NodePrefab.OutputCellXButton.SetOnClick(() => OutputConnectionTest(OutputCellXConnector));
        NodePrefab.OutputCellYButton.SetOnClick(() => OutputConnectionTest(OutputCellYConnector));

        NodePrefab.ValueInputField.SetOnTextChange(() => SetValue(ref _value, NodePrefab.ValueInputField, 0.0f, _valueIndex));
        NodePrefab.ThresholdInputField.SetOnTextChange(() => SetValue(ref _threshold, NodePrefab.ThresholdInputField, 0.0f, _thresholdIndex));
        NodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref _scale, NodePrefab.ScaleInputField, 1.0f, _scaleIndex));
        NodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref _offset.X, NodePrefab.OffsetXInputField, 0.0f, _offsetXIndex));
        NodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref _offset.Y, NodePrefab.OffsetYInputField, 0.0f, _offsetYIndex));

        NodePrefab.ValueTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.ThresholdTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.ScaleTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.ValueTextField.SetOnHold(() => SetSlideValue(ref _value, NodePrefab.ValueInputField, 5f, _valueIndex));
        NodePrefab.ThresholdTextField.SetOnHold(() => SetSlideValue(ref _threshold, NodePrefab.ThresholdInputField, 5f, _thresholdIndex));
        NodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref _scale, NodePrefab.ScaleInputField, 5f, _scaleIndex));
        NodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref _offset.X, NodePrefab.OffsetXInputField, 5f, _offsetXIndex));
        NodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref _offset.Y, NodePrefab.OffsetYInputField, 5f, _offsetYIndex));

        NodePrefab.ValueTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.ThresholdTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.ScaleTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.OffsetXTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.OffsetYTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        NodePrefab.Collection.SetOnClick(() => SelectNode(this));

        NodePrefab.RegionToggleAction = () =>
        {
            Flag = 0;
            if (Region) Flag += VoronoiOperation.InputRegion;

            Operation = VoronoiOperation.GetVoronoiOperation(Type, Flag);
        };

        Operation = VoronoiOperation.GetVoronoiOperation(type, 0);
        Type = type;
        Flag = 0;
    }

    public override int GetIndex(OutputGateConnector output)
    {
        if (output == Output)
            return 0;
        else if (output == OutputCellXConnector)
            return 1;
        else if (output == OutputCellYConnector)
            return 2;
        else
            return -1;
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
        string valueValue = _valueIndex != -1 ? $"values[{_valueIndex}]" : NoSpace(Value);
        string thresholdValue = _thresholdIndex != -1 ? $"values[{_thresholdIndex}]" : NoSpace(Threshold);
        string scaleValue = _scaleIndex != -1 ? $"values[{_scaleIndex}]" : NoSpace(Scale);
        string variableName = Output.VariableName;
        string offsetX = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.VariableName
            : ( _offsetXIndex != -1 ? $"values[{_offsetXIndex}]" : NoSpace(OffsetX));
        string offsetY = InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null
            ? InputGateConnector2.OutputGateConnector.VariableName
            : ( _offsetYIndex != -1 ? $"values[{_offsetYIndex}]" : NoSpace(OffsetY));

        string scale = $"vec2({variableName}Scale, {variableName}Scale)";
        string offset = $"vec2({offsetX}, {offsetY})";

        return $"    float {variableName}Scale = {scaleValue}; vec2 {variableName}g; float {variableName} = {Operation.GetFunction(scale, offset, valueValue, thresholdValue, variableName+"g")}; float {OutputCellXConnector.VariableName} = {variableName}g.x; float {OutputCellYConnector.VariableName} = {variableName}g.y;";
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector2.OutputGateConnector.Node);

        if (Output.IsConnected)
        {
            foreach (var input in Output.InputGateConnectors)
            {
                connectedNodes.Add(input.Node);
            }
        }
        if (OutputCellXConnector.IsConnected)
        {
            foreach (var input in OutputCellXConnector.InputGateConnectors)
            {
                connectedNodes.Add(input.Node);
            }
        }
        if (OutputCellYConnector.IsConnected)
        {
            foreach (var input in OutputCellYConnector.InputGateConnectors)
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
        if (Output.IsConnected)
        {
            foreach (var input in Output.InputGateConnectors)
            {
                outputNodes.Add(input.Node);
            }
        }
        if (OutputCellXConnector.IsConnected)
        {
            foreach (var input in OutputCellXConnector.InputGateConnectors)
            {
                outputNodes.Add(input.Node);
            }
        }
        if (OutputCellYConnector.IsConnected)
        {
            foreach (var input in OutputCellYConnector.InputGateConnectors)
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
        return [Output, OutputCellXConnector, OutputCellYConnector];
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
            "NodeType: Voronoi\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Float: " + NoSpace(Scale) + "\n" +
            "        Vector2: " + NoSpace(Offset) + "\n" +
            "        Type: " + NoSpace((int)Type) + "\n" +
            "    }\n" +
            "    Inputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(InputGateConnector1.Name) + " 0\n" +
            "        Name: " + NoSpace(InputGateConnector2.Name) + " 1\n" +
            "    }\n" +
            "    Outputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(Output.Name) + " 0\n" +
            "        Name: " + NoSpace(OutputCellXConnector.Name) + " 1\n" +
            "        Name: " + NoSpace(OutputCellYConnector.Name) + " 2\n" +
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

        if (NodePrefab.Region)
        {
            if (!InputGateConnector1.IsConnected && !InputGateConnector2.IsConnected)
            {
                _valueIndex = index;
                values.Add(Value);
                index++;
            }
            else
            {
                _valueIndex = -1;
            }

            if (!InputGateConnector1.IsConnected && !InputGateConnector2.IsConnected)
            {
                _thresholdIndex = index;
                values.Add(Threshold);
                index++;
            }
            else
            {
                _thresholdIndex = -1;
            }
        }

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