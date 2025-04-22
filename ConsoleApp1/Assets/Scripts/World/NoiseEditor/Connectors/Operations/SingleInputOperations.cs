public abstract class MinMaxInputOperations
{
    public abstract float GetValue(float min, float max, float value);
    public abstract string GetFunction(float min, float max, string valueName);
    public abstract string GetFunction(string min, string max, string valueName);

    public static MinMaxInputOperations GetOperation(MinMaxInputOperationType type)
    {
        return type switch
        {
            MinMaxInputOperationType.Clamp => new SingleInputClampValueOperation(),
            MinMaxInputOperationType.Ignore => new SingleInputIgnoreValueOperation(),
            MinMaxInputOperationType.Lerp => new SingleInputLerpValueOperation(),
            MinMaxInputOperationType.Slide => new SingleInputSlideValueOperation(),
            MinMaxInputOperationType.Smooth => new SingleInputSmoothValueOperation(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class SingleInputClampValueOperation : MinMaxInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.Clamp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"ClampNoiseValue({min}, {max}, {valueName})";
    }

    public override string GetFunction(string min, string max, string valueName)
    {
        return $"ClampNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputIgnoreValueOperation : MinMaxInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return value < min || value > max ? 0.0f : value;
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"IgnoreNoiseValue({min}, {max}, {valueName})";
    }

    public override string GetFunction(string min, string max, string valueName)
    {
        return $"IgnoreNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputLerpValueOperation : MinMaxInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.Lerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"LerpNoiseValue({min}, {max}, {valueName})";
    }

    public override string GetFunction(string min, string max, string valueName)
    {
        return $"LerpNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputSlideValueOperation : MinMaxInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.SLerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"SlideNoiseValue({min}, {max}, {valueName})";
    }

    public override string GetFunction(string min, string max, string valueName)
    {
        return $"SlideNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputSmoothValueOperation : MinMaxInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.PLerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"SmoothNoiseValue({min}, {max}, {valueName})";
    }

    public override string GetFunction(string min, string max, string valueName)
    {
        return $"SmoothNoiseValue({min}, {max}, {valueName})";
    }
}

public enum MinMaxInputOperationType
{
    Clamp = 0,
    Ignore = 1,
    Lerp = 2,
    Slide = 3,
    Smooth = 4
}
