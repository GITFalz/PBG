using OpenTK.Mathematics;

public static class Mathf
{
    public static int FloorToInt(float value)
    {
        return (int)Math.Floor(value);
    }
    
    public static int FloorToInt(double value)
    {
        return (int)Math.Floor(value);
    }
    
    public static int RoundToInt(float value)
    {
        return (int)Math.Round(value);
    }

    public static float Floor(float value)
    {
        return (float)Math.Floor(value);
    }
    
    public static Vector3i FloorToInt(Vector3 value)
    {
        return new Vector3i(FloorToInt(value.X), FloorToInt(value.Y), FloorToInt(value.Z));
    }
    
    public static Vector3 Normalize(Vector3 value)
    {
        float length = value.Length;
        if (length == 0)
            return value;
        return value / length;
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
    
    public static float Max(float a, float b)
    {
        return a > b ? a : b;
    }
    
    public static int Max(int a, int b)
    {
        return a > b ? a : b;
    }
    
    public static float Min(float a, float b)
    {
        return a < b ? a : b;
    }
    
    public static int Min(int a, int b)
    {
        return a < b ? a : b;
    }
    
    public static int Sign(float value)
    {
        return value > 0 ? 1 : value < 0 ? -1 : 0;
    }
    
    public static float Abs(float value)
    {
        return value < 0 ? -value : value;
    }
    
    public static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Abs(value.X), Abs(value.Y), Abs(value.Z));
    }
    
    /// <summary>
    /// turns the range [a, b] into [0, 1] based on t
    /// </summary>
    public static float LerpI(float a, float b, float t)
    {
        if (a == b)
            return 1;

        return (t - a) / (b - a);
    }
    
    public static System.Numerics.Vector4 ToNumericsVector4(OpenTK.Mathematics.Vector4 vector)
    {
        return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
    }
    
    public static System.Numerics.Vector3 ToNumericsVector3(OpenTK.Mathematics.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }
    
    public static System.Numerics.Vector2 ToNumericsVector2(OpenTK.Mathematics.Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }
    
    public static Vector3 RotateAround(Vector3 point, Vector3 center, Vector3 axis, float angleDegrees)
    {
        Vector3 translatedPoint = point - center;
        float angleRadians = MathHelper.DegreesToRadians(angleDegrees);
        Quaternion rotation = Quaternion.FromAxisAngle(axis, angleRadians);
        Vector3 rotatedPoint = Vector3.Transform(translatedPoint, rotation);
        return rotatedPoint + center;
    }
    
    public static Vector3 RotateAround(Vector3 point, Vector3 center, Quaternion rotation)
    {
        Vector3 translatedPoint = point - center;
        Vector3 rotatedPoint = Vector3.Transform(translatedPoint, rotation);
        return rotatedPoint + center;
    }
}