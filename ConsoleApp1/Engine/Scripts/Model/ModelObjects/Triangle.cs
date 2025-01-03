using OpenTK.Mathematics;

public class Triangle
{
    public Vertex A;
    public Vertex B;
    public Vertex C;
    
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
    }
    
    public void Invert()
    {
        (A, B) = (B, A);
    }
    
    public bool TwoVertSamePosition()
    {
        return A.Position == B.Position || A.Position == C.Position || B.Position == C.Position;
    }
}