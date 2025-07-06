public class NNS_NodeInput : NNS_NodeConnector
{
    public NNS_NodeOutput? Output = null;
    public int Index = -1;

    public NNS_NodeInput(UIButton button, NNS_NodeBase node, NNS_ValueType type) : base(button, node, type)
    {
        Name = "Input";
    }

    public void Connect(NNS_NodeOutput output)
    {
        output.Connect(this); // list logic is handled in NNS_NodeOutput
    }

    public override void Disconnect()
    {
        Output?.Disconnect(this);
    }
}