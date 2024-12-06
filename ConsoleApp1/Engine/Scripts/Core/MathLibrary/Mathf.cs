namespace ConsoleApp1.Engine.Scripts.Core.MathLibrary;

public static class Mathf
{
    public static int FloorToInt(float value)
    {
        return (int)Math.Floor(value);
    }

    public static float Floor(float value)
    {
        return (float)Math.Floor(value);
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
}