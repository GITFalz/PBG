public class NNS_NodeField(NNS_NodeValue value, NNS_NodeInput? input)
{
    public NNS_NodeValue Value = value;
    public NNS_NodeInput? Input = input;

    public void SetValueReferences(List<float> values, ref int index)
    {
        Value.SetValueReferences(values, ref index);
    }

    public bool IsConnected()
    {
        return Input != null && Input.IsConnected;
    }

    public string GetVariable()
    {
        if (Input == null || !Input.IsConnected || Input.Output == null)
        {
            return Value.GetVariable();
        }
        else
        {
            return Input.Output.VariableName;
        }
    }
}