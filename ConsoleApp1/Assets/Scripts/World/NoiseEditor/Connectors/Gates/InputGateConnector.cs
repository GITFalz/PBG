using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class InputGateConnector : GateConnector
{
    public static InputGateConnector Empty = new InputGateConnector();

    public bool IsConnected = false;
    public OutputGateConnector? OutputGateConnector = null;
    public Vector3 Position => _button?.Center ?? Vector3.Zero;

    private UIButton _button;

    private InputGateConnector() { _button = UIButton.Empty; }
    public InputGateConnector(UIButton button, ConnectorNode node)
    {
        _button = button;
        Node = node;
    }

    public void Connect(OutputGateConnector output)
    {
        IsConnected = true;
        OutputGateConnector = output;
    }

    public void Disconnect()
    {
        IsConnected = false;
        OutputGateConnector = null;
    }

    public int GetOutputIndex()
    {
        return OutputGateConnector != null ? OutputGateConnector.Node.GetIndex(OutputGateConnector) : -1;
    }

    public string GetConnectedName()
    {
        return (OutputGateConnector != null && OutputGateConnector.Node != null) ? OutputGateConnector.Name : "None";
    }

    public bool GetConnectedNode([NotNullWhen(true)] out ConnectorNode? node)
    {
        if (IsConnected && OutputGateConnector != null)
        {
            node = OutputGateConnector.Node;
            return true;
        }
        
        node = null;
        return false;
    }
}