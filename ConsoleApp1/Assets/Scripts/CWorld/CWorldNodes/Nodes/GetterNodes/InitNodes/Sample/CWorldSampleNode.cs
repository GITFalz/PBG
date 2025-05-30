using OpenTK.Mathematics;

public class CWorldSampleNode : CWorldParameterNode
{
    public Vector2 Offset = (0, 0);
    public Vector2 Scale = (1, 1);
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;

    public SampleOperation SampleOperation;
    public SampleOperationType Type;

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

    public CWorldSampleNode(SampleOperationType type) : base()
    { 
        SampleOperation = SampleOperation.GetSampleOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        if (position == _initPosition)
            return;

        InputNode1.Init(position);
        InputNode2.Init(position);
        Offset = (Value1, Value2);
        NoiseValue = SampleNoise(position);

        if (Invert)
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
        CachedValue = NoiseValue;
        _initPosition = position;
    }

    private float SampleNoise(Vector2 position)
    {
        Vector2 sampleCoord = position * Scale;
        Vector2 pos = sampleCoord + Offset;
        return (SampleOperation.GetValue(pos)+ 1) * 0.5f;
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }

    public override CWorldNode Copy()
    {
        return new CWorldSampleNode(Type)
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
            ((CWorldSampleNode)copiedNode).InputNode1 = (CWorldGetterNode)copiedNodes[inputName1];
        }
        if (InputNode2.IsntEmpty())
        {
            string inputName2 = nodeNameMap[InputNode2];
            ((CWorldSampleNode)copiedNode).InputNode2 = (CWorldGetterNode)copiedNodes[inputName2];
        }
    }
}