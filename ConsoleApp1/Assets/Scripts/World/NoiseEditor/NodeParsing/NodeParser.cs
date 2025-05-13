using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public static class NodeParser
{
    public static INodeData? NodeData;

    public static CombineData CombineData = new();
    public static CurveData CurveData = new();
    public static DisplayData DisplayData = new();
    public static DoubleInputData DoubleInputData = new();
    public static MinMaxInitMaskData MinMaxInitMaskData = new();
    public static MinMaxInputOperationData MinMaxInputOperationData = new();
    public static RangeData RangeData = new();
    public static SampleData SampleData = new();
    public static ThresholdInitMaskData ThresholdInitMaskData = new();
    public static VoronoiData VoronoiData = new();

    public static List<ConnectorNode> ConnectorNodes = [];

    public static bool Parse(UIController controller, string[] lines, out int index)
    {
        index = 0;
        ConnectorNodes = [];
        CurrentParseAction = BaseParseActions;
        for (int i = 0; i < lines.Length; i++)
        {
            index = i;
            string line = lines[i];
            var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
                continue;
            if (line.StartsWith("Connections:"))
            {
                return true;
            }
            else if (line.StartsWith("NodeType:"))
            {
                var type = values[1].Trim();
                if (!_nodeDatas.TryGetValue(type, out INodeData? parser))
                {
                    Console.WriteLine($"Parser not found for {type}");
                    return false;
                }
                NodeData?.Clear();
                NodeData = parser;

                if (values.Length > 2) // is compacted version
                {
                    if (parser.GetConnectorNode(line, controller, out var node))
                    {
                        ConnectorNodes.Add(node);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse {line}");
                        return false;
                    }
                }
                else
                {
                    while (true)
                    {
                        line = lines[i];
                        values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                        if (values.Length == 0)
                            break;
                        string key = values[0].Trim();

                        if (!CurrentParseAction.TryGetValue(key, out ParseAction? action))
                        {
                            Console.WriteLine($"Action not found for {key}");
                            return false;
                        }
                        bool result = action(ref i, line, ref NodeData);
                        if (!result)
                        {
                            break;
                        }
                    }
                    ConnectorNodes.Add(NodeData.GetConnectorNode(controller));
                }
            }
        }

        return true;
    }

    public delegate bool ParseAction(ref int index, string line, ref INodeData input);
    public static readonly Dictionary<string, ParseAction> BaseParseActions = new Dictionary<string, ParseAction>() 
    {
        {"NodeType:", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        { "{", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        {
            "Values:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = ValuesParseActions;
                return true;
            }
        },
        {
            "Inputs:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = InputParseActions;
                return true;
            }
        },
        {
            "Outputs:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = OutputParseActions;
                return true;
            }
        },
        {
            "Prefab:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = PrefabParseActions;
                return true;
            }
        },
        {
            "}", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                return false;
            }
        }
    };

    public static readonly Dictionary<string, ParseAction> ValuesParseActions = new Dictionary<string, ParseAction>() 
    {
        { "{", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        {
            "Float:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetValue(Float.Parse(values[1]));
                }
                return true;
            }
        },
        {
            "Int:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetValue(Int.Parse(values[1]));
                }
                return true;
            }
        },
        {
            "Bool:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetValue(Int.Parse(values[1]) == 1);
                }
                return true;
            }
        },
        {
            "Vector4:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetValue(String.Parse.Vec4(values[1]));
                }
                return true;
            }
        },
        {
            "Vector2:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetValue(String.Parse.Vec2(values[1]));
                }
                return true;
            }
        },
        {
            "Type:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    input.SetType(Int.Parse(values[1]));
                }
                return true;
            }
        },
        {
            "}", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = BaseParseActions;
                return true;
            }
        }
    };

    public static readonly Dictionary<string, ParseAction> InputParseActions = new Dictionary<string, ParseAction>() 
    {
        { "{", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        {
            "Name:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                input.SetInputName(values.Length > 1 ? values[1].Trim() : "none");
                return true;
            }
        },
        {
            "}", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = BaseParseActions;
                return true;
            }
        }
    };

    public static readonly Dictionary<string, ParseAction> OutputParseActions = new Dictionary<string, ParseAction>() 
    {
        { "{", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        {
            "Name:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                input.SetOutputName(values.Length > 1 ? values[1].Trim() : "none");
                return true;
            }
        },
        {
            "}", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = BaseParseActions;
                return true;
            }
        }
    };

    public static readonly Dictionary<string, ParseAction> PrefabParseActions = new Dictionary<string, ParseAction>() 
    {
        { "{", (ref int index, string line, ref INodeData input) => 
            {
                index++; 
                return true;
            }
        },
        {
            "Name:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                input.SetPrefabName(values.Length > 1 ? values[1].Trim() : "none");
                return true;
            }
        },
        {
            "Offset:", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                input.SetPrefabOffset(values.Length > 1 ? String.Parse.Vec4(values[1]) : (0, 0, 0, 0));
                return true;
            }
        },
        {
            "}", (ref int index, string line, ref INodeData input) =>
            {
                index++;
                CurrentParseAction = BaseParseActions;
                return true;
            }
        }
    };

    public static Dictionary<string, ParseAction> CurrentParseAction = BaseParseActions;

    public static readonly Dictionary<string, INodeData> _nodeDatas = new()
    {
        { "Combine", CombineData },
        { "Curve", CurveData },
        { "Display", DisplayData },
        { "DoubleInputOperation", DoubleInputData },
        { "MinMaxInitMask", MinMaxInitMaskData },
        { "MinMaxInputOperation", MinMaxInputOperationData },
        { "Range", RangeData },
        { "Sample", SampleData },
        { "ThresholdInitMask", ThresholdInitMaskData },
        { "InitMask", ThresholdInitMaskData },
        { "Voronoi", VoronoiData }
    };
}

public interface INodeData
{
    public void SetValue(float value);
    public void SetValue(int value);
    public void SetValue(bool value);
    public void SetValue(Vector4 value);
    public void SetValue(Vector2 value);
    public void SetType(int type);
    public void SetInputName(string inputName);
    public void SetOutputName(string outputName);
    public void SetPrefabName(string prefabName);
    public void SetPrefabOffset(Vector4 prefabOffset);

    public ConnectorNode GetConnectorNode(UIController controller);
    public bool GetConnectorNode(string line, UIController controller, [NotNullWhen(true)] out ConnectorNode? node); // used for parsing the compacted version

    public void Clear();
}