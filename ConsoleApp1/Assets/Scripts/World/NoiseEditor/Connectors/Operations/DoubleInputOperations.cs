public abstract class DoubleInputOperations
{
    public abstract float GetValue(float value1, float value2);
    public abstract string GetFunction(string value1, string value2);

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

public enum DoubleInputOperationType
{
    Add = 0,
    Subtract = 1,
    Multiply = 2,
    Divide = 3,
    Max = 4,
    Min = 5
}