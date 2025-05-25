using System.Globalization;

public static class Float
{
    public static float Parse(string value, float replacement = 0f)
    {
        try 
        {
            return float.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return replacement;
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

    public static string Str(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public static class Int
{
    public static int Parse(string value, int replacement = 1)
    {
        try 
        {
            return int.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return replacement;
        }
    }
}