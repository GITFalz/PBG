using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

// Deletion Class
public class SSBOBase 
{
    public static List<SSBOBase> SSBOs = new List<SSBOBase>();
    
    public int ID;
    public int DataCount;
    protected int dataSize;
    
    public SSBOBase()
    {
        SSBOs.Add(this);
    }

    public static void Delete()
    {
        foreach (var ssbo in SSBOs)
        {
            GL.DeleteBuffer(ssbo.ID);
        }
        SSBOs.Clear();
    }
}

public class SSBO<T> : SSBOBase where T : struct
{
    public SSBO() : this(new T[0]) {}
    public SSBO(List<T> data) : this(data.ToArray()) {}
    public SSBO(T[] data) : base()
    {
        ID = GL.GenBuffer();
        DataCount = data.Length;
        dataSize = Marshal.SizeOf(typeof(T));
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, DataCount * dataSize, data, BufferUsageHint.DynamicDraw);
        SSBOs.Add(this);
    }

    public void Bind(int bindingPoint)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, ID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Update(List<T> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        DataCount = newData.Count;
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, DataCount * dataSize, newData.ToArray());
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

    public void Unload()
    {
        GL.DeleteBuffer(ID);
        SSBOs.Remove(this);
    }
}