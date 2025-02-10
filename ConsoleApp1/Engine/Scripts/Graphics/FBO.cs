using OpenTK.Graphics.OpenGL4;

public class FBO
{
    public int ID;
    public int depthTextureID;
    
    public FramebufferErrorCode FramebufferErrorCode;

    public FBO(int width, int height)
    {
        depthTextureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthTextureID);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);

        ID = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTextureID, 0);

        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        FramebufferErrorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteFramebuffer(ID);
    }
}