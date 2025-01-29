using OpenTK.Mathematics;

public class Quad
{
    public OldTriangle A;
    public OldTriangle B;
    
    public Quad(OldTriangle a, OldTriangle b)
    {
        A = a; A.ParentQuad = this;
        B = b; B.ParentQuad = this;
    }

    /// <summary>
    /// prerequisite: the 4 vectors are
    /// a => bottom left
    /// b => bottom right
    /// c => top left
    /// d => top right
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    
    public Quad(OldVertex a, OldVertex b, OldVertex c, OldVertex d)
    {
        OldVertex vertex1 = new OldVertex(b);
        OldVertex vertex2 = new OldVertex(c);
        
        b.AddSharedVertex(vertex1);
        c.AddSharedVertex(vertex2);
        
        A = new OldTriangle(a, b, c);
        B = new OldTriangle(vertex2, vertex1, d);
    }

    public override string ToString()
    {
        return $"{A.C.Position}     {B.C.Position}\n\n\n\n{A.A.Position}     {A.B.Position}";
    }
}