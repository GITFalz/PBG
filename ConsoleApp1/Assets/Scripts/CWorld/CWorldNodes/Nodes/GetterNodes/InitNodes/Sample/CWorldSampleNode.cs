using OpenTK.Mathematics;

public class CWorldSampleNode : CWorldGetterNode
{
    public Vector2 Offset = (0, 0);
    public Vector2 Scale = (1, 1);
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;


    private Vector2 _initPosition = (float.MaxValue, float.MaxValue);
    
    public CWorldSampleNode() : base() { }

    public override void Init(Vector2 position) 
    {
        if (position == _initPosition) 
            return;

        NoiseValue = SampleNoise(position);
        
        if (Invert) 
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
        _initPosition = position;
    }

    public override float GetValue()
    {
        return NoiseValue;
    }

    private float SampleNoise(Vector2 position)
    {
        Vector2 sampleCoord = position * Scale;
        Vector2 pos = sampleCoord + Offset;
        return (NoiseLib.Noise(pos) + 1) * 0.5f;
    }

    public override CWorldNode Copy()
    {
        return new CWorldSampleNode()
        {
            Name = Name,
            Offset = Offset,
            Scale = Scale,
            Amplitude = Amplitude,
            Invert = Invert,
        };;
    }
}