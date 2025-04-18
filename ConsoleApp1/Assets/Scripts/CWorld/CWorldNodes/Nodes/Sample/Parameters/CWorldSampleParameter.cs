public class CWorldSampleParameter
{
    public float Min = 0;
    public float Max = 1;

    public SingleInputOperations Operation;

    public CWorldSampleParameter(float min, float max, SingleInputOperationType type)
    {
        Min = min;
        Max = max;
        Operation = SingleInputOperations.GetOperation(type);
    }

    public float GetValue(float value)
    {
        return Operation.GetValue(Min, Max, value);
    }

    public string GetFunction(string valueName)
    {
        return Operation.GetFunction(Min, Max, valueName);
    }
}