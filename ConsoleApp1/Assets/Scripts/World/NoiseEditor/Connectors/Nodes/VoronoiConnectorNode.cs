using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class VoronoiConnectorNode : ConnectorNode
{
    public string Name => VoronoiNodePrefab.Name;
    public UIVoronoiPrefab VoronoiNodePrefab;

    public OutputGateConnector OutputGateConnector;

    public VoronoiOperation Operation;
    public VoronoiOperationType Type;

    public float Scale
    {
        get {
            return _scale; 
        } set {
            _scale = value;
            VoronoiNodePrefab.ScaleInputField.SetText(NoSpace(_scale));
        }
    }
    public Vector2 Offset
    {
        get {
            return _offset; 
        } set {
            _offset = value;
            VoronoiNodePrefab.OffsetXInputField.SetText(NoSpace(_offset.X));
            VoronoiNodePrefab.OffsetYInputField.SetText(NoSpace(_offset.Y));
        }
    }

    private float _scale = 1.0f;
    private Vector2 _offset = (0.0f, 0.0f);

    private int _scaleIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public VoronoiConnectorNode(UIVoronoiPrefab voronoiNodePrefab, VoronoiOperationType type)
    {
        VoronoiNodePrefab = voronoiNodePrefab;
        OutputGateConnector = new OutputGateConnector(VoronoiNodePrefab.OutputButton, this);
        VoronoiNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        VoronoiNodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref _scale, VoronoiNodePrefab.ScaleInputField, 1.0f, _scaleIndex));
        VoronoiNodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref _offset.X, VoronoiNodePrefab.OffsetXInputField, 0.0f, _offsetXIndex));
        VoronoiNodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref _offset.Y, VoronoiNodePrefab.OffsetYInputField, 0.0f, _offsetYIndex));

        VoronoiNodePrefab.ScaleTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        VoronoiNodePrefab.OffsetXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        VoronoiNodePrefab.OffsetYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        VoronoiNodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref _scale, VoronoiNodePrefab.ScaleInputField, 5f, _scaleIndex)); 
        VoronoiNodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref _offset.X, VoronoiNodePrefab.OffsetXInputField, 5f, _offsetXIndex));
        VoronoiNodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref _offset.Y, VoronoiNodePrefab.OffsetYInputField, 5f, _offsetYIndex));

        VoronoiNodePrefab.ScaleTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        VoronoiNodePrefab.OffsetXTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        VoronoiNodePrefab.OffsetYTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        VoronoiNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);

        Operation = VoronoiOperation.GetVoronoiOperation(type);
        Type = type;
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
        return [VoronoiNodePrefab];
    }

    public override UIController GetUIController()
    {
        return VoronoiNodePrefab.Collection.UIController;
    }

    public override string ToStringList() 
    {
        return 
            $"NodeType: Voronoi " +
            $"Values: {NoSpace(Scale)} {NoSpace(Offset)} {NoSpace((int)Type)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(VoronoiNodePrefab.Collection.Offset)}";
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