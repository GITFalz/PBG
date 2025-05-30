using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class ThresholdInitMaskData : INodeData
{
    // Value
    public float Threshold = 0.5f;

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
        Threshold = value;
    }

    public void SetValue(int value, int index = 0) => Console.WriteLine("ThresholdInitMask node does not accept int values");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("ThresholdInitMask node does not accept bool values");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("ThresholdInitMask node does not accept Vector2 values");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("ThresholdInitMask node does not accept Vector4 values");

    public void SetType(int type) => Console.WriteLine("ThresholdInitMask node does not use a type");

    public void SetInputName(string inputName, int index)
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
            Console.WriteLine("ThresholdInitMask node has only two inputs");
    }

    public void SetOutputName(string outputName, int index) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        var prefab = new UIThresholdInitMaskNodePrefab(PrefabName, controller, PrefabOffset);
        var node = new InitMaskThresholdConnectorNode(prefab);
        node.ChildGateConnector.Name = ChildInputName;
        node.MaskGateConnector.Name = MaskInputName;
        node.OutputGateConnector.Name = OutputName;
        node.Threshold = Threshold;
        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 12)
        {
            connectorNode = null;
            return false;
        }

        Threshold = Float.Parse(values[3]);
        ChildInputName = values[5];
        MaskInputName = values[6];
        OutputName = values[8];
        PrefabName = values[10];
        PrefabOffset = String.Parse.Vec4(values[11]);

        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Threshold = 0.5f;
        ChildInputName = "none";
        MaskInputName = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
