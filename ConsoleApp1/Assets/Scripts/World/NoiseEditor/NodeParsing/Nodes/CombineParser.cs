using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class CombineData : INodeData
{
    // Values
    public float Value1 = 1.0f;
    public float Value2 = 1.0f;

    // Inputs
    public string InputName1 = "none";
    public string InputName2 = "none";

    // Outputs
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0)
    {
        if (index == 0)
        {
            Value1 = value;
        }
        else if (index == 1)
        {
            Value2 = value;
        }
        else
            Console.WriteLine("A combine node cannot have more than 2 values");
    }

    public void SetValue(int value, int index = 0) => Console.WriteLine("A combine node cannot have a int value");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A combine node cannot have a bool value");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A combine node cannot have a vector4 value");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("A MinMax node cannot have a vector2 value");
    public void SetType(int type) => Console.WriteLine("A combine node cannot have a type");
    public void SetInputName(string inputName, int index = 0)
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
            Console.WriteLine("A combine node cannot have more than 2 inputs");
    }
    public void SetOutputName(string outputName, int index) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;
    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UICombineNodePrefab combineNodePrefab = new UICombineNodePrefab(PrefabName, controller, PrefabOffset);
        CombineConnectorNode combineNode = new CombineConnectorNode(combineNodePrefab);
        combineNode.InputGateConnector1.Name = InputName1;
        combineNode.InputGateConnector2.Name = InputName2;
        combineNode.OutputGateConnector.Name = OutputName;
        combineNode.Value1 = Value1;
        combineNode.Value2 = Value2;
        combineNodePrefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return combineNode;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 7)
        {
            connectorNode = null;
            return false;
        }

        Value1 = Float.Parse(values[3]);
        Value2 = Float.Parse(values[4]);
        InputName1 = values[6];
        InputName2 = values[7];
        OutputName = values[9];
        PrefabName = values[11];
        PrefabOffset = String.Parse.Vec4(values[12]);
        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Value1 = 1.0f;
        Value2 = 1.0f;

        InputName1 = "none";
        InputName2 = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}