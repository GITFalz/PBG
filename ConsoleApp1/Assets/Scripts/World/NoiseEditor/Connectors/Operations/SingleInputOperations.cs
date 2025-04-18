public abstract class SingleInputOperations
{
    public abstract float GetValue(float min, float max, float value);
    public abstract string GetFunction(float min, float max, string valueName);

    public static SingleInputOperations GetOperation(SingleInputOperationType type)
    {
        return type switch
        {
            SingleInputOperationType.Clamp => new SingleInputClampValueOperation(),
            SingleInputOperationType.Ignore => new SingleInputIgnoreValueOperation(),
            SingleInputOperationType.Lerp => new SingleInputLerpValueOperation(),
            SingleInputOperationType.Slide => new SingleInputSlideValueOperation(),
            SingleInputOperationType.Smooth => new SingleInputSmoothValueOperation(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class SingleInputClampValueOperation : SingleInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.Clamp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"ClampNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputIgnoreValueOperation : SingleInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return value < min || value > max ? 0.0f : value;
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"IgnoreNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputLerpValueOperation : SingleInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.Lerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"LerpNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputSlideValueOperation : SingleInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.SLerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"SlideNoiseValue({min}, {max}, {valueName})";
    }
}

public class SingleInputSmoothValueOperation : SingleInputOperations
{
    public override float GetValue(float min, float max, float value)
    {
        return Mathf.PLerp(min, max, value);
    }

    public override string GetFunction(float min, float max, string valueName)
    {
        return $"SmoothNoiseValue({min}, {max}, {valueName})";
    }
}

public enum SingleInputOperationType
{
    Clamp,
    Ignore,
    Lerp,
    Slide,
    Smooth
}
