using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public class TBOBase : BufferBase
{
    public int ID;
    public int TextureID;

    private static int _bufferCount = 0;

    public TBOBase() : base() { _bufferCount++; }

    public override void DeleteBuffer()
    {
        GL.DeleteBuffer(ID);
        GL.DeleteTexture(TextureID);
        _bufferCount--;
        base.DeleteBuffer();
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "TBO";
    }
}

public class TBO<T> : TBOBase where T : struct
{
    public TBO(List<T> data)
    {
        Create(data.ToArray());
    }

    public void Renew(List<T> data) => Renew(data.ToArray());
    public void Renew(T[] data)
    {
        GL.DeleteBuffer(ID); // The buffer needs to be deleted before creating a new one
        GL.DeleteTexture(TextureID); // The texture needs to be deleted before creating a new one
        Create(data);
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

    public void Unbind() => GL.BindTexture(TextureTarget.TextureBuffer, 0);

    private void Create(T[] data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, ID);
        GL.BufferData(BufferTarget.TextureBuffer, data.Length * Marshal.SizeOf(typeof(T)), data, BufferUsageHint.StaticDraw);

        TextureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, TextureID);

        GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, ID);
    }
}