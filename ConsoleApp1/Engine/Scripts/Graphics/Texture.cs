using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class Texture
{
    public int ID;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public Texture(string filePath)
    {
        ID = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult texture = ImageResult.FromStream(File.OpenRead("../../../Assets/Textures/" + filePath), ColorComponents.RedGreenBlueAlpha);
        
        Width = texture.Width;
        Height = texture.Height;
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);

        Unbind();
    }
    
    public void Bind() { GL.BindTexture(TextureTarget.Texture2D, ID); }
    public void Unbind() { GL.BindTexture(TextureTarget.Texture2D, 0); }
    public void Delete() { GL.DeleteTexture(ID); }
}