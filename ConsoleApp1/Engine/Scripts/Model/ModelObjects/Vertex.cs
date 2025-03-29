using OpenTK.Mathematics;

public class Vertex
{
    private Vector3 _position = Vector3.Zero;
    public Vector3 Position;
    public Vector3 Color = (0, 0, 0);
    public HashSet<Triangle> ParentTriangles = new HashSet<Triangle>();
    public HashSet<Edge> ParentEdges = new HashSet<Edge>();
    public int Index = 0;
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

    public bool ShareEdgeWith(Vertex vertex, out Edge edge)
    {
        Edge? e = GetEdgeWith(vertex);
        if (e != null)
        {
            edge = e;
            return true;
        }
        edge = Edge.Empty;
        return false;
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

    public bool ShareTriangleWith(Vertex vertex)
    {
        return GetTriangleWith(vertex) != null;
    }

    public bool ShareTriangleWith(Vertex vertex, out Triangle triangle)
    {
        Triangle? t = GetTriangleWith(vertex);
        if (t != null)
        {
            triangle = t;
            return true;
        }
        triangle = Triangle.Empty;
        return false;
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
        // Step1: Remove all edges and its triangles that share this vertex with the vertex to be replaced
        foreach (var edge in ParentEdges)
        {
            if (edge.A == vertex || edge.B == vertex)
            {
                edgesToRemove.Add(edge);
                foreach (var triangle in edge.ParentTriangles)
                {
                    trianglesToRemove.Add(triangle);
                }
            }
        }

        // Step2: Set this vertex to the new vertex in all edges
        foreach (var edge in ParentEdges)
        {
            edge.SetVertexTo(this, vertex);
        }

        // Step3: Set this vertex to the new vertex in all triangles
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