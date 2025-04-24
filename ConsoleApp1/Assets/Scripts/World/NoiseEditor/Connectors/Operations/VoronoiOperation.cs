using OpenTK.Mathematics;

public abstract class VoronoiOperation
{
    public abstract float GetValue(Vector2 position);
    public abstract string GetFunction(string a, string b);

    public static VoronoiOperation GetVoronoiOperation(VoronoiOperationType type)
    {
        return type switch
        {
            VoronoiOperationType.Basic => new VoronoiOperationBasic(),
            VoronoiOperationType.Edge => new VoronoiOperationEdge(),
            VoronoiOperationType.Distance => new VoronoiOperationDistance(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class VoronoiOperationBasic : VoronoiOperation
{
    public override float GetValue(Vector2 position)
    {
        return VoronoiLib.Voronoi(position);
    }

    public override string GetFunction(string a, string b)
    {
        return $"SampleVoronoi({a}, {b})";
    }
}

public class VoronoiOperationEdge : VoronoiOperation
{
    public override float GetValue(Vector2 position)
    {
        return VoronoiLib.VoronoiF2(position);
    }

    public override string GetFunction(string a, string b)
    {
        return $"SampleVoronoiF2({a}, {b})";
    }
}

public class VoronoiOperationDistance : VoronoiOperation
{
    public override float GetValue(Vector2 position)
    {
        return VoronoiLib.VoronoiDistance(position);
    }

    public override string GetFunction(string a, string b)
    {
        return $"SampleVoronoiDistance({a}, {b})";
    }
}

public enum VoronoiOperationType
{
    Basic,
    Edge,
    Distance
}