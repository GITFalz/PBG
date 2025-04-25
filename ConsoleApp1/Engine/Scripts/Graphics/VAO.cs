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
    
    public void LinkToVAO<T>(int location, int size, VBO<T> vbo) where T : struct
    {
        Bind();
        vbo.Bind();
        GL.VertexAttribPointer(location, size, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(location);
        vbo.Unbind();
        Unbind();
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