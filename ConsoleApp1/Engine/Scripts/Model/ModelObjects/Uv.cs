using OpenTK.Mathematics;

public class Uv
{
    public List<UvEdge> ParentEdges = [];
    public List<UvTriangle> ParentTriangles = [];

    public Vector2 Value;
    public Vector3 Color = Vector3.Zero;

    public float X
    {
        get { return Value.X; }
        set { Value.X = value; }
    }
    public float Y
    {
        get { return Value.Y; }
        set { Value.Y = value; }
    }

    public Uv(Vector2 v)
    {
        Value = v;
    }

    public Uv(float x, float y)
    {
        Value = (x, y);
    }

    public bool AddParentTriangle(UvTriangle triangle)
    {
        if (ParentTriangles.Contains(triangle))
            return false;

        ParentTriangles.Add(triangle);
        return true;
    }

    public bool AddParentEdge(UvEdge edge)
    {
        if (ParentEdges.Contains(edge))
            return false;

        ParentEdges.Add(edge);
        return true;
    }

    public static implicit operator Vector2(Uv uv) => uv.Value;
    public static implicit operator Uv(Vector2 v) => new(v);
    public static Vector2 operator -(Uv uv1, Uv uv2) => uv1 - uv2;
    public static Vector2 operator +(Uv uv1, Uv uv2) => uv1 + uv2;
    public static bool operator ==(Uv uv1, Uv uv2) => ReferenceEquals(uv1, uv2) || uv1.Value == uv2.Value;
    public static bool operator !=(Uv uv1, Uv uv2) => !ReferenceEquals(uv1, uv2) && uv1.Value != uv2.Value;
}