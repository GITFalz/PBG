using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public class TBOBase
{
    public static List<TBOBase> TBOs = new List<TBOBase>();

    public int ID;
    public int TextureID;

    public TBOBase()
    {
        TBOs.Add(this);
    }

    public static void Delete()
    {
        foreach (var tbo in TBOs)
        {
            GL.DeleteBuffer(tbo.ID);
        }
        TBOs.Clear();
    }
}

public class TBO<T> : TBOBase where T : struct
{
    public TBO(List<T> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, ID);
        GL.BufferData(BufferTarget.TextureBuffer, data.Count * Marshal.SizeOf(typeof(T)), data.ToArray(), BufferUsageHint.StaticDraw);

        TextureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, TextureID);

        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, ID);

        TBOs.Add(this);
    }

    public void Update(List<int> data)
    {
        GL.BindBuffer(BufferTarget.TextureBuffer, ID);
        GL.BufferSubData(BufferTarget.TextureBuffer, IntPtr.Zero, data.Count * sizeof(int), data.ToArray());
    }

    public void Bind(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.TextureBuffer, TextureID);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
    }
}