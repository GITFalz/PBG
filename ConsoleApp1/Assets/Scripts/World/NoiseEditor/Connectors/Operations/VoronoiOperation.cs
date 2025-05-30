using OpenTK.Mathematics;

public abstract class VoronoiOperation
{
    public abstract float GetValue(Vector2 position, out Vector2 cell);
    public abstract string GetFunction(string a, string b, string c);

    public static VoronoiOperation GetVoronoiOperation(VoronoiOperationType type)
    {
        return type switch
        {
            VoronoiOperationType.Basic => new VoronoiOperationBasic(),
            VoronoiOperationType.Edge => new VoronoiOperationEdge(),
            VoronoiOperationType.Distance => new VoronoiOperationDistance(),
            VoronoiOperationType.Normal => new VoronoiOperationNormal(),
            VoronoiOperationType.Angle => new VoronoiOperationAngle(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class VoronoiOperationBasic : VoronoiOperation
{
    public override float GetValue(Vector2 position, out Vector2 cell)
    {
        return VoronoiLib.Voronoi(position, out cell);
    }

    public override string GetFunction(string a, string b, string c)
    {
        return $"SampleVoronoi({a}, {b}, {c})";
    }
}

public class VoronoiOperationEdge : VoronoiOperation
{
    public override float GetValue(Vector2 position, out Vector2 cell)
    {
        return VoronoiLib.VoronoiF2(position, out cell);
    }

    public override string GetFunction(string a, string b, string c)
    {
        return $"SampleVoronoiF2({a}, {b}, {c})";
    }
}

public class VoronoiOperationDistance : VoronoiOperation
{
    public override float GetValue(Vector2 position, out Vector2 cell)
    {
        return VoronoiLib.VoronoiDistance(position, out cell);
    }

    public override string GetFunction(string a, string b, string c)
    {
        return $"SampleVoronoiDistance({a}, {b}, {c})";
    }
}

public class VoronoiOperationNormal : VoronoiOperation
{
    public override float GetValue(Vector2 position, out Vector2 cell)
    {
        return VoronoiLib.VoronoiWF(position, out cell);
    }

    public override string GetFunction(string a, string b, string c)
    {
        return $"SampleVoronoiNormal({a}, {b}, {c})";
    }
}

public class VoronoiOperationAngle : VoronoiOperation
{
    public override float GetValue(Vector2 position, out Vector2 cell)
    {
        return VoronoiLib.VoronoiAngle(position, out cell);
    }

    public override string GetFunction(string a, string b, string c)
    {
        return $"SampleVoronoiAngle({a}, {b}, {c})";
    }
}

public enum VoronoiOperationType
{
    Basic,
    Edge,
    Distance,
    Normal,
    Angle,
}