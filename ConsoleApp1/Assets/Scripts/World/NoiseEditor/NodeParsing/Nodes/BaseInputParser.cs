using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class BaseInputData : INodeData
{
    public BaseInputOperationType Type = BaseInputOperationType.Invert;

    // Inputs
    public string InputName = "none";

    // Outputs
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0) => Console.WriteLine("A base input node cannot have a float value");
    public void SetValue(int value, int index = 0) => Console.WriteLine("A base input node cannot have a int value");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A base input node cannot have a bool value");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A base input node cannot have a vector4 value");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("A base input node cannot have a vector2 value");
    public void SetType(int type) => Type = (BaseInputOperationType)type;
    public void SetInputName(string inputName, int index = 0)
    {
        if (index == 0)
        {
            InputName = inputName;
        }
        else
            Console.WriteLine("A Base input node cannot have more than 2 inputs");
    }
    public void SetOutputName(string outputName, int index) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIBaseInputNodePrefab BaseInputNodePrefab = new UIBaseInputNodePrefab(PrefabName, controller, PrefabOffset, Type);
        BaseInputConnectorNode BaseInputNode = new BaseInputConnectorNode(BaseInputNodePrefab, Type);

        BaseInputNode.InputGateConnector.Name = InputName;
        BaseInputNode.OutputGateConnector.Name = OutputName;

        BaseInputNodePrefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return BaseInputNode;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 7)
        {
            connectorNode = null;
            return false;
        }

        Type = (BaseInputOperationType)Int.Parse(values[3]);
        InputName = values[5];
        OutputName = values[7];
        PrefabName = values[9]; 
        PrefabOffset = String.Parse.Vec4(values[10]);
        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Type = BaseInputOperationType.Invert;
        InputName = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}