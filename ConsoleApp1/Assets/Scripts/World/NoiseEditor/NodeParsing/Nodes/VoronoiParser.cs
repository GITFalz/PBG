using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class VoronoiData : INodeData
{
    // Values
    public float Scale = 1.0f;
    public Vector2 Offset = Vector2.Zero;

    // Type
    public VoronoiOperationType Type = 0;

    // Input
    public string InputName1 = "none";
    public string InputName2 = "none";

    // Output
    public string OutputName = "none";
    public string OutputCellX = "none";
    public string OutputCellY = "none";

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
            Console.WriteLine("A Voronoi node only accepts one float (scale)");
    }

    public void SetValue(Vector2 value, int index = 0) => Offset = value;
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A Voronoi node does not accept a Vector4");
    public void SetValue(int value, int index = 0) => Console.WriteLine("Use SetType to set the operation type");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A Voronoi node does not accept a bool");

    public void SetType(int type) => Type = (VoronoiOperationType)type;

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
            Console.WriteLine("A Voronoi node only accepts two input names");
        }
    }

    public void SetOutputName(string outputName, int index)
    {
        if (index == 0)
        {
            OutputName = outputName;
        }
        else if (index == 1)
        {
            OutputCellX = outputName;
        }
        else if (index == 2)
        {
            OutputCellY = outputName;
        }
        else
        {
            Console.WriteLine("A Voronoi node only accepts three output names");
        }
    }

    public void SetPrefabName(string prefabName) => PrefabName = prefabName;

    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIVoronoiPrefab prefab = new UIVoronoiPrefab(PrefabName, controller, PrefabOffset, Type);
        VoronoiConnectorNode node = new VoronoiConnectorNode(prefab, Type);
        node.InputGateConnector1.Name = InputName1;
        node.InputGateConnector2.Name = InputName2;
        node.Output.Name = OutputName;
        node.Scale = Scale;
        node.Offset = Offset;
        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 11)
        {
            connectorNode = null;
            return false;
        }

        Scale = Float.Parse(values[3]);
        Offset = String.Parse.Vec2(values[4]);
        SetType(Int.Parse(values[5]));
        OutputName = values[7];
        PrefabName = values[9];
        PrefabOffset = String.Parse.Vec4(values[10]);

        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Scale = 1.0f;
        Offset = Vector2.Zero;
        Type = 0;
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
