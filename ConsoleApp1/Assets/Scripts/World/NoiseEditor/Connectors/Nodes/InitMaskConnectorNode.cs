using OpenTK.Windowing.Common;

public class InitMaskConnectorNode : ConnectorNode
{
    public string Name => NodePrefab.Name;
    public UIInitMaskNodePrefab NodePrefab;

    public InputGateConnector ChildGateConnector;
    public InputGateConnector MaskGateConnector;
    public OutputGateConnector OutputGateConnector;

    public float Threshold {
        get {
            return _threshold; 
        } set {
            _threshold = value;
            NodePrefab.ThresholdInputField.SetText(NoSpace(_threshold));
        }
    }
    
    private float _threshold = 0;

    private int _thresholdIndex = -1;

    public InitMaskConnectorNode(UIInitMaskNodePrefab initMaskNodePrefab)
    {
        NodePrefab = initMaskNodePrefab;
        ChildGateConnector = new InputGateConnector(NodePrefab.ChildButton, this);
        MaskGateConnector = new InputGateConnector(NodePrefab.MaskButton, this);
        OutputGateConnector = new OutputGateConnector(NodePrefab.OutputButton, this);
        NodePrefab.ChildButton.SetOnClick(() => InputConnectionTest(ChildGateConnector));
        NodePrefab.MaskButton.SetOnClick(() => InputConnectionTest(MaskGateConnector));
        NodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        NodePrefab.ThresholdInputField.SetOnTextChange(() => SetValue(ref _threshold, NodePrefab.ThresholdInputField, 0.5f, _thresholdIndex));

        NodePrefab.ThresholdText.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        NodePrefab.ThresholdText.SetOnHold(() => SetSlideValue(ref _threshold, NodePrefab.ThresholdInputField, 5f, _thresholdIndex));

        NodePrefab.ThresholdText.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
        NodePrefab.Collection.SetOnClick(() => { NoiseEditor.Selected = true; NoiseEditor.SelectedNode = this; });
    }

    public override void Select()
    {
        NodePrefab.SelectionImage.SetVisibility(true);
    }

    public override void Deselect()
    {
        NodePrefab.SelectionImage.SetVisibility(false);
    }

    public override string GetLine()
    {
        string thresholdValue = _thresholdIndex != -1 ? $"values[{_thresholdIndex}]" : NoSpace(Threshold);

        string line = $"    float {VariableName} = ";
        string child = ChildGateConnector.IsConnected && ChildGateConnector.OutputGateConnector != null
            ? ChildGateConnector.OutputGateConnector.Node.VariableName
            : "0.0";
        
        string mask = MaskGateConnector.IsConnected && MaskGateConnector.OutputGateConnector != null
            ? MaskGateConnector.OutputGateConnector.Node.VariableName
            : "1.0";

        line += $"{mask} > {thresholdValue} ? {child} : 0.0;";
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
            $"NodeType: InitMask " +
            $"Values: {NoSpace(Threshold)} " +
            $"Inputs: {NoSpace(ChildGateConnector.Name)} {NoSpace(MaskGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(NodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _thresholdIndex = index;
        values.Add(Threshold);
        index++;
    }
}