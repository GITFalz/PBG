    using OpenTK.Graphics.OpenGL4;

    public class TBO
    {
        public int ID;
        public int TextureID;

        public TBO(List<int> data)
        {
            ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, ID);
            GL.BufferData(BufferTarget.TextureBuffer, data.Count * sizeof(int), data.ToArray(), BufferUsageHint.StaticDraw);

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, TextureID);

            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32i, ID);
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

        public void Delete()
        {
            GL.DeleteBuffer(ID);
            GL.DeleteTexture(TextureID);
        }
    }