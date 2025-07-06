using OpenTK.Mathematics;

public class NNS_SampleNode : NNS_NodeBase
{
    public NNS_SampleNode(Vector2 position) : 
    base("Sample", SAMPLE_NODE_COLOR, position) { }

    public override void InitializeNodeData()
    {
        RegisterOutputField("sample", NNS_ValueType.Float);
        RegisterInputField("scale", false, new Vector2(1, 1));
        RegisterInputField("offset", true, new Vector2(0, 0));
    }
}