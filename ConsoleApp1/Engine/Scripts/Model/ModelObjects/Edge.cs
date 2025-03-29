using OpenTK.Mathematics;

public class Edge
{
    public static Edge Empty = new Edge(new Vertex(Vector3.Zero), new Vertex(Vector3.Zero));

    public Vertex A;
    public Vertex B;

    public HashSet<Triangle> ParentTriangles = new HashSet<Triangle>();

    public Edge(Vertex v1, Vertex v2)
    {
        A = v1;
        B = v2;

        A.AddParentEdge(this);
        B.AddParentEdge(this);
    }

    public void AddParentTriangle(Triangle triangle)
    {
        if (!ParentTriangles.Contains(triangle))
            ParentTriangles.Add(triangle);
    }

    public void SetVertexTo(Vertex oldVertex, Vertex newVertex)
    {
        if (A == oldVertex)
            A = newVertex;
        else if (B == oldVertex)
            B = newVertex;

        oldVertex.ParentEdges.Remove(this);
        newVertex.ParentEdges.Add(this);
    }

    public bool HasNotVertex(Vertex v)
    {
        return A != v && B != v;
    }

    public Vector3 GetDirectionFrom(Vertex vertex)
    {
        return vertex == A ? B.Position - A.Position : A.Position - B.Position;
    }

    public void ReplaceWith(Edge edge)
    {
        foreach (Triangle triangle in ParentTriangles)
        {
            triangle.SetEdgeTo(this, edge);
        }
    }

    public Vertex Not(Vertex v)
    {
        return A == v ? B : A;
    }

    public Edge Delete()
    {
        A.ParentEdges.Remove(this);
        B.ParentEdges.Remove(this);
        return this;
    }
}