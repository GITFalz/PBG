using OpenTK.Mathematics;

public class Triangle
{
    public string ID = "ID";
    public static Triangle Empty = new Triangle(new Vertex(Vector3.Zero), new Vertex(Vector3.Zero), new Vertex(Vector3.Zero));

    public Vertex A;
    public Vertex B;
    public Vertex C;

    public Vector2 UvA;
    public Vector2 UvB;
    public Vector2 UvC;

    public Edge AB;
    public Edge BC;
    public Edge CA;
    
    public Vector3 Normal;
    public Vector3 Center = Vector3.Zero;

    public bool Inverted = false;

    public Triangle(Vertex a, Vertex b, Vertex c, Vector2 uvA, Vector2 uvB, Vector2 uvC, Edge ab, Edge bc, Edge ca)
    {
        A = a;
        B = b;
        C = c;

        UvA = uvA;
        UvB = uvB;
        UvC = uvC;

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

        UpdateNormal();
    }

    public Triangle(Vertex a, Vertex b, Vertex c, Edge ab, Edge bc, Edge ca) : this(a, b, c, new(0, 0), new(0, 0), new(0, 0), ab, bc, ca) {}
    public Triangle(Vertex a, Vertex b, Vertex c) : this(a, b, c, new(a, b), new(b, c), new(c, a)) {}

    public void UpdateNormal()
    {
        Vector3 edge1 = B - A;
        Vector3 edge2 = C - A;
        Normal = Vector3.Cross(edge1, edge2).Normalized();
    }
    
    public void Invert()
    {
        (A, B) = (B, A);
        Inverted = !Inverted;
    }
    
    public bool TwoVertSamePosition()
    {
        return A & B || A & C || B & C;
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

    public void SetUv(Vertex vertex, Vector2 uv)
    {
        if (A == vertex)
            UvA = uv;
        else if (B == vertex)
            UvB = uv;
        else if (C == vertex)
            UvC = uv;
    }

    public bool GetEdgeWithout(Vertex vertex, out Edge? edge)
    {
        edge = null;
        if (AB.HasNot(vertex))
        {
            edge = AB;
            return true;
        }
        if (BC.HasNot(vertex))
        {
            edge = BC;
            return true;
        }
        if (CA.HasNot(vertex))
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

    public bool HasSharedVertexUv(Triangle triangle, out Vector2 uv1, out Vector2 uv2)
    {
        if (triangle.GetVertexUv(A, out uv2))
        {
            uv1 = UvA;
            return true;
        }
        if (triangle.GetVertexUv(B, out uv2))
        {
            uv1 = UvB;
            return true;
        }
        if (triangle.GetVertexUv(C, out uv2))
        {
            uv1 = UvC;
            return true;
        }

        uv1 = Vector2.Zero;
        uv2 = Vector2.Zero;
        return false;
    }

    public bool HasSameVertices()
    {
        return A == B || A == C || B == C;
    }

    public bool GetVertexUv(Vertex vertex, out Vector2 uv)
    {
        if (A == vertex)
        {
            uv = UvA;
            return true;
        }
        if (B == vertex)
        {
            uv = UvB;
            return true;
        }
        if (C == vertex)
        {
            uv = UvC;
            return true;
        }
        uv = Vector2.Zero;
        return false;
    }

    public void Not(Edge edge, out Edge A, out Edge B)
    {
        if (AB == edge)
        {
            A = BC;
            B = CA;
        }
        else if (BC == edge)
        {
            A = CA;
            B = AB;
        }
        else
        {
            A = AB;
            B = BC;
        }
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

    public Vector3[] GetVerticesPosition()
    {
        return [A, B, C];
    }

    public Vector3[] GetVerticesPosition(Vector3 offset)
    {
        return [A + offset, B + offset, C + offset];
    }

    public Vertex[] GetVertices()
    {
        return [A, B, C];
    }

    public Vector3 CalculateCenter()
    {
        Center = new Vector3(A + B + C) / 3f;
        return Center;
    }

    public Vector3 GetCenter()
    {
        return new Vector3(A + B + C) / 3f;
    }

    public Vector2[] GetUvs()
    {
        return [UvA, UvB, UvC];
    }
    
    public override string ToString()
    {
        return $"Triangle:\nA: {A}\nB: {B}\nC: {C}";
    }
}

public static class Blocker
{
    private static volatile bool isBlocked = false;

    public static void Block()
    {
        if (!Thread.CurrentThread.IsBackground)
            return;

        isBlocked = true;
        while (isBlocked)
        {
            Thread.Sleep(1);
        }
    }

    public static void Unblock()
    {
        isBlocked = false;
    }

    public static void Reset()
    {
        isBlocked = true;
    }
}