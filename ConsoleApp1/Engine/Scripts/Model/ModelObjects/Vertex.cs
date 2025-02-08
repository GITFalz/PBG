using OpenTK.Mathematics;

public class OldVertex
{
    public Vector3 Position;
    public OldTriangle? ParentTriangle;
    public int Index = -1;
    public int BoneIndex = 0;
    public bool WentThrough = false;
    
    public List<OldVertex> SharedVertices = new List<OldVertex>();

    public OldVertex(Vector3 position, OldTriangle? parentTriangle = null)
    {
        Position = position;
        ParentTriangle = parentTriangle;
    }
    
    public OldVertex(OldVertex vertex) : this(vertex.Position) { }
    public OldVertex(OldVertex vertex, OldTriangle triangle) : this(vertex.Position, triangle) { }
    
    public void AddSharedVertex(OldVertex vertex)
    {
        if (!SharedVertices.Contains(vertex))
            SharedVertices.Add(vertex);
        if (!vertex.SharedVertices.Contains(this))
            vertex.SharedVertices.Add(this);
    }

    public void AddSharedVertexToAll(List<OldVertex> list, OldVertex vertex)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var sharedVertex = list[i];
            for (int j = 0; j < vertex.SharedVertices.Count; j++)
            {
                sharedVertex.AddSharedVertex(vertex.SharedVertices[j]);
            }
            sharedVertex.AddSharedVertex(vertex);
        }
    }

    public bool GetTwoOtherVertex(out OldVertex? a, out OldVertex? b)
    {
        a = null;
        b = null;
        if (ParentTriangle == null)
            return false;

        if (ParentTriangle.A == this)
        {
            a = ParentTriangle.B;
            b = ParentTriangle.C;
        }
        else if (ParentTriangle.B == this)
        {
            a = ParentTriangle.A;
            b = ParentTriangle.C;
        }
        else if (ParentTriangle.C == this)
        {
            a = ParentTriangle.A;
            b = ParentTriangle.B;
        }

        return true;
    }    

    public void SetBoneIndexForAll(int index)
    {
        BoneIndex = index;
        foreach (var sharedVertex in SharedVertices)
        {
            sharedVertex.BoneIndex = index;
        }
    }
    
    public bool SharesTriangleWith(OldVertex vertex)
    {
        List<OldVertex> sharedVertices = [
            ..SharedVertices,
            this
        ];

        List<OldVertex> sharedVerticesVertex = [
            ..vertex.SharedVertices,
            vertex
        ];

        foreach (var sharedVertex in sharedVertices)
        {
            foreach (var sharedVertexVertex in sharedVerticesVertex)
            {
                if (sharedVertexVertex.ParentTriangle == sharedVertex.ParentTriangle)
                    return true;
            }
        }

        return false;
    }
    
    public void RemoveSharedVertex(OldVertex vertex)
    {
        SharedVertices.Remove(vertex);
        if (vertex.SharedVertices.Contains(this))
            vertex.SharedVertices.Remove(this);
    }
    
    public void RemoveSingleSharedVertex(OldVertex vertex)
    {
        SharedVertices.Remove(vertex);
    }

    public void RemoveInstanceFromAll()
    {
        for (int i = 0; i < SharedVertices.Count; i++)
        {
            SharedVertices[i].RemoveSingleSharedVertex(this);
        }
    }
    
    public void RemoveSharedVertexFromAll(List<OldVertex> list, OldVertex vertex)
    {
        foreach (var sharedVertex in list)
        {
            foreach (var vert in vertex.SharedVertices)
            {
                sharedVertex.RemoveSharedVertex(vert);    
            }
            
            sharedVertex.RemoveSharedVertex(vertex);
        }
    }
    
    public List<OldVertex> ToList()
    {
        List<OldVertex> list = new List<OldVertex>() { this };
        foreach (var sharedVertex in SharedVertices)
            list.Add(sharedVertex);
        return list;
    }

    public void MoveVertex(Vector3 offset)
    {
        Position += offset;
        foreach (var sharedVertex in SharedVertices)
        {
            sharedVertex.Position = Position;
        }
    }
    
    public Vector3 GetNormal()
    {
        Vector3 normal = Vector3.UnitY;
        if (ParentTriangle != null)
            normal = ParentTriangle.Normal;
        return normal;
    }
    
    public Vector3 GetAverageNormal()
    {
        Vector3 normal = GetNormal();
        foreach (var sharedVertex in SharedVertices)
        {
            normal += sharedVertex.GetNormal();
        }
        return normal / (SharedVertices.Count + 1);
    }

    public void SetPosition(Vector3 pos)
    {
        Position = pos;
    }

    public void SetAllPosition(Vector3 position)
    {
        SetPosition(position);
        foreach (var vert in SharedVertices)
            vert.SetPosition(position);
    }
    
    public bool WentThroughOne()
    {
        return SharedVertices.Any(sharedVertex => sharedVertex.WentThrough) || WentThrough;
    }

    public override string ToString()
    {
        return "( " + Position.X + ", " + Position.Y + ", " + Position.Z + " )";
    }
}

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