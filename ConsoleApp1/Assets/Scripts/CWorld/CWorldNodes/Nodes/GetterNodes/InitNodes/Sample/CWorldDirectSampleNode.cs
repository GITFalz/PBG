using OpenTK.Mathematics;

public class CWorldDirectSampleNode : CWorldParameterNode
{
    public Vector2 Position = (0, 0);
    public Vector2 Offset = (0, 0);
    public Vector2 Scale = (1, 1);
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;

    public CWorldGetterNode PosXNode = new CWorldEmptyNode("PosX");
    public int PosXNodeIndex = 0;
    public CWorldGetterNode PosYNode = new CWorldEmptyNode("PosY");
    public int PosYNodeIndex = 0;
    public CWorldGetterNode InputNode1 = new CWorldEmptyNode("Empty");
    public int InputNode1Index = 0;
    public CWorldGetterNode InputNode2 = new CWorldEmptyNode("Empty");
    public int InputNode2Index = 0;

    public float PosX
    {
        get => PosXNode.GetCachedValue(PosXNodeIndex);
        set => PosXNode.SetValue(value);
    }

    public float PosY
    {
        get => PosYNode.GetCachedValue(PosYNodeIndex);
        set => PosYNode.SetValue(value);
    }

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

    public CWorldDirectSampleNode() : base(){ }

    public override void Init(Vector2 position)
    {
        if (position == _initPosition)
            return;

        PosXNode.Init(position);
        PosYNode.Init(position);
        InputNode1.Init(position);
        InputNode2.Init(position);
        Offset = (Value1, Value2);
        Position = (PosX, PosY);
        NoiseValue = SampleNoise();

        if (Invert)
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
        CachedValue = NoiseValue;
        _initPosition = position;
    }

    private float SampleNoise()
    {
        Vector2 pos = (Position * Scale) + Offset;
        return (NoiseLib.Noise(pos) + 1) * 0.5f;
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }

    public override CWorldNode Copy()
    {
        return new CWorldDirectSampleNode()
        {
            Name = Name,
            Position = Position,
            Offset = Offset,
            Scale = Scale,
            Amplitude = Amplitude,
            Invert = Invert,
            PosXNodeIndex = PosXNodeIndex,
            PosYNodeIndex = PosYNodeIndex,
            InputNode1Index = InputNode1Index,
            InputNode2Index = InputNode2Index,
        };
    }
    
    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (PosXNode.IsntEmpty())
        {
            string posXName = nodeNameMap[PosXNode];
            ((CWorldDirectSampleNode)copiedNode).PosXNode = (CWorldGetterNode)copiedNodes[posXName];
        }
        if (PosYNode.IsntEmpty())
        {
            string posYName = nodeNameMap[PosYNode];
            ((CWorldDirectSampleNode)copiedNode).PosYNode = (CWorldGetterNode)copiedNodes[posYName];
        }
        if (InputNode1.IsntEmpty())
        {
            string inputName1 = nodeNameMap[InputNode1];
            ((CWorldDirectSampleNode)copiedNode).InputNode1 = (CWorldGetterNode)copiedNodes[inputName1];
        }
        if (InputNode2.IsntEmpty())
        {
            string inputName2 = nodeNameMap[InputNode2];
            ((CWorldDirectSampleNode)copiedNode).InputNode2 = (CWorldGetterNode)copiedNodes[inputName2];
        }
    }
}