using OpenTK.Mathematics;
using Vec4 = System.Numerics.Vector4;
using Vec3 = System.Numerics.Vector3;
using Vec2 = System.Numerics.Vector2;

public static class Mathf
{
    public static int FloorToInt(float value) => (int)Math.Floor(value);
    public static int FloorToInt(double value) => (int)Math.Floor(value);
    public static Vector3i FloorToInt(Vector3 value) => new Vector3i(FloorToInt(value.X), FloorToInt(value.Y), FloorToInt(value.Z));
    public static Vector2i FloorToInt(Vector2 value) => new Vector2i(FloorToInt(value.X), FloorToInt(value.Y));
    public static float Floor(float value) => (float)Math.Floor(value);

    public static int RoundToInt(float value) => (int)Math.Round(value);

    public static int CeilToInt(float value) => (int)Math.Ceiling(value);

    public static float Clamp(float min, float max, float value) => value < min ? min : value > max ? max : value; 
    public static int Clamp(int min, int max, int value) => value < min ? min : value > max ? max : value;
    public static float Clamp01(float value) => value < 0 ? 0 : value > 1 ? 1 : value;
    public static int Clamp01(int value) => value < 0 ? 0 : value > 1 ? 1 : value;

    public static float Pow(float value, float power) => (float)Math.Pow(value, power);
    public static double Pow(double value, double power) => Math.Pow(value, power);
    
    public static Vector3 Normalize(Vector3 value)
    {
        float length = value.Length;
        if (length == 0)
            return value;
        return value / length;
    }

    public static float Lerp(float a, float b, float t) => a + t * (b - a); 
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;
    public static float Ease_I_O_Lerp(float a, float b, float t) => Lerp(a, b, t * t * (3f - 2f * t));

    public static Vector3 YAngleToDirection(float yAngleDegrees)
    {
        float yAngleRadians = yAngleDegrees * (float)Math.PI / 180.0f;
        
        float x = (float)Math.Cos(yAngleRadians);
        float z = (float)Math.Sin(yAngleRadians);

        return new Vector3(x, 0, z).Normalized();
    }

    public static double Max(params double[] values)
    {
        double max = values[0];
        for (int i = 1; i < values.Length; i++)
            max = Max(max, values[i]);
        return max;
    } 
    public static double Max(double a, double b) => a > b ? a : b;


    public static float Max(params float[] values)
    {
        float max = values[0];
        for (int i = 1; i < values.Length; i++)
            max = Max(max, values[i]);
        return max;
    }
    public static float Max(float a, float b) => a > b ? a : b;

    
    public static int Max(params int[] values)
    {
        int max = values[0];
        for (int i = 1; i < values.Length; i++)
            max = Max(max, values[i]);
        return max;
    }
    public static int Max(int a, int b) => a > b ? a : b;
    

    public static double Min(params double[] values)
    {
        double min = values[0];
        for (int i = 1; i < values.Length; i++)
            min = Min(min, values[i]);
        return min;
    }
    public static double Min(double a, double b) => a < b ? a : b;
    

    public static float Min(params float[] values)
    {
        float min = values[0];
        for (int i = 1; i < values.Length; i++)
            min = Min(min, values[i]);
        return min;
    }
    public static float Min(float a, float b) => a < b ? a : b;
    

    public static int Min(params int[] values)
    {
        int min = values[0];
        for (int i = 1; i < values.Length; i++)
            min = Min(min, values[i]);
        return min;
    }
    public static int Min(int a, int b) => a < b ? a : b;
    
    public static int Sign(float value) => value > 0 ? 1 : value < 0 ? -1 : 0;
    public static int SignNo0(float value) => value < 0 ? -1 : 1;
    
    public static float Abs(float value) => value < 0 ? -value : value;
    public static Vector3 Abs(Vector3 value) => new Vector3(Abs(value.X), Abs(value.Y), Abs(value.Z));
    public static Vector2 Abs(Vector2 value) => new Vector2(Abs(value.X), Abs(value.Y));

    public static float Fraction(float value) => value - Floor(value);
    public static Vector2 Fraction(Vector2 value) => new Vector2(Fraction(value.X), Fraction(value.Y));
    public static Vector3 Fraction(Vector3 value) => new Vector3(Fraction(value.X), Fraction(value.Y), Fraction(value.Z));

    public static float Sin(float value) => (float)Math.Sin(value);
    public static Vector2 Sin(Vector2 value) => new Vector2(Sin(value.X), Sin(value.Y));
    public static Vector3 Sin(Vector3 value) => new Vector3(Sin(value.X), Sin(value.Y), Sin(value.Z));

    public static float Cos(float value) => (float)Math.Cos(value);
    public static Vector2 Cos(Vector2 value) => new Vector2(Cos(value.X), Cos(value.Y));
    public static Vector3 Cos(Vector3 value) => new Vector3(Cos(value.X), Cos(value.Y), Cos(value.Z));

    public static float Mod(float value, float mod) => value - (mod * Floor(value / mod));
    public static Vector2 Mod(Vector2 value, float mod) => new Vector2(Mod(value.X, mod), Mod(value.Y, mod));
    public static Vector3 Mod(Vector3 value, float mod) => new Vector3(Mod(value.X, mod), Mod(value.Y, mod), Mod(value.Z, mod));
    
    /// <summary>
    /// turns the range [a, b] into [0, 1] based on t
    /// </summary>
    public static float LerpI(float a, float b, float t)
    {
        if (a == b)
            return 1;

        return (t - a) / (b - a);
    }
    
    public static System.Numerics.Matrix4x4 ToNumerics(Matrix4 matrix) => new System.Numerics.Matrix4x4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    
    public static Vec4 Num(Vector4 vector) => new Vec4(vector.X, vector.Y, vector.Z, vector.W);
    public static Vec3 Num(Vector3 vector) => new Vec3(vector.X, vector.Y, vector.Z);
    public static Vec2 Num(Vector2 vector) => new Vec2(vector.X, vector.Y);

    // Vector4 Adding
    public static Vec4 Add(Vec4 vector, Vector4 add) => new Vec4(vector.X + add.X, vector.Y + add.Y, vector.Z + add.Z, vector.W + add.W);
    public static Vector4 Add(Vector4 vector, Vec4 add) => new Vector4(vector.X + add.X, vector.Y + add.Y, vector.Z + add.Z, vector.W + add.W);

    // Vector3 Adding
    public static Vec3 Add(Vec3 vector, Vector3 add) => new Vec3(vector.X + add.X, vector.Y + add.Y, vector.Z + add.Z);
    public static Vector3 Add(Vector3 vector, Vec3 add) => new Vector3(vector.X + add.X, vector.Y + add.Y, vector.Z + add.Z);

    // Vector2 Adding
    public static Vec2 Add(Vec2 vector, Vector2 add) => new Vec2(vector.X + add.X, vector.Y + add.Y);
    public static Vector2 Add(Vector2 vector, Vec2 add) => new Vector2(vector.X + add.X, vector.Y + add.Y);


    public static Vector3 Xyz(Vector4 vector) => (vector.X, vector.Y, vector.Z);
    public static Vec3 Xyz(Vec4 vector) => new Vec3(vector.X, vector.Y, vector.Z);

    public static Vector3 DegreesToRadians(Vector3 degrees) => new Vector3(
            MathHelper.DegreesToRadians(degrees.X),
            MathHelper.DegreesToRadians(degrees.Y),
            MathHelper.DegreesToRadians(degrees.Z)
        );
    
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
    
    public static Quaternion RotateAround(Vector3 axis, Quaternion rotation, float angle) => Quaternion.FromAxisAngle(axis, angle) * rotation;
    
    public static Vector2? WorldToScreen(Vector3 worldPosition, System.Numerics.Matrix4x4 projectionMatrix, System.Numerics.Matrix4x4 vMatrix)
    {
        System.Numerics.Matrix4x4 viewMatrix = vMatrix;
        System.Numerics.Matrix4x4 projMatrix = projectionMatrix;

        Vec4 viewSpace = Vec4.Transform(
            new Vec4(Num(worldPosition), 1.0f),
            viewMatrix
        );

        Vec4 clipSpace = Vec4.Transform(viewSpace, projMatrix);

        if (clipSpace.W <= 0)
            return null;

        float ndcX = clipSpace.X / clipSpace.W;
        float ndcY = clipSpace.Y / clipSpace.W;

        Vector2 screenPos = new Vector2(
            (ndcX + 1.0f) * Game.Width * 0.5f,
            (1.0f - ndcY) * Game.Height * 0.5f
        );

        return screenPos;
    }
    
    public static List<Vector2> GetVertexScreenPositions(List<Vector3> vertices, Camera camera)
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        HashSet<Vector3> uniqueVertices = new HashSet<Vector3>();
        List<Vector2> screenPositions = new List<Vector2>();
        
        foreach (var vert in vertices)
        {
            if (!uniqueVertices.Add(vert))
                continue;
            
            Vector2? screenPos = WorldToScreen(vert, projection, view);
            if (screenPos == null)
                continue;
            
            screenPositions.Add(screenPos.Value);
        }
        
        return screenPositions;
    }
    
    public static float GetAngleBetweenPoints(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float angleRadians = MathF.Atan2(direction.X, direction.Y);
        float angleDegrees = angleRadians * (180f / MathF.PI);
        if (angleDegrees < 0) 
            angleDegrees += 360f;
        return angleDegrees;
    }
    
    
    public static bool IsPointNearLine(Vector2 pointA, Vector2 pointB, Vector2 point, float distance)
    {
        Vector2 lineDirection = pointB - pointA;
        Vector2 pointToStart = point - pointA;
        float lineLengthSq = lineDirection.LengthSquared;
        float dot = Vector2.Dot(pointToStart, lineDirection);
        float t = MathHelper.Clamp(dot / lineLengthSq, 0f, 1f);
        Vector2 closestPoint = pointA + (lineDirection * t);
        return (point - closestPoint).LengthSquared <= distance * distance;
    }


    // Noise manipulation functions
    public static float PLerp(float min, float max, float value)
    {
        float midpoint = (min + max) / 2;
        if (value < min || value > max)
            return 0;

        float distanceFromMidpoint = Mathf.Abs(value - midpoint);
        float totalDistance = (max - min) / 2;
        
        float proximityValue = 1 - (distanceFromMidpoint / totalDistance);

        return proximityValue;
    }

    public static float SLerp(float min, float max, float value)
    {
        if (value <= min) return 0f;
        if (value >= max) return 1f;
        return (value - min) / (max - min);
    }

    public static float NoiseLerp(float noiseA, float noiseB, float minB, float maxA, float t)
    {
        if (maxA - minB == 0)
            return noiseA;
        
        float nt = (Clamp(t, minB, maxA) - minB) / (maxA - minB);
        return Lerp(noiseA, noiseB, nt);
    }


    public static string ConvertGLSL(Vector2 vector) => $"vec2({vector.X}, {vector.Y})";
}