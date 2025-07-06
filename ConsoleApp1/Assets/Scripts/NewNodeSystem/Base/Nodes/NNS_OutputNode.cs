using OpenTK.Mathematics;

/// <summary>
/// This node is pre defined because it is essential for the node system.
/// </summary>
public class NNS_OutputNode : NNS_NodeBase
{
    public NNS_OutputNode(Vector2 position) :
    base("Output", (0.5f, 0.5f, 0.5f, 1f), position) { }

    public override void InitializeNodeData()
    {
        RegisterInputField("result", NNS_ValueType.Float);
    }
}