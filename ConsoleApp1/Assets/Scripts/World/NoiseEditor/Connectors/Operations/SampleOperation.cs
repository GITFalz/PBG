using OpenTK.Mathematics;

public abstract class SampleOperation
{
    public abstract float GetValue(Vector2 position);
    public abstract string GetFunction(string a, string b);

    public static SampleOperation GetSampleOperation(SampleOperationType type)
    {
        return type switch
        {
            SampleOperationType.Basic => new SampleOperationBasic(),
            SampleOperationType.Angle => new SampleOperationAngle(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class SampleOperationBasic : SampleOperation
{
    public override float GetValue(Vector2 position)
    {
        return NoiseLib.Noise(position);
    }

    public override string GetFunction(string a, string b)
    {
        return $"SampleNoise({a}, {b})";
    }
}

public class SampleOperationAngle : SampleOperation
{
    public override float GetValue(Vector2 position)
    {
        return NoiseLib.AngleNoise(position);
    }

    public override string GetFunction(string a, string b)
    {
        return $"SampleAngle({a}, {b})";
    }
}

public enum SampleOperationType
{
    Basic,
    Angle
}