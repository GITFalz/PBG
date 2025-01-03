using OpenTK.Mathematics;

public class Vertex
{
    public Vector3 Position;
    public Triangle? ParentTriangle;
    public int Index = -1;
    
    public bool WentThrough = false;
    
    public List<Vertex> SharedVertices = new List<Vertex>();
    
    private Vector3 _placeHolderPosition;

    public Vertex(Vector3 position, Triangle? parentTriangle = null)
    {
        _placeHolderPosition = position;
        Position = _placeHolderPosition;
        ParentTriangle = parentTriangle;
    }
    
    public Vertex(Vertex vertex) : this(vertex.Position) { }
    public Vertex(Vertex vertex, Triangle triangle) : this(vertex.Position, triangle) { }
    
    public void AddSharedVertex(Vertex vertex)
    {
        if (!SharedVertices.Contains(vertex))
            SharedVertices.Add(vertex);
        if (!vertex.SharedVertices.Contains(this))
            vertex.SharedVertices.Add(this);
    }

    public void AddSharedVertexToAll(List<Vertex> list, Vertex vertex)
    {
        foreach (var sharedVertex in list)
        {
            foreach (var vert in vertex.SharedVertices)
            {
                sharedVertex.AddSharedVertex(vert);    
            }
            
            sharedVertex.AddSharedVertex(vertex);
        }
    }

    public bool GetTwoOtherVertex(out Vertex? a, out Vertex? b)
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
    
    public void RemoveSharedVertex(Vertex vertex)
    {
        SharedVertices.Remove(vertex);
        if (vertex.SharedVertices.Contains(this))
            vertex.SharedVertices.Remove(this);
    }
    
    public void RemoveSingleSharedVertex(Vertex vertex)
    {
        SharedVertices.Remove(vertex);
    }

    public void RemoveInstanceFromAll()
    {
        foreach (var vert in SharedVertices)
        {
            vert.RemoveSingleSharedVertex(this);
        }
    }
    
    public void RemoveSharedVertexFromAll(List<Vertex> list, Vertex vertex)
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
    
    public List<Vertex> ToList()
    {
        List<Vertex> list = new List<Vertex>() { this };
        foreach (var sharedVertex in SharedVertices)
            list.Add(sharedVertex);
        return list;
    }

    public void MoveVertex(Vector3 offset)
    {
        _placeHolderPosition += offset;
        Position = _placeHolderPosition;
        foreach (var sharedVertex in SharedVertices)
        {
            sharedVertex.Position = Position;
        }
    }
    
    public void MoveStep(Vector3 offset, float step)
    {
        if (step <= 0)
            return;
        
        _placeHolderPosition += offset;
        Position =
        (
            Mathf.Floor(_placeHolderPosition.X / step) * step,
            Mathf.Floor(_placeHolderPosition.Y / step) * step,
            Mathf.Floor(_placeHolderPosition.Z / step) * step
        );
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
        _placeHolderPosition = Position;
    }

    public void SetAllPosition(Vector3 position)
    {
        SetPosition(position);
        foreach (var vert in SharedVertices)
            vert.SetPosition(position);
    }

    public void ToPosition()
    {
        _placeHolderPosition = Position;
    }
    
    public void ResetPosition()
    {
        Position = _placeHolderPosition;
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