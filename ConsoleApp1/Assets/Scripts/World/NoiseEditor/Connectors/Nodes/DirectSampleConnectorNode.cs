using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class DirectSampleConnectorNode : ConnectorNode 
{
    public string Name => NodePrefab.Name;
    public UIDirectSampleNodePrefab NodePrefab;

    public InputGateConnector InputPosXConnector;
    public InputGateConnector InputPosYConnector;
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
    
    public Vector2 Position
    {
        get
        {
            return (PosX, PosY);
        }
        set
        {
            PosX = value.X;
            PosY = value.Y;
            NodePrefab.PosXInputField.SetText(NoSpace(PosX));
            NodePrefab.PosYInputField.SetText(NoSpace(PosY));
        }
    }
    
    public float PosX
    {
        get
        {
            return _posX;
        }
        set
        {
            _posX = value;
            NodePrefab.PosXInputField.SetText(NoSpace(_posX));
        }
    }

    public float PosY {
        get {
            return _posY;
        } set {
            _posY = value;
            NodePrefab.PosYInputField.SetText(NoSpace(_posY));
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
    private float _posX = 0.0f;
    private float _posY = 0.0f;
    private float _offsetX = 0.0f;
    private float _offsetY = 0.0f;

    private int _scaleIndex = -1;
    private int _posXIndex = -1;
    private int _posYIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public DirectSampleConnectorNode(UIDirectSampleNodePrefab sampleNodePrefab)
    {
        NodePrefab = sampleNodePrefab;
        InputPosXConnector = new InputGateConnector(sampleNodePrefab.InputPosX, this);
        InputPosYConnector = new InputGateConnector(sampleNodePrefab.InputPosY, this);
        InputGateConnector1 = new InputGateConnector(sampleNodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(sampleNodePrefab.InputButton2, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);   

        NodePrefab.InputPosX.SetOnClick(() => InputConnectionTest(InputPosXConnector));
        NodePrefab.InputPosY.SetOnClick(() => InputConnectionTest(InputPosYConnector));
        NodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        NodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref _scale, NodePrefab.ScaleInputField, 1.0f, _scaleIndex));
        NodePrefab.PosXInputField.SetOnTextChange(() => SetValue(ref _posX, NodePrefab.PosXInputField, 0.0f, _posXIndex));
        NodePrefab.PosYInputField.SetOnTextChange(() => SetValue(ref _posY, NodePrefab.PosYInputField, 0.0f, _posYIndex));
        NodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref _offset.X, NodePrefab.OffsetXInputField, 0.0f, _offsetXIndex));
        NodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref _offset.Y, NodePrefab.OffsetYInputField, 0.0f, _offsetYIndex));

        NodePrefab.ScaleTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.PosXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.PosYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.OffsetYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref _scale, NodePrefab.ScaleInputField, 5f, _scaleIndex));
        NodePrefab.PosXTextField.SetOnHold(() => SetSlideValue(ref _posX, NodePrefab.PosXInputField, 5f, _posXIndex));
        NodePrefab.PosYTextField.SetOnHold(() => SetSlideValue(ref _posY, NodePrefab.PosYInputField, 5f, _posYIndex));
        NodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref _offset.X, NodePrefab.OffsetXInputField, 5f, _offsetXIndex));
        NodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref _offset.Y, NodePrefab.OffsetYInputField, 5f, _offsetYIndex));

        NodePrefab.ScaleTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.PosXTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.PosYTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
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

        string posX = InputPosXConnector.IsConnected && InputPosXConnector.OutputGateConnector != null
            ? InputPosXConnector.OutputGateConnector.VariableName
            : ( _posXIndex != -1 ? $"values[{_posXIndex}]" : NoSpace(PosX));

        string posY = InputPosYConnector.IsConnected && InputPosYConnector.OutputGateConnector != null
            ? InputPosYConnector.OutputGateConnector.VariableName
            : ( _posYIndex != -1 ? $"values[{_posYIndex}]" : NoSpace(PosY));

        string offsetX = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.VariableName
            : ( _offsetXIndex != -1 ? $"values[{_offsetXIndex}]" : NoSpace(OffsetX));
        
        string offsetY = InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null
            ? InputGateConnector2.OutputGateConnector.VariableName
            : ( _offsetYIndex != -1 ? $"values[{_offsetYIndex}]" : NoSpace(OffsetY));

        string scale = $"vec2({variableName}Scale, {variableName}Scale)";
        string offset = $"vec2({offsetX}, {offsetY})";

        return $"    float {variableName}Scale = {scaleValue}; float {variableName} = DirectSampleNoise({scale}, {offset}, vec2({posX}, {posY}));";
    }

    public override int GetIndex(OutputGateConnector output)
    {
        if (output == OutputGateConnector)
            return 0;
        return -1;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = GetInputNodes();

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
        if (InputPosXConnector.IsConnected && InputPosXConnector.OutputGateConnector != null)
            inputNodes.Add(InputPosXConnector.OutputGateConnector.Node);

        if (InputPosYConnector.IsConnected && InputPosYConnector.OutputGateConnector != null)
            inputNodes.Add(InputPosYConnector.OutputGateConnector.Node);
        
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
        return [InputPosXConnector, InputPosYConnector, InputGateConnector1, InputGateConnector2];
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
            "NodeType: DirectSample\n" +
            "{\n" +
            "    Values:\n" +
            "    {\n" +
            "        Float: " + NoSpace(Scale) + "\n" +
            "        Vector2: " + NoSpace(Offset) + " 0\n" +
            "        Vector2: " + NoSpace(Position) + " 1\n" +         
            "    }\n" +
            "    Inputs:\n" +
            "    {\n" +
            "        Name: " + NoSpace(InputPosXConnector.Name) + " 0\n" +
            "        Name: " + NoSpace(InputPosYConnector.Name) + " 1\n" +
            "        Name: " + NoSpace(InputGateConnector1.Name) + " 2\n" +
            "        Name: " + NoSpace(InputGateConnector2.Name) + " 3\n" +
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

        if (!InputPosXConnector.IsConnected)
        {
            _posXIndex = index;
            values.Add(PosX);
            index++;
        }
        else
        {
            _posXIndex = -1;
        }

        if (!InputPosYConnector.IsConnected)
        {
            _posYIndex = index;
            values.Add(PosY);
            index++;
        }
        else
        {
            _posYIndex = -1;
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