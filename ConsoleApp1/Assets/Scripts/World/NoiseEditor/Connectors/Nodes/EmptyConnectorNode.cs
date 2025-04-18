
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
}