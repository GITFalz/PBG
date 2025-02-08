using OpenTK.Mathematics;

public class Triangle
{
    public Vertex A;
    public Vertex B;
    public Vertex C;

    public Edge AB;
    public Edge BC;
    public Edge CA;
    
    public Vector3 Normal;
    
    public Quad? ParentQuad;

    public Triangle(Vertex a, Vertex b, Vertex c, Edge ab, Edge bc, Edge ca, Quad? parentQuad = null)
    {
        A = a;
        B = b;
        C = c;

        AB = ab;
        BC = bc;
        CA = ca;

        A.AddParentTriangle(this);
        B.AddParentTriangle(this);
        C.AddParentTriangle(this);

        AB.AddParentTriangle(this);
        BC.AddParentTriangle(this);
        CA.AddParentTriangle(this);

        A.Index = 0;
        B.Index = 1;
        C.Index = 2;

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

    public void SetVertexTo(Vertex oldVertex, Vertex newVertex)
    {
        if (A == oldVertex)
            A = newVertex;
        else if (B == oldVertex)
            B = newVertex;
        else if (C == oldVertex)
            C = newVertex;

        AB.SetVertexTo(oldVertex, newVertex);
        BC.SetVertexTo(oldVertex, newVertex);
        CA.SetVertexTo(oldVertex, newVertex);
    }

    public void SetEdgeTo(Edge oldEdge, Edge newEdge)
    {
        if (AB == oldEdge)
            AB = newEdge;
        else if (BC == oldEdge)
            BC = newEdge;
        else if (CA == oldEdge)
            CA = newEdge;
    }

    public bool GetEdgeWithout(Vertex vertex, out Edge? edge)
    {
        edge = null;
        if (AB.HasNotVertex(vertex))
        {
            edge = AB;
            return true;
        }
        if (BC.HasNotVertex(vertex))
        {
            edge = BC;
            return true;
        }
        if (CA.HasNotVertex(vertex))
        {
            edge = CA;
            return true;
        }
        return false;
    }


    public bool HasVertices(Vertex a, Vertex b, Vertex c)
    {
        return (A == a || A == b || A == c) && (B == a || B == b || B == c) && (C == a || C == b || C == c);
    }

    public bool HasVertices(Vertex a, Vertex b)
    {
        return (A == a && B == b) || (A == b && B == a) || (A == a && C == b) || (A == b && C == a) || (B == a && C == b) || (B == b && C == a);
    }

    public List<Vector3> GetVerticesPosition()
    {
        return new List<Vector3> {A.Position, B.Position, C.Position};
    }

    public List<Vertex> GetVertices()
    {
        return new List<Vertex> {A, B, C};
    }
}