using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class DisplayConnectorNode : ConnectorNode
{
    public string Name => DisplayNodePrefab.Name;
    public UIDisplayNodePrefab DisplayNodePrefab;

    public InputGateConnector InputGateConnector;

    public DisplayConnectorNode(UIDisplayNodePrefab displayNodePrefab)
    {
        DisplayNodePrefab = displayNodePrefab;
        InputGateConnector = new InputGateConnector(DisplayNodePrefab.InputButton, this);
        DisplayNodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
    }

    public override void Select()
    {
        
    }

    public override void Deselect()
    {
        
    }

    public override void Move(Vector2 delta)
    {
        
    }

    public override string GetLine()
    {
        string line = $"    display = ";
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            line += $"{InputGateConnector.OutputGateConnector.Node.VariableName};";
        }
        else
        {
            line += "0.0;";
        }
        return line;
    }

    public bool GetConnectedNode(out ConnectorNode node)
    {
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            node = InputGateConnector.OutputGateConnector.Node;
            return true;
        }
        else
        {
            node = Empty;
            return false;
        }
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector.OutputGateConnector.Node);
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes() 
    { 
        List<ConnectorNode> inputNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector.OutputGateConnector.Node);
        return inputNodes; 
    }

    public override List<ConnectorNode> GetOutputNodes() 
    { 
        return [];
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        return [InputGateConnector];
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [DisplayNodePrefab];
    }

    public override UIController GetUIController()
    {
        return DisplayNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            "NodeType: Display\n"+
            "{\n"+
            "    Inputs:\n"+
            "    {\n"+
            "        Name: "+NoSpace(InputGateConnector.Name)+"\n"+
            "    }\n"+
            "    Prefab:\n"+
            "    {\n"+
            "        Name: "+NoSpace(Name)+"\n"+
            "        Offset: "+NoSpace(DisplayNodePrefab.Collection.Offset)+"\n"+
            "    }\n"+
            "}\n";
        ;
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        // No values to set for display node
    }
}