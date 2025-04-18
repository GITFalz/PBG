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

        output.IsConnected = true;
        output.InputGateConnector = this;
    }

    public void Disconnect()
    {
        IsConnected = false;
        OutputGateConnector = null;
    }
}