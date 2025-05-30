using OpenTK.Mathematics;

public class OutputGateConnector : GateConnector
{
    public string VariableName = "output";
    public static OutputGateConnector Empty = new OutputGateConnector();

    public bool IsConnected = false;
    public List<InputGateConnector> InputGateConnectors = [];
    public Vector3 Position => _button?.Center ?? Vector3.Zero;
    public List<int> Indices = [];

    public UIButton _button;

    private OutputGateConnector() { _button = UIButton.Empty; }
    public OutputGateConnector(UIButton button, ConnectorNode node)
    {
        _button = button;
        Node = node;
    }

    public void SetIndex(InputGateConnector input, int index)
    {
        if (InputGateConnectors.Contains(input))
        {
            Indices[InputGateConnectors.IndexOf(input)] = index;
        }
    }

    public int GetIndex()
    {
        return Node.GetIndex(this);
    }

    public void Connect(InputGateConnector input)
    {
        IsConnected = true;
        Indices.Add(-1);
        InputGateConnectors.Add(input);

        input.IsConnected = true;
        input.OutputGateConnector = this;
    }

    public void Disconnect()
    {
        IsConnected = false;
        Indices.Clear();
        InputGateConnectors.Clear();
    }

    public void Disconnect(InputGateConnector input)
    {
        if (InputGateConnectors.Contains(input))
        {
            Indices.Remove(InputGateConnectors.IndexOf(input));
            InputGateConnectors.Remove(input);
        }
    }

    public override string ToString()
    {
        return VariableName;
    }
}