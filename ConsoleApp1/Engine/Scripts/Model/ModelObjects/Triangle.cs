using OpenTK.Mathematics;

public class Triangle
{
    public static Triangle Empty = new Triangle(new Vertex(Vector3.Zero), new Vertex(Vector3.Zero), new Vertex(Vector3.Zero));

    public Vertex A;
    public Vertex B;
    public Vertex C;

    public Edge AB;
    public Edge BC;
    public Edge CA;
    
    public Vector3 Normal;

    public bool Inverted = false;

    public Triangle(Vertex a, Vertex b, Vertex c, Edge ab, Edge bc, Edge ca)
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
    }

    public Triangle(Vertex a, Vertex b, Vertex c) : this(a, b, c, new Edge(a, b), new Edge(b, c), new Edge(c, a)) {}

    public void UpdateNormals()
    {
        Vector3 edge1 = B.Position - A.Position;
        Vector3 edge2 = C.Position - A.Position;
        Normal = Vector3.Cross(edge1, edge2);
    }
    
    public void Invert()
    {
        (A, B) = (B, A);
        Inverted = !Inverted;
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

        oldVertex.ParentTriangles.Remove(this);
        newVertex.ParentTriangles.Add(this);
    }

    public void SetEdgeTo(Edge oldEdge, Edge newEdge)
    {
        if (AB == oldEdge)
            AB = newEdge;
        else if (BC == oldEdge)
            BC = newEdge;
        else if (CA == oldEdge)
            CA = newEdge;

        oldEdge.ParentTriangles.Remove(this);
        newEdge.ParentTriangles.Add(this);
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

    public bool HasEdge(Edge edge)
    {
        return AB == edge || BC == edge || CA == edge;
    }

    public Triangle Delete()
    {
        A.ParentTriangles.Remove(this);
        B.ParentTriangles.Remove(this);
        C.ParentTriangles.Remove(this);
        
        AB.ParentTriangles.Remove(this);
        BC.ParentTriangles.Remove(this);
        CA.ParentTriangles.Remove(this);
        
        return this;
    }

    public List<Vector3> GetVerticesPosition()
    {
        return new List<Vector3> {A.Position, B.Position, C.Position};
    }

    public List<Vertex> GetVertices()
    {
        return [A, B, C];
    }

    public void InvertIfInverted()
    {
        foreach (Edge edge in new[] { AB, BC, CA })
        {
            foreach (Triangle adjTri in edge.ParentTriangles)
            {
                if (adjTri == this) continue;

                Vertex thisSingleVertex = edge.HasNotVertex(A) ? A : edge.HasNotVertex(B) ? B : C;
                Vertex adjSingleVertex = edge.HasNotVertex(adjTri.A) ? adjTri.A : edge.HasNotVertex(adjTri.B) ? adjTri.B : adjTri.C;

                Vector3 oldPosition = thisSingleVertex.Position;
                thisSingleVertex.SetPosition(adjSingleVertex.Position);
                UpdateNormals();
                float dot = Vector3.Dot(Normal, adjTri.Normal);
                thisSingleVertex.SetPosition(oldPosition);
                UpdateNormals();

                if (dot > 0)
                {
                    Normal = -Normal;
                    Invert();
                    return;
                }
            }
        }
    }
}