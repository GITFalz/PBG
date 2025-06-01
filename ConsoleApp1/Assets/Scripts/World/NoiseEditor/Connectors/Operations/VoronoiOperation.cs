using OpenTK.Mathematics;

public abstract class VoronoiOperation
{
    public const int InputNone = 0b0;
    public const int InputRegion = 0b1;

    public abstract float GetValue(Vector2 position, float value, float threshold, out Vector2 cell);
    public abstract string GetFunction(string a, string b, string value, string threshold, string cell);

    public static VoronoiOperation GetVoronoiOperation(VoronoiOperationType type, int inputFlags)
    {
        return type switch
        {
            VoronoiOperationType.Basic => new VoronoiOperationBasic(),
            VoronoiOperationType.Edge => inputFlags switch
            {
                0b0 => new VoronoiOperationEdge(),
                0b1 => new VoronoiOperationEdgeRegion(),
                _ => throw new ArgumentException("Invalid Voronoi operation input type for Edge operation"),
            },
            VoronoiOperationType.Distance => new VoronoiOperationDistance(),
            VoronoiOperationType.Normal => new VoronoiOperationNormal(),
            VoronoiOperationType.Angle => new VoronoiOperationAngle(),
            _ => throw new ArgumentException("Invalid Voronoi operation type"),
        };
    }
}

public class VoronoiOperationBasic : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.Voronoi(position, out cell);
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoi({a}, {b}, {cell})";
    }
}

public class VoronoiOperationEdge : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.VoronoiF2(position, out cell);
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoiF2({a}, {b}, {cell})";
    }
}

public class VoronoiOperationEdgeRegion : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.VoronoiF2(position, out cell); // needs to change
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoiF2Region({a}, {b}, {value}, {threshold}, {cell})";
    }
}

public class VoronoiOperationDistance : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.VoronoiDistance(position, out cell);
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoiDistance({a}, {b}, {cell})";
    }
}

public class VoronoiOperationNormal : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.VoronoiWF(position, out cell);
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoiNormal({a}, {b}, {cell})";
    }
}

public class VoronoiOperationAngle : VoronoiOperation
{
    public override float GetValue(Vector2 position, float value, float threshold, out Vector2 cell)
    {
        return VoronoiLib.VoronoiAngle(position, out cell);
    }

    public override string GetFunction(string a, string b, string value, string threshold, string cell)
    {
        return $"SampleVoronoiAngle({a}, {b}, {cell})";
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