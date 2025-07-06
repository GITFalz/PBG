public class NNS_NodeOutput : NNS_NodeConnector
{
    public List<NNS_NodeInput> Inputs = new List<NNS_NodeInput>();
    public List<int> Indices = new List<int>();
    public string VariableName = "output";

    public NNS_NodeOutput(UIButton button, NNS_NodeBase node, NNS_ValueType type) : base(button, node, type)
    {
        Name = "Output";
    }

    public void Connect(NNS_NodeInput input)
    {
        if (Inputs.Contains(input))
            return;

        Inputs.Add(input);
        Indices.Add(-1);
        input.Output = this;

        IsConnected = true;
        input.IsConnected = true;
    }

    public override void Disconnect()
    {
        foreach (var input in Inputs)
        {
            input.Output = null;
            input.IsConnected = false;
        }

        IsConnected = false;
        Inputs = [];
        Indices = [];
    }

    public void SetIndex(NNS_NodeInput input, int index)
    {
        if (Inputs.Contains(input))
        {
            Indices[Inputs.IndexOf(input)] = index;
        }
    }

    public void Disconnect(NNS_NodeInput input)
    {
        if (Inputs.Remove(input))
        {
            Indices.Remove(Inputs.IndexOf(input));
            input.Output = null;
            input.IsConnected = false;
        }

        if (Inputs.Count == 0)
        {
            IsConnected = false;
        }
    }
    
    public bool IsConnectedTo(NNS_NodeInput input)
    {
        return Inputs.Contains(input);
    }
}