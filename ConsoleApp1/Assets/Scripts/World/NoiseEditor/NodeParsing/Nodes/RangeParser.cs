using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class RangeData : INodeData
{
    // Values
    private int _valueIndex = 0;
    public int Start = 0;
    public int Height = 1;
    public bool Flipped = false;

    // Inputs
    private int _inputIndex = 0;
    public string InputName1 = "none";
    public string InputName2 = "none";

    // Output
    public string OutputName = "none";

    // Prefab
    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    public void SetValue(int value)
    {
        if (_valueIndex == 0)
        {
            Start = value;
            _valueIndex++;
        }
        else if (_valueIndex == 1)
        {
            Height = value;
            _valueIndex++;
        }
        else
            Console.WriteLine("A Range node cannot have more than 2 int values");
    }

    public void SetValue(float value) => Console.WriteLine("A Range node does not accept float values");
    public void SetValue(bool value) => Flipped = value;
    public void SetValue(Vector2 value) => Console.WriteLine("A Range node does not accept Vector2 values");
    public void SetValue(Vector4 value) => Console.WriteLine("A Range node does not accept Vector4 values");

    public void SetType(int type) => Console.WriteLine("A Range node does not use a type");

    public void SetInputName(string inputName)
    {
        if (_inputIndex == 0)
        {
            InputName1 = inputName;
            _inputIndex++;
        }
        else if (_inputIndex == 1)
        {
            InputName2 = inputName;
            _inputIndex++;
        }
        else
            Console.WriteLine("A Range node cannot have more than 2 input names");
    }

    public void SetOutputName(string outputName) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        UIRangeNodePrefab prefab = new UIRangeNodePrefab(PrefabName, controller, PrefabOffset);
        RangeConnectorNode node = new RangeConnectorNode(prefab);
        node.StartGateConnector.Name = InputName1;
        node.HeightGateConnector.Name = InputName2;
        node.OutputGateConnector.Name = OutputName;
        node.Start = Start;
        node.Height = Height;
        node.Flipped = Flipped;
        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 14)
        {
            connectorNode = null;
            return false;
        }

        Start = Int.Parse(values[3]);
        Height = Int.Parse(values[4]);
        Flipped = values[5] == "1";

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
        _valueIndex = 0;
        Start = 0;
        Height = 1;
        Flipped = false;

        _inputIndex = 0;
        InputName1 = "none";
        InputName2 = "none";
        OutputName = "none";

        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
