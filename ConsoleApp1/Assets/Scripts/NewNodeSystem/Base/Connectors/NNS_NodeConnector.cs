using OpenTK.Mathematics;

public abstract class NNS_NodeConnector(UIButton button, NNS_NodeBase node, NNS_ValueType type)
{
    public string Name = "Connector";
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return;
            Deselect(); // deselect whenever a connection modified
            _isConnected = value;
        }
    }
    public bool _isConnected = false;
    public bool IsSelected = false;

    public UIButton Button = button;
    public NNS_NodeBase Node = node;
    public NNS_ValueType Type = type;

    public Vector3 Position => Button?.Center ?? Vector3.Zero;

    public abstract void Disconnect();

    public bool Connected()
    {
        return IsConnected;
    }

    public void Select()
    {
        IsSelected = true;
        Button.TextureIndex = 10;
        Button.UpdateTexture();
    }

    public void Deselect()
    {
        IsSelected = false;
        Button.TextureIndex = 11;
        Button.UpdateTexture();
    }
}