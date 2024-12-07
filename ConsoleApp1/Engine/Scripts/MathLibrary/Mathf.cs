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
    
    public static System.Numerics.Vector3 ToNumericsVector3(OpenTK.Mathematics.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }
    
    public static System.Numerics.Vector2 ToNumericsVector2(OpenTK.Mathematics.Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }
}