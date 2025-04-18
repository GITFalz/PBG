using OpenTK.Mathematics;

public class SampleConnectorNode : ConnectorNode
{
    public string Name => SampleNodePrefab.Name;
    public UISampleNodePrefab SampleNodePrefab;

    public OutputGateConnector OutputGateConnector;

    public SampleConnectorNode(UISampleNodePrefab sampleNodePrefab)
    {
        SampleNodePrefab = sampleNodePrefab;
        OutputGateConnector = new OutputGateConnector(SampleNodePrefab.OutputButton, this);
        SampleNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));
    } 

    public override string GetLine()
    {
        string line = $"    float {VariableName} = SampleNoise(vec2(1.0, 1.0));";
        return line;
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
}