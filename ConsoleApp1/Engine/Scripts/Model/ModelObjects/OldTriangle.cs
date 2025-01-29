using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class OldTriangle
{
    public OldVertex A;
    public OldVertex B;
    public OldVertex C;
    
    public Vector3 Normal;
    
    public Quad? ParentQuad;

    public OldTriangle(OldVertex a, OldVertex b, OldVertex c, Quad? parentQuad = null)
    {
        A = a; A.ParentTriangle = this; A.Index = 0;
        B = b; B.ParentTriangle = this; B.Index = 1;
        C = c; C.ParentTriangle = this; C.Index = 2;
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

    public List<Vector3> GetVerticesPosition()
    {
        return new List<Vector3> {A.Position, B.Position, C.Position};
    }

    public List<OldVertex> GetVertices()
    {
        return new List<OldVertex> {A, B, C};
    }
}