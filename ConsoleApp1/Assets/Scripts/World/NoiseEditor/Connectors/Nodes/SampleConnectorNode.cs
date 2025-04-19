using OpenTK.Mathematics;

public class SampleConnectorNode : ConnectorNode
{
    public string Name => SampleNodePrefab.Name;
    public UISampleNodePrefab SampleNodePrefab;

    public OutputGateConnector OutputGateConnector;

    public float Scale = 1.0f;
    public Vector2 Offset = (0.0f, 0.0f);

    public SampleConnectorNode(UISampleNodePrefab sampleNodePrefab)
    {
        SampleNodePrefab = sampleNodePrefab;
        OutputGateConnector = new OutputGateConnector(SampleNodePrefab.OutputButton, this);
        SampleNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        SampleNodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref Scale, SampleNodePrefab.ScaleInputField, 1.0f));
        SampleNodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref Offset.X, SampleNodePrefab.OffsetXInputField, 0.0f));
        SampleNodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref Offset.Y, SampleNodePrefab.OffsetYInputField, 0.0f));

        SampleNodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref Scale, SampleNodePrefab.ScaleInputField, 0.1f)); 
        SampleNodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref Offset.X, SampleNodePrefab.OffsetXInputField, 0.1f));
        SampleNodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref Offset.Y, SampleNodePrefab.OffsetYInputField, 0.1f));

        sampleNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
    } 

    public override string GetLine()
    {
        return $"    float {VariableName} = SampleNoise({Mathf.ConvertGLSL(new Vector2(Scale, Scale))}, {Mathf.ConvertGLSL(Offset)});";
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            connectedNodes.Add(OutputGateConnector.InputGateConnector.Node);
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes() 
    { 
        return [];
    }

    public override List<ConnectorNode> GetOutputNodes() 
    { 
        List<ConnectorNode> outputNodes = [];
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            outputNodes.Add(OutputGateConnector.InputGateConnector.Node);
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
        return [SampleNodePrefab];
    }

    public override UIController GetUIController()
    {
        return SampleNodePrefab.Collection.UIController;
    }

    public override string ToStringList() 
    {
        return 
            $"NodeType: Sample " +
            $"Values: {NoSpace(Scale)} {NoSpace(Offset)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(SampleNodePrefab.Collection.Offset)}";
    }
}