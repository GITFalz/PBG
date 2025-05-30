using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class MinMaxInputOperationData : INodeData
{
    // Values
    public float Min = 0.0f;
    public float Max = 1.0f;

    // Type
    public MinMaxInputOperationType Type = MinMaxInputOperationType.Clamp;

    // Inputs
    public string InputName = "none";

    // Outputs
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0)
    {
        if (index == 0)
        {
            Min = value;
        }
        else if (index == 1)
        {
            Max = value;
        }
        else
            Console.WriteLine("A MinMax node cannot have more than 2 values");
    }

    public void SetValue(int value, int index = 0) => Console.WriteLine("Use SetType to define the operation type");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A MinMax node cannot have a bool value");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A MinMax node cannot have a vector4 value");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("A MinMax node cannot have a vector2 value");

    public void SetType(int type) => Type = (MinMaxInputOperationType)type;

    public void SetInputName(string inputName, int index) => InputName = inputName;

    public void SetOutputName(string outputName, int index) => OutputName = outputName;

    public void SetPrefabName(string prefabName) => PrefabName = prefabName;

    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIMinMaxInputNodePrefab prefab = new UIMinMaxInputNodePrefab(PrefabName, controller, PrefabOffset, Type);
        MinMaxInputOperationConnectorNode node = new MinMaxInputOperationConnectorNode(prefab, Type);
        node.InputGateConnector.Name = InputName;
        node.OutputGateConnector.Name = OutputName;
        node.Min = Min;
        node.Max = Max;
        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 13)
        {
            connectorNode = null;
            return false;
        }

        Min = Float.Parse(values[3]);
        Max = Float.Parse(values[4]);
        SetType(Int.Parse(values[5]));
        InputName = values[7];
        OutputName = values[9];
        PrefabName = values[11];
        PrefabOffset = String.Parse.Vec4(values[12]);

        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Min = 0.0f;
        Max = 1.0f;
        Type = 0;
        InputName = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
