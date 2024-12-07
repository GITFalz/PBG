using OpenTK.Graphics.OpenGL4;

namespace ConsoleApp1.Engine.Scripts.Core.Graphics;

public class IBO
{
    public int ID;
    
    public IBO(List<uint> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Count * sizeof(uint), data.ToArray(), BufferUsageHint.StaticDraw);
    }
    
    public void Bind() { GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID); }
    public void Unbind() { GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); }
    public void Delete() { GL.DeleteBuffer(ID); }
    public void Update(List<uint> newData)
    {
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, newData.Count * sizeof(uint), newData.ToArray());
    }
}