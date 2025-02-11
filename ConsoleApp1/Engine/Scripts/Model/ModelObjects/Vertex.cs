using OpenTK.Mathematics;

public class Vertex
{
    private Vector3 _position = Vector3.Zero;
    public Vector3 Position;
    public List<Triangle> ParentTriangles = new List<Triangle>();
    public List<Edge> ParentEdges = new List<Edge>();
    public int Index = -1;
    public int BoneIndex = 0;

    public Vertex(Vector3 position, Triangle? parentTriangle = null)
    {
        Position = position;
        _position = position;
        if (parentTriangle != null && !ParentTriangles.Contains(parentTriangle))
            ParentTriangles.Add(parentTriangle);
    }

    public void AddParentTriangle(Triangle triangle)
    {
        if (!ParentTriangles.Contains(triangle))
            ParentTriangles.Add(triangle);
    }

    public void AddParentEdge(Edge edge)
    {
        if (!ParentEdges.Contains(edge))
            ParentEdges.Add(edge);
    }

    public void SetPosition(Vector3 pos)
    {
        _position = pos;
        Position = pos;
    }

    public void MovePosition(Vector3 offset)
    {
        Position += offset;
        _position = Position;
    }

    public void SnapPosition(Vector3 offset, float snap)
    {
        _position += offset;
        Position = new Vector3(
            (float)Math.Round(_position.X / snap) * snap,
            (float)Math.Round(_position.Y / snap) * snap,
            (float)Math.Round(_position.Z / snap) * snap
        );
    }

    public bool ShareEdgeWith(Vertex vertex)
    {
        return GetEdgeWith(vertex) != null;
    }

    public Edge? GetEdgeWith(Vertex vertex)
    {
        foreach (var edge in ParentEdges)
        {
            if (edge.A == vertex || edge.B == vertex)
                return edge;
        }
        return null;
    }

    public Triangle? GetTriangleWith(Vertex vertex)
    {
        foreach (var triangle in ParentTriangles)
        {
            if (triangle.HasVertices(this, vertex))
                return triangle;
        }
        return null;
    }

    public bool ShareTriangle(Vertex A, Vertex B)
    {
        foreach (var triangle in ParentTriangles)
        {
            if (triangle.HasVertices(this, A, B))
                return true;
        }
        return false;
    }

    public void ReplaceWith(Vertex vertex, HashSet<Edge> edgesToRemove, HashSet<Triangle> trianglesToRemove)
    {
        foreach (var edge in ParentEdges)
        {
            if (edge.A == vertex || edge.B == vertex)
            {
                edgesToRemove.Add(edge);
                foreach (var triangle in edge.ParentTriangles)
                {
                    trianglesToRemove.Add(triangle);
                }
                continue;
            }

            foreach (var vEdge in vertex.ParentEdges)
            {
                // find an edge of this that has a shared vertex with another edge of vertex
                if (edge.Not(this) == vEdge.Not(vertex))
                {
                    foreach (var triangle in edge.ParentTriangles)
                    {
                        triangle.SetEdgeTo(edge, vEdge);
                    }

                    edgesToRemove.Add(edge);
                }
            }
        }

        foreach (var triangle in ParentTriangles)
        {
            triangle.SetVertexTo(this, vertex);
        }
    }

    public Vertex Copy()
    {
        return new Vertex(Position);
    }
    
    public override string ToString()
    {
        return "( " + Position.X + ", " + Position.Y + ", " + Position.Z + " )";
    }
}