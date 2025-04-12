using OpenTK.Graphics.OpenGL4;

public class IBO : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;
    
    public IBO(List<uint> data) : base()
    {
        Create(data.ToArray());
        _bufferCount++;
    }

    public void Renew(List<uint> data) => Create(data.ToArray());
    public void Renew(uint[] data) => Create(data);
    public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID); 
    public void Unbind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); 

    private void Create(uint[] data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
    }

    public override void DeleteBuffer()
    {
        GL.DeleteBuffer(ID);
        _bufferCount--;
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