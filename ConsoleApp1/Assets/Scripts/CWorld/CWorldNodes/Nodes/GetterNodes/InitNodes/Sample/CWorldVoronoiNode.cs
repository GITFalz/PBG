using OpenTK.Mathematics;

public class CWorldVoronoiNode : CWorldParameterNode
{
    public static CWorldVoronoiNode Basic => new CWorldVoronoiNode(VoronoiOperationType.Basic);
    public static CWorldVoronoiNode Edge => new CWorldVoronoiNode(VoronoiOperationType.Edge);
    public static CWorldVoronoiNode Distance => new CWorldVoronoiNode(VoronoiOperationType.Distance);

    public float[] CachedValues = new float[3];

    public Vector2 Offset = (0, 0);
    public Vector2 Scale = (1, 1);
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;
    public VoronoiOperation VoronoiOperation;
    public VoronoiOperationType Type;

    public CWorldGetterNode InputNode1 = new CWorldEmptyNode("Empty");
    public int InputNode1Index = 0;
    public CWorldGetterNode InputNode2 = new CWorldEmptyNode("Empty");
    public int InputNode2Index = 0;

    public float Value1
    {
        get => InputNode1.GetCachedValue(InputNode1Index);
        set => InputNode1.SetValue(value);
    }

    public float Value2
    {
        get => InputNode2.GetCachedValue(InputNode2Index);
        set => InputNode2.SetValue(value);
    }


    private Vector2 _initPosition = (float.MaxValue, float.MaxValue);

    public CWorldVoronoiNode(VoronoiOperationType type) : base()
    {
        VoronoiOperation = VoronoiOperation.GetVoronoiOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        if (position == _initPosition)
            return;

        InputNode1.Init(position);
        InputNode2.Init(position);
        Offset = (Value1, Value2);
        NoiseValue = SampleVoronoi(position, out var cell);

        if (Invert)
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
        CachedValues[0] = NoiseValue;
        CachedValues[1] = cell.X;
        CachedValues[2] = cell.Y;
        _initPosition = position;
    }

    private float SampleVoronoi(Vector2 position, out Vector2 cell)
    {
        Vector2 sampleCoord = position * Scale;
        Vector2 pos = sampleCoord + Offset;
        return VoronoiOperation.GetValue(pos, out cell);
    }

    public override float GetCachedValue(int index)
    {
        if (index < 0 || index >= CachedValues.Length)
            throw new IndexOutOfRangeException("Index must be between 0 and " + (CachedValues.Length - 1));

        //Console.WriteLine($"GetCachedValue called with index: {index}, returning: {CachedValues[index]}");
        return CachedValues[index];
    }

    public override CWorldNode Copy()
    {
        return new CWorldVoronoiNode(Type)
        {
            Name = Name,
            Offset = Offset,
            Scale = Scale,
            Amplitude = Amplitude,
            Invert = Invert,
            InputNode1Index = InputNode1Index,
            InputNode2Index = InputNode2Index,
        };
    }
    
    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (InputNode1.IsntEmpty())
        {
            string inputName1 = nodeNameMap[InputNode1];
            ((CWorldVoronoiNode)copiedNode).InputNode1 = (CWorldGetterNode)copiedNodes[inputName1];
        }
        if (InputNode2.IsntEmpty())
        {
            string inputName2 = nodeNameMap[InputNode2];
            ((CWorldVoronoiNode)copiedNode).InputNode2 = (CWorldGetterNode)copiedNodes[inputName2];
        }
    }
}