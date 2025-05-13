using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class SampleData : INodeData
{
    // Values
    private int _valueIndex = 0;
    public float Scale = 1.0f;
    public Vector2 Offset = Vector2.Zero;

    // Output
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value)
    {
        if (_valueIndex == 0)
        {
            Scale = value;
            _valueIndex++;
        }
        else
            Console.WriteLine("A Sample node only accepts one float (scale)");
    }

    public void SetValue(Vector4 value) => Console.WriteLine("Use SetOffset for Sample node vector2 offset");
    public void SetValue(Vector2 value) => Offset = value;
    public void SetValue(bool value) => Console.WriteLine("A Sample node cannot have a bool value");
    public void SetValue(int value) => Console.WriteLine("A Sample node cannot have an int value");

    public void SetType(int type) => Console.WriteLine("A Sample node does not have a type");

    public void SetInputName(string inputName) => Console.WriteLine("A Sample node has no input connectors");

    public void SetOutputName(string outputName) => OutputName = outputName;

    public void SetPrefabName(string prefabName) => PrefabName = prefabName;

    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UISampleNodePrefab prefab = new UISampleNodePrefab(PrefabName, controller, PrefabOffset);
        SampleConnectorNode node = new SampleConnectorNode(prefab);
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
        _valueIndex = 0;
        Scale = 1.0f;
        Offset = Vector2.Zero;
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
