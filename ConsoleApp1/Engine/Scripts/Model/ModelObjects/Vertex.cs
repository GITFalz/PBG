using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class Vertex
{
    public static Vertex Empty = new Vertex(Vector3.Zero);

    public string Name = "0";

    private Vector3 _position = Vector3.Zero;
    public Vector3 Position;
    public Vector4 V4 => new Vector4(Position.X, Position.Y, Position.Z, 1);

    public float X
    {
        get { return Position.X; }
        set { Position.X = value; }
    }
    public float Y
    {
        get { return Position.Y;}
        set { Position.Y = value; }
    }
    public float Z
    {
        get { return Position.Z;}
        set { Position.Z = value; }
    }

    public Vector3 Color = (0, 0, 0);
    public HashSet<Triangle> ParentTriangles = new HashSet<Triangle>();
    public HashSet<Edge> ParentEdges = new HashSet<Edge>();
    public int Index = 0;
    public string BoneName = "ChildBone1";
    public Bone? Bone = null;

    public Vertex(Vector3 position, Triangle? parentTriangle = null)
    {
        SetPosition(position);
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

    public void ReplaceWith(Vertex vertex)
    {
        List<Edge> edges = [.. ParentEdges];
        foreach (var edge in edges)
        {
            edge.SetVertexTo(this, vertex);
        }

        List<Triangle> triangles = [.. ParentTriangles];
        foreach (var triangle in triangles)
        {
            triangle.SetVertexTo(this, vertex);
        }
    }

    public bool HasEdgeWith(Vertex vertex, [NotNullWhen(true)] out Edge? edge)
    {
        edge = null;
        foreach (var e in ParentEdges)
        {
            if (e.Is(this, vertex))
            {
                edge = e;
                return true;
            }
        }
        return false;
    }

    public Vertex Copy()
    {
        Vertex vertex = new Vertex(Position);
        vertex.Name = Name;
        return vertex;
    }

    public void GetConnectedVertices(HashSet<Vertex> vertices, HashSet<Triangle> triangles)
    {
        if (!vertices.Add(this))
            return;

        foreach (var triangle in ParentTriangles)
        {
            if (!triangles.Add(triangle))
                continue;

            foreach (var vertex in triangle.GetVertices())
            {
                if (vertex != this)
                {
                    vertex.GetConnectedVertices(vertices, triangles);
                }
            }
        }
    }


    // Operators
    public static implicit operator Vector3(Vertex vertex) => vertex.Position;
    public static bool operator &(Vertex vertex1, Vertex vertex2) => vertex1.Position == vertex2.Position;
    public static Vector3 operator -(Vertex vertex1, Vertex vertex2) => vertex1.Position - vertex2.Position;
    public static Vector3 operator +(Vertex vertex1, Vertex vertex2) => vertex1.Position + vertex2.Position;
    
    public override string ToString()
    {
        return "( " + Position.X + ", " + Position.Y + ", " + Position.Z + " )";
    }
}