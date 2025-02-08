public class Edge
{
    public Vertex A;
    public Vertex B;

    public List<Triangle> ParentTriangles = new List<Triangle>();

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
    }

    public bool HasNotVertex(Vertex v)
    {
        return A != v && B != v;
    }

    public Vertex Not(Vertex v)
    {
        return A == v ? B : A;
    }

    public void Swap()
    {
        (A, B) = (B, A);
    }
}