using OpenTK.Windowing.Common;

public class InitMaskConnectorNode : ConnectorNode
{
    public string Name => UIInitMaskNodePrefab.Name;
    public UIInitMaskNodePrefab UIInitMaskNodePrefab;

    public InputGateConnector ChildGateConnector;
    public InputGateConnector MaskGateConnector;
    public OutputGateConnector OutputGateConnector;

    public float Threshold {
        get {
            return _threshold; 
        } set {
            _threshold = value;
            UIInitMaskNodePrefab.ThresholdInputField.SetText(NoSpace(_threshold));
        }
    }
    
    private float _threshold = 0;

    private int _thresholdIndex = -1;

    public InitMaskConnectorNode(UIInitMaskNodePrefab initMaskNodePrefab)
    {
        UIInitMaskNodePrefab = initMaskNodePrefab;
        ChildGateConnector = new InputGateConnector(UIInitMaskNodePrefab.ChildButton, this);
        MaskGateConnector = new InputGateConnector(UIInitMaskNodePrefab.MaskButton, this);
        OutputGateConnector = new OutputGateConnector(UIInitMaskNodePrefab.OutputButton, this);
        UIInitMaskNodePrefab.ChildButton.SetOnClick(() => InputConnectionTest(ChildGateConnector));
        UIInitMaskNodePrefab.MaskButton.SetOnClick(() => InputConnectionTest(MaskGateConnector));
        UIInitMaskNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        UIInitMaskNodePrefab.ThresholdInputField.SetOnTextChange(() => SetValue(ref _threshold, UIInitMaskNodePrefab.ThresholdInputField, 0.5f, _thresholdIndex));

        UIInitMaskNodePrefab.ThresholdText.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        UIInitMaskNodePrefab.ThresholdText.SetOnHold(() => SetSlideValue(ref _threshold, UIInitMaskNodePrefab.ThresholdInputField, 5f, _thresholdIndex));

        UIInitMaskNodePrefab.ThresholdText.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
        UIInitMaskNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
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
        return [UIInitMaskNodePrefab];
    }

    public override UIController GetUIController()
    {
        return UIInitMaskNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            $"NodeType: InitMask " +
            $"Values: {NoSpace(Threshold)} " +
            $"Inputs: {NoSpace(ChildGateConnector.Name)} {NoSpace(MaskGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(UIInitMaskNodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _thresholdIndex = index;
        values.Add(Threshold);
        index++;
    }
}