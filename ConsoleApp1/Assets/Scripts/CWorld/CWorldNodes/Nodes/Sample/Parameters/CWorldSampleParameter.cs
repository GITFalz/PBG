public class CWorldSampleParameter
{
    public float Min = 0;
    public float Max = 1;

    public MinMaxInputOperations Operation;

    public CWorldSampleParameter(float min, float max, MinMaxInputOperationType type)
    {
        Min = min;
        Max = max;
        Operation = MinMaxInputOperations.GetOperation(type);
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