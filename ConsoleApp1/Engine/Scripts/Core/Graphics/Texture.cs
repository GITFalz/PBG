using ConsoleApp1.Engine.Scripts.Core.Folder;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace ConsoleApp1.Engine.Scripts.Core.Graphics;

public class Texture
{
    public int ID;
    
    public Texture(string path)
    {
        string[] files = FileManager.GetFolderPngFiles("../../../Assets/Textures");
        
        
        ID = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, ID);
        
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        if (files.Length == 0)
        {
            Console.WriteLine("No texture files found in the folder.");
            return;
        }
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult texture = ImageResult.FromStream(File.OpenRead(files[0]), ColorComponents.RedGreenBlueAlpha);
        
        int width = texture.Width;
        int height = texture.Height;
        
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, files.Length);
        GL.TextureSubImage3D(ID, 0, 0, 0, 0, width, height, 1, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);
        
        for (int i = 1; i < files.Length; i++)
        {
            texture = ImageResult.FromStream(File.OpenRead(files[i]), ColorComponents.RedGreenBlueAlpha);

            GL.TextureSubImage3D(ID, 0, 0, 0, i, width, height, 1, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);
        }
        
        Unbind();
    }
    
    public void Bind() { GL.BindTexture(TextureTarget.Texture2DArray, ID); }
    public void Unbind() { GL.BindTexture(TextureTarget.Texture2DArray, 0); }
    public void Delete() { GL.DeleteTexture(ID); }
}