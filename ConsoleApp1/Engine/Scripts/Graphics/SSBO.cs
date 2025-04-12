using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

// Deletion Class
public class SSBOBase : BufferBase
{
    public int ID;
    public int DataCount;
    protected int dataSize;

    private static int _bufferCount = 0;
    
    public SSBOBase() : base() { _bufferCount++; }

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
        return "SSBO";
    }
}

public class SSBO<T> : SSBOBase where T : struct
{
    public SSBO() : this(new T[0]) {}
    public SSBO(List<T> data) : this(data.ToArray()) {}
    public SSBO(T[] data) : base()
    {
        Create(data);
    }

    public void Renew(List<T> data) => Create(data.ToArray());
    public void Renew(T[] data) => Create(data);

    public void Bind(int bindingPoint)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, ID);
    }

    public void Unbind() => GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

    public void Update(List<T> newData, int bindingPoint)
    {
        Update(newData.ToArray(), bindingPoint);
    }

    public void Update(T[] newData, int bindingPoint)
    {
        Bind(bindingPoint);
        DataCount = newData.Length;
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, DataCount * dataSize, newData);
        Unbind();
    }

    public T[] ReadData(int count)
    {
        T[] data = new T[count];
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, count * dataSize, data);
        Unbind();
        return data;
    }

    private void Create(T[] data)
    {
        ID = GL.GenBuffer();
        DataCount = data.Length;
        dataSize = Marshal.SizeOf(typeof(T));
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, DataCount * dataSize, data, BufferUsageHint.DynamicDraw);
    }
}