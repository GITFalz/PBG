using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class DirectSampleData : INodeData
{
    // Values
    public float Scale = 1.0f;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Position = Vector2.Zero;

    // Input
    public string InputPosX = "none";
    public string InputPosY = "none";
    public string InputName1 = "none";
    public string InputName2 = "none";

    // Output
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "Direct Sample Prefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0)
    {
        if (index == 0)
        {
            Scale = value;
        }
        else
            Console.WriteLine("A Direct Sample node only accepts one float (scale)");
    }

    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("Use SetOffset for Direct Sample node vector2 offset");
    public void SetValue(Vector2 value, int index = 0)
    {
        if (index == 0)
        {
            Offset = value;
        }
        else if (index == 1)
        {
            Position = value;
        }
        else
        {
            Console.WriteLine("A Direct Sample node only accepts two vector2 values (offset and position)");
        }
    }
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A Direct Sample node cannot have a bool value");
    public void SetValue(int value, int index = 0) => Console.WriteLine("A Direct Sample node cannot have an int value");

    public void SetType(int type) => Console.WriteLine("A Direct Sample node does not have a type");

    public void SetInputName(string inputName, int index)
    {
        if (index == 0)
        {
            InputPosX = inputName;
        }
        else if (index == 1)
        {
            InputPosY = inputName;
        }
        else if (index == 2)
        {
            InputName1 = inputName;
        }
        else if (index == 3)
        {
            InputName2 = inputName;
        }
        else
        {
            Console.WriteLine("A Direct Sample node only accepts four input names (two for position and two for values)");
        }
    }

    public void SetOutputName(string outputName, int index) => OutputName = outputName;

    public void SetPrefabName(string prefabName) => PrefabName = prefabName;

    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIDirectSampleNodePrefab prefab = new UIDirectSampleNodePrefab(PrefabName, controller, PrefabOffset);
        DirectSampleConnectorNode node = new DirectSampleConnectorNode(prefab);
        node.InputPosXConnector.Name = InputPosX;
        node.InputPosYConnector.Name = InputPosY;
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
