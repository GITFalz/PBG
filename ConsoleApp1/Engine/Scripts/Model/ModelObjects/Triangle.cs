using OpenTK.Mathematics;

public class Triangle
{
    public Vertex A;
    public Vertex B;
    public Vertex C;
    
    public Vector3 EnAb;
    public Vector3 EnBc;
    public Vector3 EnCa;
    
    public Vector3 Normal;
    
    public Quad? ParentQuad;

    public Triangle(Vertex a, Vertex b, Vertex c, Quad? parentQuad = null)
    {
        A = a; A.ParentTriangle = this; A.Index = 0;
        B = b; B.ParentTriangle = this; B.Index = 1;
        C = c; C.ParentTriangle = this; C.Index = 2;
        UpdateNormals();
        ParentQuad = parentQuad;
    }

    public void UpdateNormals()
    {
        Vector3 edge1 = B.Position - A.Position;
        Vector3 edge2 = C.Position - A.Position;
        Normal = Vector3.Cross(edge1, edge2);
        
        EnAb = CalcNormal(A.Position, B.Position, C.Position);
        EnBc = CalcNormal(B.Position, C.Position, A.Position);
        EnCa = CalcNormal(C.Position, A.Position, B.Position);
    }

    private Vector3 CalcNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 edge = b - a;
        Vector3 edgeNormal = Vector3.Cross(Normal, edge);
        Vector3 dir = c - a;
        if (Vector3.Dot(edgeNormal, dir) < 0)
            edgeNormal = -edgeNormal;
        return Mathf.Normalize(edgeNormal);
    }
    
    public void Invert()
    {
        (A, B) = (B, A);
    }
}