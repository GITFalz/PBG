using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class SSBO
{
    public int ID;
    private const int Matrix4SizeInBytes = sizeof(float) * 16;
    private const int IntSizeInBytes = sizeof(int);


    public SSBO(List<Matrix4> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Matrix4SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public SSBO(List<int> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * IntSizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public SSBO(List<Vector3> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Vector3.SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public SSBO(List<Vector4i> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Vector4i.SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public SSBO(List<Vector2i> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Vector2i.SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
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

    public void Update(List<Matrix4> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Matrix4SizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<int> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * IntSizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector3> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Vector3.SizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector4i> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Vector4i.SizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector2i> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Vector2i.SizeInBytes, newData.ToArray());
        Unbind();
    }



    public int[] ReadData(int count)
    {
        int[] data = new int[count];
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, count * IntSizeInBytes, data);
        Unbind();
        return data;
    }

    public void Delete()
    {
        Unbind();
        GL.DeleteBuffer(ID);
    }
}