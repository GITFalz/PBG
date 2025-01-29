using System.Globalization;

public static class Float
{
    public static float Parse(string value)
    {
        try 
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return 0f;
        }
    }
}