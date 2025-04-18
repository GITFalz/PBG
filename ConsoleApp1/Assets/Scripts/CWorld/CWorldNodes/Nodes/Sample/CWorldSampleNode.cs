using OpenTK.Mathematics;

public class CWorldSampleNode : CWorldNode
{
    public Vector2 Offset = (0, 0);
    public Vector2 Size = (1, 1);
    public List<CWorldSampleParameter> Parameters = new List<CWorldSampleParameter>();
    public float Amplitude = 1.0f;
    public bool Invert = false;

    public float NoiseValue = 0;
    
    public CWorldSampleNode(string name, Vector2 offset, Vector2 size, float amplitude, bool invert) : base()
    {
        Name = name;
        Offset = offset;
        Size = size;
        Amplitude = amplitude;
        Invert = invert;
    }

    public void Init(Vector2 position)
    {
        NoiseValue = (NoiseLib.Noise(position) + 1) * 0.5f;

        foreach (var parameter in Parameters)
            NoiseValue = parameter.GetValue(NoiseValue);
        
        if (Invert) 
            NoiseValue = 1 - NoiseValue;

        NoiseValue *= Amplitude;
    }
}