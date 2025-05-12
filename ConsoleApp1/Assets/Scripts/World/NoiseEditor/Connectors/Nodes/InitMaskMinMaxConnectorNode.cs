using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class InitMaskMinMaxConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIMinMaxInitMaskNodePrefab NodePrefab;

    public InputGateConnector ChildGateConnector;
    public InputGateConnector MaskGateConnector;
    public OutputGateConnector OutputGateConnector;

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

    public InitMaskMinMaxConnectorNode(UIMinMaxInitMaskNodePrefab initMaskNodePrefab)
    {
        NodePrefab = initMaskNodePrefab;
        ChildGateConnector = new InputGateConnector(NodePrefab.ChildButton, this);
        MaskGateConnector = new InputGateConnector(NodePrefab.MaskButton, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);
        NodePrefab.ChildButton.SetOnClick(() => InputConnectionTest(ChildGateConnector));
        NodePrefab.MaskButton.SetOnClick(() => InputConnectionTest(MaskGateConnector));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.MinInputField.SetOnTextChange(() => SetValue(ref _min, NodePrefab.MinInputField, 0.0f, _minIndex));
        NodePrefab.MaxInputField.SetOnTextChange(() => SetValue(ref _max, NodePrefab.MaxInputField, 1.0f, _maxIndex));

        NodePrefab.MinText.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        NodePrefab.MaxText.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.MinText.SetOnHold(() => SetSlideValue(ref _min, NodePrefab.MinInputField, 5f, _minIndex));
        NodePrefab.MaxText.SetOnHold(() => SetSlideValue(ref _max, NodePrefab.MaxInputField, 5f, _maxIndex));

        NodePrefab.MinText.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        NodePrefab.MaxText.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
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
        string minValue = _minIndex != -1 ? $"values[{_minIndex}]" : NoSpace(Min);
        string maxValue = _maxIndex != -1 ? $"values[{_maxIndex}]" : NoSpace(Max);

        string line = $"    float {VariableName} = ";
        string child = ChildGateConnector.IsConnected && ChildGateConnector.OutputGateConnector != null
            ? ChildGateConnector.OutputGateConnector.Node.VariableName
            : "0.0";
        
        string mask = MaskGateConnector.IsConnected && MaskGateConnector.OutputGateConnector != null
            ? MaskGateConnector.OutputGateConnector.Node.VariableName
            : "1.0";

        line += $"({mask} >= {minValue} && {mask} < {maxValue}) ? {child} : 0.0;";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (ChildGateConnector.IsConnected && ChildGateConnector.OutputGateConnector != null)
            connectedNodes.Add(ChildGateConnector.OutputGateConnector.Node);

        if (MaskGateConnector.IsConnected && MaskGateConnector.OutputGateConnector != null)
            connectedNodes.Add(MaskGateConnector.OutputGateConnector.Node);

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
        if (ChildGateConnector.IsConnected && ChildGateConnector.OutputGateConnector != null)
            inputNodes.Add(ChildGateConnector.OutputGateConnector.Node);
        
        if (MaskGateConnector.IsConnected && MaskGateConnector.OutputGateConnector != null)
            inputNodes.Add(MaskGateConnector.OutputGateConnector.Node);

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
        return [ChildGateConnector, MaskGateConnector];
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
            $"NodeType: MinMaxInitMask " +
            $"Values: {NoSpace(Min)} {NoSpace(Max)} " +
            $"Inputs: {NoSpace(ChildGateConnector.Name)} {NoSpace(MaskGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(NodePrefab.Collection.Offset)}";
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