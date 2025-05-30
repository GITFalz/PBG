public abstract class DoubleInputOperations
{
    public abstract float GetValue(float value1, float value2);
    public abstract string GetFunction(string value1, string value2);
    private static readonly DoubleInputOperationType[] _types = 
    {
        DoubleInputOperationType.Add,
        DoubleInputOperationType.Subtract,
        DoubleInputOperationType.Multiply,
        DoubleInputOperationType.Divide,
        DoubleInputOperationType.Max,
        DoubleInputOperationType.Min,
        DoubleInputOperationType.Mod,
        DoubleInputOperationType.Power
    };

    public static DoubleInputOperations GetOperation(DoubleInputOperationType type)
    {
        return type switch
        {
            DoubleInputOperationType.Add => new DoubleInputAddValueOperation(),
            DoubleInputOperationType.Subtract => new DoubleInputSubtractValueOperation(),
            DoubleInputOperationType.Multiply => new DoubleInputMultiplyValueOperation(),
            DoubleInputOperationType.Divide => new DoubleInputDivideValueOperation(),
            DoubleInputOperationType.Max => new DoubleInputMaxValueOperation(),
            DoubleInputOperationType.Min => new DoubleInputMinValueOperation(),
            DoubleInputOperationType.Mod => new DoubleInputModValueOperation(),
            DoubleInputOperationType.Power => new DoubleInputPowerValueOperation(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class DoubleInputAddValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return value1 + value2;
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"{value1} + {value2}";
    }
}

public class DoubleInputSubtractValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return value1 - value2;
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"{value1} - {value2}";
    }
}

public class DoubleInputMultiplyValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return value1 * value2;
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"{value1} * {value2}";
    }
}

public class DoubleInputDivideValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return value1 / value2;
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"{value1} / {value2}";
    }
}

public class DoubleInputMaxValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return Mathf.Max(value1, value2);
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"max({value1}, {value2})";
    }
}

public class DoubleInputMinValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return Mathf.Min(value1, value2);
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"min({value1}, {value2})";
    }
}

public class DoubleInputModValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return value1 % value2;
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"mod({value1}, {value2})";
    }
}

public class DoubleInputPowerValueOperation : DoubleInputOperations
{
    public override float GetValue(float value1, float value2)
    {
        return Mathf.Pow(value1, value2);
    }

    public override string GetFunction(string value1, string value2)
    {
        return $"pow({value1}, {value2})";
    }
}

public enum DoubleInputOperationType
{
    Add = 0,
    Subtract = 1,
    Multiply = 2,
    Divide = 3,
    Max = 4,
    Min = 5,
    Mod = 6,
    Power = 7
}