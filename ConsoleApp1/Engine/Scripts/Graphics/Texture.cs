using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class Texture : BufferBase
{
    public int ID;
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    private static int _bufferCount = 0;
    
    public Texture(string filePath) : base()
    {
        Create(filePath);
        _bufferCount++;
    }

    public void Renew(string filePath) => Create(filePath);

    public void Bind()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);
    }

    public static void Bind(int id)
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, id);
    }

    public void Unbind() => GL.BindTexture(TextureTarget.Texture2D, 0); 
    public static void UnbindAll() => GL.BindTexture(TextureTarget.Texture2D, 0);

    private void Create(string filePath)
    {
        ID = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult texture = ImageResult.FromStream(File.OpenRead(Path.Combine(Game.texturePath, filePath)), ColorComponents.RedGreenBlueAlpha);
        
        Width = texture.Width;
        Height = texture.Height;
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);

        Unbind();
    }

    public override void DeleteBuffer()
    {
        GL.DeleteTexture(ID);
        _bufferCount--;
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "Texture";
    }
}