using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class VoronoiConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIVoronoiPrefab NodePrefab;

    public OutputGateConnector OutputGateConnector;

    public VoronoiOperation Operation;
    public VoronoiOperationType Type;

    public float Scale
    {
        get {
            return _scale; 
        } set {
            _scale = value;
            NodePrefab.ScaleInputField.SetText(NoSpace(_scale));
        }
    }
    public Vector2 Offset
    {
        get {
            return _offset; 
        } set {
            _offset = value;
            NodePrefab.OffsetXInputField.SetText(NoSpace(_offset.X));
            NodePrefab.OffsetYInputField.SetText(NoSpace(_offset.Y));
        }
    }

    private float _scale = 1.0f;
    private Vector2 _offset = (0.0f, 0.0f);

    private int _scaleIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public VoronoiConnectorNode(UIVoronoiPrefab voronoiNodePrefab, VoronoiOperationType type)
    {
        NodePrefab = voronoiNodePrefab;
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);
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

        NodePrefab.Collection.SetOnClick(() => SelectNode(this));

        Operation = VoronoiOperation.GetVoronoiOperation(type);
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
        string scaleValue = _scaleIndex != -1 ? $"values[{_scaleIndex}]" : Scale.ToString();
        string offsetXValue = _offsetXIndex != -1 ? $"values[{_offsetXIndex}]" : Offset.X.ToString();
        string offsetYValue = _offsetYIndex != -1 ? $"values[{_offsetYIndex}]" : Offset.Y.ToString();

        string scale = $"vec2({VariableName}Scale, {VariableName}Scale)";
        string offset = $"vec2({offsetXValue}, {offsetYValue})";

        return $"    float {VariableName}Scale = {scaleValue}; float {VariableName} = {Operation.GetFunction(scale, offset)};";
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
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
        return [];
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
        return [];
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
            $"NodeType: Voronoi " +
            $"Values: {NoSpace(Scale)} {NoSpace(Offset)} {NoSpace((int)Type)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(NodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _scaleIndex = index;
        values.Add(Scale);
        index++;

        _offsetXIndex = index;
        values.Add(Offset.X);
        index++;

        _offsetYIndex = index;
        values.Add(Offset.Y);
        index++;
    }
}