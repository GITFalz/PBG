using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class DoubleInputData : INodeData
{
    // Values
    public float Value1 = 1.0f;
    public float Value2 = 1.0f;
    public DoubleInputOperationType Type = DoubleInputOperationType.Add;

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
            Console.WriteLine("A double input node cannot have more than 2 values");
    }
    public void SetValue(int value, int index = 0) => Console.WriteLine("A display node cannot have a int value");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A display node cannot have a bool value");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A display node cannot have a vector4 value");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("A MinMax node cannot have a vector2 value");
    public void SetType(int type) => Type = (DoubleInputOperationType)type;
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
            Console.WriteLine("A double input node cannot have more than 2 inputs");
    }
    public void SetOutputName(string outputName) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIDoubleInputNodePrefab doubleInputNodePrefab = new UIDoubleInputNodePrefab(PrefabName, controller, PrefabOffset, Type);
        DoubleInputConnectorNode doubleInputNode = new DoubleInputConnectorNode(doubleInputNodePrefab, Type);

        doubleInputNode.InputGateConnector1.Name = InputName1;
        doubleInputNode.InputGateConnector2.Name = InputName2;
        doubleInputNode.OutputGateConnector.Name = OutputName;
        doubleInputNode.Value1 = Value1;
        doubleInputNode.Value2 = Value2;

        doubleInputNodePrefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return doubleInputNode;
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
        Type = (DoubleInputOperationType)Int.Parse(values[5]);
        InputName1 = values[7];
        InputName2 = values[8];
        OutputName = values[10];
        PrefabName = values[12]; 
        PrefabOffset = String.Parse.Vec4(values[13]);
        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Value1 = 1.0f;
        Value2 = 1.0f;
        Type = DoubleInputOperationType.Add;
        InputName1 = "none";
        InputName2 = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}