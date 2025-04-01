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

    public static float Clamp(float min, float max, float value)
    {
        return value < min ? min : value > max ? max : value;
    }
    
    public static int Clamp(int min, int max, int value)
    {
        return value < min ? min : value > max ? max : value;
    }
    
    public static float Clamp01(float value)
    {
        return value < 0 ? 0 : value > 1 ? 1 : value;
    }
    
    public static int Clamp01(int value)
    {
        return value < 0 ? 0 : value > 1 ? 1 : value;
    }

    public static float Pow(float value, float power)
    {
        return (float)Math.Pow(value, power);
    }

    public static double Pow(double value, double power)
    {
        return Math.Pow(value, power);
    }

    public static float Floor(float value)
    {
        return (float)Math.Floor(value);
    }
    
    public static Vector3 Floor(Vector3 value)
    {
        return new Vector3(Floor(value.X), Floor(value.Y), Floor(value.Z));
    }
    
    public static Vector3i FloorToInt(Vector3 value)
    {
        return new Vector3i(FloorToInt(value.X), FloorToInt(value.Y), FloorToInt(value.Z));
    }

    public static Vector3 Transform(Vector3 value, Matrix4 matrix)
    {
        return ToOpenTKVector3(System.Numerics.Vector3.Transform(ToNumericsVector3(value), ToNumericsMatrix4(matrix)));
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
    
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return a + (b - a) * t;
    }
    
    public static float Ease_I_O_Lerp(float a, float b, float t)
    {
        t = t * t * (3f - 2f * t);
        return Lerp(a, b, t);
    }
    
    public static Vector3 YAngleToDirection(float yAngleDegrees)
    {
        float yAngleRadians = yAngleDegrees * (float)Math.PI / 180.0f;
        
        float x = (float)Math.Cos(yAngleRadians);
        float z = (float)Math.Sin(yAngleRadians);

        return new Vector3(x, 0, z);
    }
    
    public static double Max(double a, double b)
    {
        return a > b ? a : b;
    }
    
    public static float Max(float a, float b)
    {
        return a > b ? a : b;
    }
    
    public static int Max(int a, int b)
    {
        return a > b ? a : b;
    }

    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new Vector2(Max(a.X, b.X), Max(a.Y, b.Y));
    }

    public static Vector3 Max(Vector3 a, Vector3 b)
    {
        return new Vector3(Max(a.X, b.X), Max(a.Y, b.Y), Max(a.Z, b.Z));
    }

    public static Vector4 Max(Vector4 a, Vector4 b)
    {
        return new Vector4(Max(a.X, b.X), Max(a.Y, b.Y), Max(a.Z, b.Z), Max(a.W, b.W));
    }
    
    public static double Min(double a, double b)
    {
        return a < b ? a : b;
    }
    
    public static float Min(float a, float b)
    {
        return a < b ? a : b;
    }
    
    public static int Min(int a, int b)
    {
        return a < b ? a : b;
    }

    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new Vector2(Min(a.X, b.X), Min(a.Y, b.Y));
    }

    public static Vector3 Min(Vector3 a, Vector3 b)
    {
        return new Vector3(Min(a.X, b.X), Min(a.Y, b.Y), Min(a.Z, b.Z));
    }

    public static Vector4 Min(Vector4 a, Vector4 b)
    {
        return new Vector4(Min(a.X, b.X), Min(a.Y, b.Y), Min(a.Z, b.Z), Min(a.W, b.W));
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
    
    public static System.Numerics.Matrix4x4 ToNumericsMatrix4(Matrix4 matrix)
    {
        return new System.Numerics.Matrix4x4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }
    
    public static System.Numerics.Vector4 ToNumericsVector4(OpenTK.Mathematics.Vector4 vector)
    {
        return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
    }
    
    public static System.Numerics.Vector3 ToNumericsVector3(OpenTK.Mathematics.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector3 ToOpenTKVector3(System.Numerics.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
    
    public static System.Numerics.Vector2 ToNumericsVector2(OpenTK.Mathematics.Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }

    public static Vector3 DegreesToRadians(Vector3 degrees)
    {
        return new Vector3(
            MathHelper.DegreesToRadians(degrees.X),
            MathHelper.DegreesToRadians(degrees.Y),
            MathHelper.DegreesToRadians(degrees.Z)
        );
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
    
    public static Quaternion RotateAround(Vector3 axis, Quaternion rotation, float angle)
    {
        return Quaternion.FromAxisAngle(axis, angle) * rotation;
    }

    public static Vector3 RotatePoint(Vector3 point, Vector3 center, Vector3 axis, float degrees)
    {
        float radians = MathHelper.DegreesToRadians(degrees);
        float sinHalfAngle = MathF.Sin(radians / 2);

        axis.Normalize();
        Vector3 relativePoint = point - center;

        Quaternion rotation = new Quaternion(axis * sinHalfAngle, MathF.Cos(radians / 2));
        Quaternion pQuat = new Quaternion(relativePoint, 0);
        Quaternion rotatedQuat = rotation * pQuat * rotation.Inverted();

        return (rotatedQuat.X, rotatedQuat.Y, rotatedQuat.Z) + center;
    }
    
    public static Vector2? WorldToScreen(Vector3 worldPosition, System.Numerics.Matrix4x4 projectionMatrix, System.Numerics.Matrix4x4 vMatrix)
    {
        System.Numerics.Matrix4x4 viewMatrix = vMatrix;
        System.Numerics.Matrix4x4 projMatrix = projectionMatrix;

        System.Numerics.Vector4 viewSpace = System.Numerics.Vector4.Transform(
            new System.Numerics.Vector4(ToNumericsVector3(worldPosition), 1.0f),
            viewMatrix
        );

        System.Numerics.Vector4 clipSpace = System.Numerics.Vector4.Transform(viewSpace, projMatrix);

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
    
    
    public static bool IsPointNearLine(Link<Vector2> link, Vector2 point, float distance)
    {
        return IsPointNearLine(link.A, link.B, point, distance);
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

    public static void GetSmallestBoundingBox(List<Vertex> vertices, out Vector3 min, out Vector3 max)
    {
        if (vertices.Count == 0)
        {
            min = Vector3.Zero;
            max = Vector3.Zero;
            return;
        }

        List<Vector3> positions = [];
        foreach (var vertex in vertices)
        {
            positions.Add(vertex.Position);
        }

        Vector3 center = Vector3.Zero;
        Vector3 rotationAxis = (0, 1, 0);
        foreach (var vertex in vertices)
        {
            center += vertex.Position;
        }
        center /= vertices.Count;

        Vector3 axisX = (1, 0, 0);

        List<Edge> edges = Edge.GetEdges(vertices);
        List<Vector3> copy = [.. positions];

        if (edges.Count == 0)
        {
            min = Vector3.Zero;
            max = Vector3.Zero;
            return;
        }

        Edge best = edges[0];
        Vector3 direction = edges[0].GetDirection();
        float angle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(axisX, direction));

        for (int i = 0; i < copy.Count; i++)
        {
            copy[i] = RotateAround(copy[i], center, rotationAxis, angle);
        }

        Vector3 minC = copy[0];
        Vector3 maxC = copy[0];

        for (int i = 1; i < copy.Count; i++)
        {
            minC = Vector3.ComponentMin(minC, copy[i]);
            maxC = Vector3.ComponentMax(maxC, copy[i]);
        }

        Vector3 size = maxC - minC;
        min = minC;
        max = maxC;

        for (int i = 1; i < edges.Count; i++)
        {
            copy = [.. positions];

            direction = edges[i].GetDirection();
            float a = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(axisX, direction));

            for (int j = 0; j < copy.Count; j++)
            {
                copy[j] = RotateAround(copy[j], center, rotationAxis, a);
            }

            minC = copy[0];
            maxC = copy[0];

            for (int j = 1; j < copy.Count; j++)
            {
                minC = Vector3.ComponentMin(minC, copy[j]);
                maxC = Vector3.ComponentMax(maxC, copy[j]);
            }

            Vector3 sizeC = maxC - minC;

            //Console.WriteLine("Size original: " + size + " Volume: " + size.X * size.Y * size.Z);
            //Console.WriteLine("Size rotated: " + sizeC + " Volume: " + sizeC.X * sizeC.Y * sizeC.Z);

            if (sizeC.X * sizeC.Z < size.X * size.Z)
            {
                min = minC;
                max = maxC;
                size = sizeC;
                angle = a;
                best = edges[i];
            }
        }

        best.A.Color = (0.25f, 0.3f, 1);
        best.B.Color = (0.25f, 0.3f, 1);

        foreach (var vertex in vertices)
        {
            vertex.SetPosition(RotateAround(vertex, center, rotationAxis, angle));
        }
    }
}