public abstract class BaseInputOperations
{
    public abstract float GetValue(float value);
    public abstract string GetFunction(string value);

    public static BaseInputOperations GetOperation(BaseInputOperationType type)
    {
        return type switch
        {
            BaseInputOperationType.Invert => new BaseInputInvertOperation(),
            BaseInputOperationType.Absolute => new BaseInputAbsoluteOperation(),
            BaseInputOperationType.Square => new BaseInputSquareOperation(),
            BaseInputOperationType.Sin => new BaseInputSinOperation(),
            BaseInputOperationType.Cos => new BaseInputCosOperation(),
            BaseInputOperationType.Tan => new BaseInputTanOperation(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class BaseInputInvertOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return 1-value;
    }

    public override string GetFunction(string value)
    {
        return $"1-{value}";
    }
}

public class BaseInputAbsoluteOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return Math.Abs(value);
    }

    public override string GetFunction(string value)
    {
        return $"abs({value})";
    }
}

public class BaseInputSquareOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return value * value;
    }

    public override string GetFunction(string value)
    {
        return $"{value} * {value}";
    }
}

public class BaseInputSinOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return (float)Math.Sin(value);
    }

    public override string GetFunction(string value)
    {
        return $"sin({value})";
    }
}

public class BaseInputCosOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return (float)Math.Cos(value);
    }

    public override string GetFunction(string value)
    {
        return $"cos({value})";
    }
}

public class BaseInputTanOperation : BaseInputOperations
{
    public override float GetValue(float value)
    {
        return (float)Math.Tan(value);
    }

    public override string GetFunction(string value)
    {
        return $"tan({value})";
    }
}


public enum BaseInputOperationType
    {
        Invert,
        Absolute,
        Square,
        Sin,
        Cos,
        Tan,
    }