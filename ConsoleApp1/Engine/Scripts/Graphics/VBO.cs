using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class VBO
{
    public int ID;
    
    public VBO(List<Vector3> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector3.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public VBO(List<Vector4> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector4.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public VBO(List<Vector2> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector2.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }
    
    public VBO(List<Vector2i> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Vector2i.SizeInBytes, data.ToArray(), BufferUsageHint.StaticDraw);
    }
    
    public VBO(List<int> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(int), data.ToArray(), BufferUsageHint.StaticDraw);
    }

    public void Bind() { GL.BindBuffer(BufferTarget.ArrayBuffer, ID); }
    public void Unbind() { GL.BindBuffer(BufferTarget.ArrayBuffer, 0); }
    public void Delete() { GL.DeleteBuffer(ID); }
    public void Update(List<Vector3> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Vector3.SizeInBytes, newData.ToArray());
        Unbind();
    }

    public void Update(List<Vector4> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Vector4.SizeInBytes, newData.ToArray());
        Unbind();
    }
    
    public void Update(List<Vector2> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Vector2.SizeInBytes, newData.ToArray());
        Unbind();
    }
    
    public void Update(List<Vector2i> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Vector2i.SizeInBytes, newData.ToArray());
        Unbind();
    }
    
    public void Update(List<int> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * sizeof(int), newData.ToArray());
        Unbind();
    }
}