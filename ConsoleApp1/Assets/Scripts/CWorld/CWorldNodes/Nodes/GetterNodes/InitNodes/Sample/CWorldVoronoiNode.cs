using OpenTK.Mathematics;

public class CWorldVoronoiNode : CWorldInitNode
{
    public Vector2 Offset = (0, 0);
    public Vector2 Scale = (1, 1);
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;
    public VoronoiOperation VoronoiOperation;
    public VoronoiOperationType Type;
    
    public CWorldVoronoiNode(VoronoiOperationType type) : base() 
    {
        VoronoiOperation = VoronoiOperation.GetVoronoiOperation(type);
        Type = type;
    }

    public override void Init(Vector2 position)
    {
        NoiseValue = SampleVoronoi(position);
        
        if (Invert) 
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
    }

    public override float GetValue()
    {
        return NoiseValue;
    }

    private float SampleVoronoi(Vector2 position)
    {
        Vector2 sampleCoord = position * Scale;
        Vector2 pos = sampleCoord + Offset;
        return VoronoiOperation.GetValue(pos);
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
        };;
    }
}