using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class FBO
{
    public static List<FBO> FBOs = new List<FBO>();

    public int ID;
    public int colorTextureID;
    public int depthTextureID;
    
    public FramebufferErrorCode FramebufferErrorCode;

    public FBO(int width, int height)
    {
        colorTextureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, colorTextureID);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

        depthTextureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthTextureID);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

        ID = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTextureID, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTextureID, 0);

        DrawBuffersEnum[] drawBuffers = { DrawBuffersEnum.ColorAttachment0 };
        GL.DrawBuffers(1, drawBuffers);

        FramebufferErrorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (FramebufferErrorCode != FramebufferErrorCode.FramebufferComplete)
            Console.WriteLine("Framebuffer is not complete: " + FramebufferErrorCode);

        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        FBOs.Add(this);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
    }

    public void BindTexture()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, colorTextureID);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void UnbindTexture()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void SaveFramebufferToPNG(int width, int height, string filePath)
    {
        byte[] pixels = new byte[width * height * 4]; 

        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        using (var image = new Image<Rgba32>(width, height))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (y * width + x) * 4;
                    byte r = pixels[pixelIndex + 0];
                    byte g = pixels[pixelIndex + 1];
                    byte b = pixels[pixelIndex + 2];
                    byte a = pixels[pixelIndex + 3];

                    image[x, height - y - 1] = new Rgba32(r, g, b, a); 
                }
            }

            filePath = Path.Combine(Game.texturePath, filePath);
            image.Save(filePath);

            Console.WriteLine($"Framebuffer saved to {filePath}");
        }
    }

    public void Delete()
    {
        GL.DeleteFramebuffer(ID);
        GL.DeleteTexture(colorTextureID);
        GL.DeleteTexture(depthTextureID);
    }

    public static void DeleteAll()
    {
        foreach (var fbo in FBOs)
        {
            GL.DeleteFramebuffer(fbo.ID);
            GL.DeleteTexture(fbo.colorTextureID);
            GL.DeleteTexture(fbo.depthTextureID);
        }
        FBOs.Clear();
    }
}