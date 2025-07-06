public abstract class OperationOperations
{
    public abstract float GetValue(float value1, float value2);
    public abstract string GetFunction(string value1, string value2);
    private static readonly OperationOperationType[] _types = 
    {
        OperationOperationType.Add,
        OperationOperationType.Subtract,
        OperationOperationType.Multiply,
        OperationOperationType.Divide,
        OperationOperationType.Max,
        OperationOperationType.Min,
        OperationOperationType.Mod,
        OperationOperationType.Power
    };

    public static OperationOperations GetOperation(OperationOperationType type)
    {
        return type switch
        {
            OperationOperationType.Add => new DoubleInputAddValueOperation(),
            OperationOperationType.Subtract => new DoubleInputSubtractValueOperation(),
            OperationOperationType.Multiply => new DoubleInputMultiplyValueOperation(),
            OperationOperationType.Divide => new DoubleInputDivideValueOperation(),
            OperationOperationType.Max => new DoubleInputMaxValueOperation(),
            OperationOperationType.Min => new DoubleInputMinValueOperation(),
            OperationOperationType.Mod => new DoubleInputModValueOperation(),
            OperationOperationType.Power => new DoubleInputPowerValueOperation(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class DoubleInputAddValueOperation : OperationOperations
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

public class DoubleInputSubtractValueOperation : OperationOperations
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

public class DoubleInputMultiplyValueOperation : OperationOperations
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

public class DoubleInputDivideValueOperation : OperationOperations
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

public class DoubleInputMaxValueOperation : OperationOperations
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

public class DoubleInputMinValueOperation : OperationOperations
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

public class DoubleInputModValueOperation : OperationOperations
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

public class DoubleInputPowerValueOperation : OperationOperations
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

public enum OperationOperationType
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