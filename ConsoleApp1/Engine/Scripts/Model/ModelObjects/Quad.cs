using OpenTK.Mathematics;

public class Quad
{
    public Triangle A;
    public Triangle B;
    
    public Quad(Triangle a, Triangle b)
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
    
    public Quad(Vertex a, Vertex b, Vertex c, Vertex d)
    {
        Vertex vertex1 = new Vertex(b);
        Vertex vertex2 = new Vertex(c);
        
        b.AddSharedVertex(vertex1);
        c.AddSharedVertex(vertex2);
        
        A = new Triangle(a, b, c);
        B = new Triangle(vertex2, vertex1, d);
    }

    public override string ToString()
    {
        return $"{A.C.Position}     {B.C.Position}\n\n\n\n{A.A.Position}     {A.B.Position}";
    }
}