using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class SampleData : INodeData
{
    // Values
    public float Scale = 1.0f;
    public Vector2 Offset = Vector2.Zero;

    // Type
    public SampleOperationType Type = SampleOperationType.Basic;

    // Input
    public string InputName1 = "none";
    public string InputName2 = "none";

    // Output
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0)
    {
        if (index == 0)
        {
            Scale = value;
        }
        else
            Console.WriteLine("A Sample node only accepts one float (scale)");
    }

    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("Use SetOffset for Sample node vector2 offset");
    public void SetValue(Vector2 value, int index = 0) => Offset = value;
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A Sample node cannot have a bool value");
    public void SetValue(int value, int index = 0) => Console.WriteLine("A Sample node cannot have an int value");

    public void SetType(int type) => Type = (SampleOperationType)type;

    public void SetInputName(string inputName, int index)
    {
        if (index == 0)
        {
            InputName1 = inputName;
        }
        else if (index == 1)
        {
            InputName2 = inputName;
        }
        else
        {
            Console.WriteLine("A Sample node only accepts two input names");
        }
    }

    public void SetOutputName(string outputName, int index) => OutputName = outputName;

    public void SetPrefabName(string prefabName) => PrefabName = prefabName;

    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UISampleNodePrefab prefab = new UISampleNodePrefab(PrefabName, controller, PrefabOffset, Type);
        SampleConnectorNode node = new SampleConnectorNode(prefab, Type);
        node.InputGateConnector1.Name = InputName1;
        node.InputGateConnector2.Name = InputName2;
        node.OutputGateConnector.Name = OutputName;
        node.Scale = Scale;
        node.Offset = Offset;
        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 10)
        {
            connectorNode = null;
            return false;
        }

        Scale = Float.Parse(values[3]);
        Offset = String.Parse.Vec2(values[4]);
        OutputName = values[6];
        PrefabName = values[8];
        PrefabOffset = String.Parse.Vec4(values[9]);

        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Scale = 1.0f;
        Offset = Vector2.Zero;
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
