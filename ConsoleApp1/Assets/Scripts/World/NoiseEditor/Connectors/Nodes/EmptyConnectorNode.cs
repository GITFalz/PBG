
public class EmptyConnectorNode : ConnectorNode
{
    public override string GetLine()
    {
        throw new NotImplementedException();
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        throw new NotImplementedException();
    }

    public override List<ConnectorNode> GetInputNodes()
    {
        throw new NotImplementedException();
    }

    public override List<ConnectorNode> GetOutputNodes()
    {
        throw new NotImplementedException();
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        throw new NotImplementedException();
    }

    public override UIController GetUIController()
    {
        throw new NotImplementedException();
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        throw new NotImplementedException();
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        throw new NotImplementedException();
    }

    public override string ToStringList()
    {
        throw new NotImplementedException();
    }
}