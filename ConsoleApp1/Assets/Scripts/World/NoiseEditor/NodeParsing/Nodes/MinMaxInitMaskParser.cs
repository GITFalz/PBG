using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class MinMaxInitMaskData : INodeData
{
    // Values
    public float Min = 0.0f;
    public float Max = 1.0f;

    // Inputs
    public string ChildInputName = "none";
    public string MaskInputName = "none";

    // Output
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
        {
            Console.WriteLine("MinMaxInitMask node accepts only two float values: Min and Max.");
        }
    }

    public void SetValue(int value, int index = 0) => Console.WriteLine("MinMaxInitMask node does not accept int values.");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("MinMaxInitMask node does not accept bool values.");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("MinMaxInitMask node does not accept Vector2 values.");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("MinMaxInitMask node does not accept Vector4 values.");

    public void SetType(int type) => Console.WriteLine("MinMaxInitMask node does not use a type.");

    public void SetInputName(string inputName, int index = 0)
    {
        if (index == 0)
        {
            ChildInputName = inputName;
        }
        else if (index == 1)
        {
            MaskInputName = inputName;
        }
        else
        {
            Console.WriteLine("MinMaxInitMask node has only two inputs: Child and Mask.");
        }
    }

    public void SetOutputName(string outputName, int index) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        var prefab = new UIMinMaxInitMaskNodePrefab(PrefabName, controller, PrefabOffset);
        var node = new InitMaskMinMaxConnectorNode(prefab);

        node.ChildGateConnector.Name = ChildInputName;
        node.MaskGateConnector.Name = MaskInputName;
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
        ChildInputName = values[6];
        MaskInputName = values[7];
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
        ChildInputName = "none";
        MaskInputName = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
