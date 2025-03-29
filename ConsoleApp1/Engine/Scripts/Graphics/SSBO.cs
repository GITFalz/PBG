using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class SSBO
{
    public int ID;
    private const int Matrix4SizeInBytes = sizeof(float) * 16;

    public SSBO(List<Matrix4> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Matrix4SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    // Vector2
    public SSBO(List<Vector2> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * sizeof(float) * 2, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    // Vector4
    public SSBO(List<Vector4> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * sizeof(float) * 4, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    // Vector4i
    public SSBO(List<Vector4i> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * sizeof(int) * 4, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    // Vector2i
    public SSBO(List<Vector2i> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * sizeof(int) * 2, data.ToArray(), BufferUsageHint.DynamicDraw);
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

    public void Delete()
    {
        GL.DeleteBuffer(ID);
    }

    public void Update(List<Matrix4> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Matrix4SizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector2> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * sizeof(float) * 2, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector4> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * sizeof(float) * 4, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector4i> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * sizeof(int) * 4, newData.ToArray());
        Unbind();
    }
    
    public void Update(List<Vector2i> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * sizeof(int) * 2, newData.ToArray());
        Unbind();
    }
}