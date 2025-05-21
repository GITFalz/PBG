using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class Texture : BufferBase
{
    public int ID;
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    private static int _bufferCount = 0;
    
    public Texture(string filePath, TextureLocation textureLocation = TextureLocation.NormalTexture) : base()
    {
        Create(filePath, textureLocation);
        _bufferCount++;
    }

    public void Renew(string filePath, TextureLocation textureLocation = TextureLocation.NormalTexture)
    {
        GL.DeleteTexture(ID); // The texture needs to be deleted before creating a new one
        Create(filePath, textureLocation);
    }

    public void Bind()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);
    }

    public void Bind(TextureUnit textureUnit)
    {
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(TextureTarget.Texture2D, ID);
    }

    public static void Bind(int id)
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, id);
    }

    public void Unbind() => GL.BindTexture(TextureTarget.Texture2D, 0); 
    public static void UnbindAll() => GL.BindTexture(TextureTarget.Texture2D, 0);

    private void Create(string filePath, TextureLocation textureLocation)
    {
        ID = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        string path = textureLocation == TextureLocation.NormalTexture ? Game.texturePath : Game.texturePath;

        StbImage.stbi_set_flip_vertically_on_load(1);

        string fullPath = Path.Combine(path, filePath);
        ImageResult texture;

        using (var stream = File.OpenRead(fullPath))
        {
            texture = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }
        
        Width = texture.Width;
        Height = texture.Height;
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);

        Unbind();
    }

    public override void DeleteBuffer()
    {
        GL.DeleteTexture(ID);
        _bufferCount--;
        base.DeleteBuffer();
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

public enum TextureLocation
{
    NormalTexture,
    CustumTexture,
}