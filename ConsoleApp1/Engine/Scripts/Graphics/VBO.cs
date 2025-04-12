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
    public VBO(List<T> data) : base()
    {
        Create(data.ToArray());
    }

    public void Renew(List<T> data) => Create(data.ToArray());
    public void Renew(T[] data) => Create(data);
    public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
    public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

    public void Update(List<T> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Marshal.SizeOf(typeof(T)), newData.ToArray());
        Unbind();
    }

    private void Create(T[] data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * Marshal.SizeOf(typeof(T)), data, BufferUsageHint.DynamicDraw);
    }
}