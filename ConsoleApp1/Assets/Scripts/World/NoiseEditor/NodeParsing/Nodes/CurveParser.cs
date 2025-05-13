using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class CurveData : INodeData
{
    public float Min = 0.0f;
    public float Max = 1.0f;
    public List<Vector4> Offsets = new();

    public string InputName = "none";
    public string OutputName = "none";

    public string PrefabName = "DisplayPrefab";
    public Vector4 PrefabOffset = (0, 0, 0, 0);

    private int _valueIndex = 0;
    private int _pointCount = 0;

    public void SetValue(float value)
    {
        if (_valueIndex == 0)
        {
            Min = value;
        }
        else if (_valueIndex == 1)
        {
            Max = value;
        }
        else
        {
            Console.WriteLine("Unexpected float value for Curve node.");
        }
        _valueIndex++;
    }

    public void SetValue(Vector4 value)
    {
        Offsets.Add(value);
    }

    public void SetValue(int value)
    {
        if (_pointCount == 0)
            _pointCount = value;
        else
            Console.WriteLine("Curve node already has a point count.");
    }

    public void SetValue(Vector2 value) => Console.WriteLine("Curve node does not accept Vector2 values.");

    public void SetValue(bool value) => Console.WriteLine("Curve node does not accept bool values.");
    public void SetType(int type) => Console.WriteLine("Curve node does not use a type.");

    public void SetInputName(string inputName) => InputName = inputName;
    public void SetOutputName(string outputName) => OutputName = outputName;
    public void SetPrefabName(string prefabName) => PrefabName = prefabName;
    public void SetPrefabOffset(Vector4 prefabOffset) => PrefabOffset = prefabOffset;

    public ConnectorNode GetConnectorNode(UIController controller)
    {
        var prefab = new UICurveNodePrefab(PrefabName, controller, PrefabOffset);

        foreach (var point in Offsets)
            prefab.CurveWindow.AddButton(point);

        prefab.CurveWindow.GenerateButtons();
        prefab.CurveWindow.UpdatePoints();

        var node = new CurveConnectorNode(prefab)
        {
            Min = Min,
            Max = Max
        };

        node.InputGateConnector.Name = InputName;
        node.OutputGateConnector.Name = OutputName;

        prefab.AddedMoveAction = NoiseNodeManager.UpdateLines;
        return node;
    }

    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 7)
        {
            connectorNode = null;
            return false;
        }

        Min = Float.Parse(values[3]);
        Max = Float.Parse(values[4]);
        _pointCount = Int.Parse(values[5], 0);

        Offsets.Clear();
        for (int i = 0; i < _pointCount; i++)
        {
            Offsets.Add(String.Parse.Vec4(values[6 + i]));
        }

        int index = 6 + _pointCount;
        InputName = values[index + 1];
        OutputName = values[index + 3];
        PrefabName = values[index + 5];
        PrefabOffset = String.Parse.Vec4(values[index + 6]);

        connectorNode = GetConnectorNode(controller);
        return true;
    }

    public void Clear()
    {
        Min = 0.0f;
        Max = 1.0f;
        _valueIndex = 0;
        _pointCount = 0;
        Offsets.Clear();
        InputName = "none";
        OutputName = "none";
        PrefabName = "DisplayPrefab";
        PrefabOffset = (0, 0, 0, 0);
    }
}
