using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

// Deletion Class
public class SSBOBase 
{
    public static List<SSBOBase> SSBOs = new List<SSBOBase>();
    
    public int ID;
    
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
    public SSBO(List<T> data) : base()
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Marshal.SizeOf(typeof(T)), data.ToArray(), BufferUsageHint.DynamicDraw);
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
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Marshal.SizeOf(typeof(T)), newData.ToArray());
        Unbind();
    }
}