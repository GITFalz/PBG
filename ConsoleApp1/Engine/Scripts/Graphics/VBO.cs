using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public class VBOBase
{
    public static List<VBOBase> VBOs = new List<VBOBase>();

    public int ID;

    public VBOBase()
    {
        VBOs.Add(this);
    }

    public static void Delete()
    {
        foreach (var vbo in VBOs)
        {
            GL.DeleteBuffer(vbo.ID);
        }
        VBOs.Clear();
    }
}

public class VBO<T> : VBOBase where T : struct
{
    public VBO(List<T> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Marshal.SizeOf(typeof(T)), data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public void Bind() { GL.BindBuffer(BufferTarget.ArrayBuffer, ID); }
    public void Unbind() { GL.BindBuffer(BufferTarget.ArrayBuffer, 0); }

    public void Update(List<T> newData)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, newData.Count * Marshal.SizeOf(typeof(T)), newData.ToArray());
        Unbind();
    }
}