using OpenTK.Graphics.OpenGL4;

public class VAO : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;
    
    public VAO() : base()
    {
        ID = GL.GenVertexArray();
        GL.BindVertexArray(ID);
        _bufferCount++;
    }

    public void Renew()
    {
        GL.DeleteVertexArray(ID); // The VAO needs to be deleted before creating a new one
        ID = GL.GenVertexArray();
        GL.BindVertexArray(ID);
    }
    
    public void LinkToVAO<T>(int location, int size, VertexAttribPointerType type, int stride, int offset, VBO<T> vbo) where T : struct
    {
        vbo.Bind();
        Link(location, size, type, stride, offset);
        vbo.Unbind();
    }

    public void IntLinkToVAO<T>(int location, int size, VertexAttribIntegerType type, int stride, int offset, VBO<T> vbo) where T : struct
    {
        vbo.Bind();
        IntLink(location, size, type, stride, offset);
        vbo.Unbind();
    }

    public void InstanceLinkToVAO<T>(int location, int size, VertexAttribPointerType type, int stride, int offset, VBO<T> vbo, int divisor = 1) where T : struct
    {
        vbo.Bind();
        InstanceLink(location, size, type, stride, offset, divisor);
        vbo.Unbind();
    }

    public void Link(int location, int size, VertexAttribPointerType type, int stride, int offset)
    {
        GL.VertexAttribPointer(location, size, type, false, stride, offset);
        GL.EnableVertexAttribArray(location);
    }

    public void IntLink(int location, int size, VertexAttribIntegerType type, int stride, int offset)
    {
        GL.VertexAttribIPointer(location, size, type, stride, offset);
        GL.EnableVertexAttribArray(location);
    }

    public void InstanceLink(int location, int size, VertexAttribPointerType type, int stride, int offset, int divisor = 1)
    {
        Link(location, size, type, stride, offset);
        GL.VertexAttribDivisor(location, divisor);
    }

    public void InstanceIntLink(int location, int size, VertexAttribIntegerType type, int stride, int offset, int divisor = 1)
    {
        IntLink(location, size, type, stride, offset);
        GL.VertexAttribDivisor(location, divisor);
    }
    
    
    public void Bind() => GL.BindVertexArray(ID);
    public void Unbind() => GL.BindVertexArray(0);
    public override void DeleteBuffer()
    {
        GL.DeleteVertexArray(ID);
        _bufferCount--;
        base.DeleteBuffer();
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "VAO";
    }
}