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

public static class Int
{
    public static int Parse(string value, int replacement = 1)
    {
        try 
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return replacement;
        }
    }
}