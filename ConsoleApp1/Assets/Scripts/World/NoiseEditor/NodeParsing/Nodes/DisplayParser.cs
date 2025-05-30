using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

/// <summary>
/// NodeType: Display { Inputs: { Name: -name } Prefab: { Name: -name Offset: -offset } }
/// </summary>
public class DisplayData : INodeData
{   
    public string InputName = "none";
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(float value, int index = 0) => Console.WriteLine("A display node cannot have a float value");
    public void SetValue(int value, int index = 0) => Console.WriteLine("A display node cannot have a int value");
    public void SetValue(bool value, int index = 0) => Console.WriteLine("A display node cannot have a bool value");
    public void SetValue(Vector4 value, int index = 0) => Console.WriteLine("A display node cannot have a vector4 value");
    public void SetValue(Vector2 value, int index = 0) => Console.WriteLine("A MinMax node cannot have a vector2 value");
    public void SetType(int type) => Console.WriteLine("A display node cannot have a type");
    public void SetInputName(string inputName, int index = 0) => InputName = inputName;
    public void SetOutputName(string outputName, int index) => Console.WriteLine("A display node cannot have an output name");
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIDisplayNodePrefab displayNodePrefab = new UIDisplayNodePrefab(PrefabName, controller, PrefabOffset);
        DisplayConnectorNode displayNode = new DisplayConnectorNode(displayNodePrefab);
        displayNode.InputGateConnector.Name = InputName;
        displayNodePrefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return displayNode;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 7)
        {
            connectorNode = null;
            return false;
        }

        InputName = values[3];
        PrefabName = values[5];
        PrefabOffset = String.Parse.Vec4(values[6]);
        connectorNode = GetConnectorNode(controller);
        return true;
    }


    public void Clear()
    {
        InputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}