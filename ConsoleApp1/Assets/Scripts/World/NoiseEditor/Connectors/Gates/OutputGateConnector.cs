using OpenTK.Mathematics;

public class OutputGateConnector : GateConnector
{
    public static OutputGateConnector Empty = new OutputGateConnector();

    public bool IsConnected = false;
    public InputGateConnector? InputGateConnector = null;
    public Vector3 Position => _button?.Center ?? Vector3.Zero;
    public int Index = -1;

    private UIButton _button;

    private OutputGateConnector() { _button = UIButton.Empty; }
    public OutputGateConnector(UIButton button, ConnectorNode node)
    {
        _button = button;
        Node = node;
    }

    public void Connect(InputGateConnector input)
    {
        IsConnected = true;
        InputGateConnector = input;

        input.IsConnected = true;
        input.OutputGateConnector = this;
    }

    public void Disconnect()
    {
        IsConnected = false;
        InputGateConnector = null;
    }
}