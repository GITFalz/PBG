using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public class VBOBase : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;

    public VBOBase() : base() { _bufferCount++; }

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
        return "VBO";
    }
}

public class VBO<T> : VBOBase where T : struct
{
    public VBO() : this(Array.Empty<T>()) { }
    public VBO(List<T> data) : this(data.ToArray()) { }
    public VBO(T[] data) : base()
    {
        Create(data);
    }

    public void Renew(List<T> data) => Renew(data.ToArray());
    public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
    public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

    public void Update(List<T> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Marshal.SizeOf(typeof(T)), newData.ToArray());
        Unbind();
    }

    public void Renew(T[] data) 
    {
        GL.DeleteBuffer(ID); // The buffer needs to be deleted before creating a new one
        Create(data);
        GL.Finish();
    }

    private void Create(T[] data)
    {
        ID = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * Marshal.SizeOf(typeof(T)), data, BufferUsageHint.DynamicDraw);
        Unbind();
    }
}