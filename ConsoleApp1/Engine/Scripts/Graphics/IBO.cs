using OpenTK.Graphics.OpenGL4;

public class IBO : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;
    
    public IBO() : this(Array.Empty<uint>()) { }
    public IBO(List<uint> data) : this(data.ToArray()) {}
    public IBO(uint[] data) : base()
    {
        Create(data);
        _bufferCount++;
    }

    public void Renew(List<uint> data) => Renew(data.ToArray());
    public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID); 
    public void Unbind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); 

    private void Create(uint[] data)
    {
        ID = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
        Unbind();
    }

    public void Renew(uint[] data)
    {
        GL.DeleteBuffer(ID); // The buffer needs to be deleted before creating a new one
        Create(data);
    }

    public override void DeleteBuffer()
    {
        GL.DeleteBuffer(ID);
        _bufferCount--;
        base.DeleteBuffer();
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "IBO";
    }
}