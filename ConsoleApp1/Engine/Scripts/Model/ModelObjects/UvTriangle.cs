using OpenTK.Mathematics;

public class UvTriangle
{
    public Uv A;
    public Uv B;
    public Uv C;

    public UvEdge AB;
    public UvEdge BC;
    public UvEdge CA;

    public bool Hidden = false;

    public UvTriangle(Uv uv1, Uv uv2, Uv uv3, UvEdge ab, UvEdge bc, UvEdge ca)
    {
        A = uv1;
        B = uv2;
        C = uv3;

        AB = ab;
        BC = bc;
        CA = ca;

        A.AddParentTriangle(this);
        B.AddParentTriangle(this);
        C.AddParentTriangle(this);

        AB.AddParentTriangle(this);
        BC.AddParentTriangle(this);
        CA.AddParentTriangle(this);
    }

    public Uv[] GetUvs()
    {
        return [A, B, C];
    }

    public Vector2[] GetUvPositions()
    {
        return [A, B, C];
    }
}