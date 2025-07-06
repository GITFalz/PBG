using OpenTK.Mathematics;

public class NNS_OperationNode : NNS_NodeBase
{
    public NNS_OperationNode(Vector2 position, string type) :
    base ("Operation", DOUBLE_INPUT_COLOR, position, type) { } 

    public override void InitializeNodeData()
    {
        RegisterInputField("value1", true, 0.0f);
        RegisterInputField("value2", true, 0.0f);
        RegisterOutputField("result", NNS_ValueType.Float);

        RegisterAction("add", "operation", "+");
        RegisterAction("subtract", "operation", "-");
        RegisterAction("multiply", "operation", "*");
        RegisterAction("divide", "operation", "/");
        RegisterAction("max", "function", "max");
        RegisterAction("min", "function", "min");
        RegisterAction("mod", "function", "mod");
        RegisterAction("power", "function", "pow");
    }
}