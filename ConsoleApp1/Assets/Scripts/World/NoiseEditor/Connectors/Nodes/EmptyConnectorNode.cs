
using OpenTK.Mathematics;

public class EmptyConnectorNode : ConnectorNode
{
    public override string GetLine()
    {
        throw new NotImplementedException();
    }

    public override void Select()
    {
        
    }

    public override void Deselect()
    {
        
    }

    public override void Move(Vector2 delta)
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

    public override void SetValueReferences(List<float> values, ref int index)
    {
        throw new NotImplementedException();
    }
}