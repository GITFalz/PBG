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

    public static bool TryParse(string value, out float result)
    {
        try 
        {
            result = Parse(value);
            return true;
        }
        catch (Exception)
        {
            result = 0f;
            return false;
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