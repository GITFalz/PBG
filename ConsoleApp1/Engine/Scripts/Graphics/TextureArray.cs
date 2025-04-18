using ConsoleApp1.Engine.Scripts.Core.Data;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

public class TextureArray : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;
    private TextureArrayLoadType _textureArrayLoadType = TextureArrayLoadType.CellSize;

    public TextureArray(string atlasPath, int cellWidth, int cellHeight, TextureArrayLoadType textureArrayLoadType = TextureArrayLoadType.CellSize) : base()
    {
        _textureArrayLoadType = textureArrayLoadType;
        Create(atlasPath, cellWidth, cellHeight);
        _bufferCount++;
    }

    public void Renew(string atlasPath, int cellWidth, int cellHeight) => Create(atlasPath, cellWidth, cellHeight);

    public void Bind()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, ID);
    }
    public void Unbind() => GL.BindTexture(TextureTarget.Texture2DArray, 0); 

    private void Create(string atlasPath, int cellWidth, int cellHeight)
    {
        List<byte[]> textureData;

        if (_textureArrayLoadType == TextureArrayLoadType.CellSize || _textureArrayLoadType == TextureArrayLoadType.CellSizeFlipped)
        {
            bool flipped = _textureArrayLoadType == TextureArrayLoadType.CellSizeFlipped;
            textureData = TextureData.SplitTextureAtlasCellSize(Path.Combine(Game.texturePath, atlasPath), cellWidth, cellHeight, flipped);
        }      
        else
        {
            bool flipped = _textureArrayLoadType == TextureArrayLoadType.AtlasSizeFlipped;
            textureData = TextureData.SplitTextureAtlasAtlasSize(Path.Combine(Game.texturePath, atlasPath), cellWidth, cellHeight, out int newWidth, out int newHeight, flipped);
            cellWidth = newWidth;
            cellHeight = newHeight;
        }
        
        int layers = textureData.Count;
        
        ID = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, ID);
        
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, cellWidth, cellHeight, layers);

        for (int i = 0; i < layers; i++)
        {
            GL.TextureSubImage3D(ID, 0, 0, 0, i, cellWidth, cellHeight, 1, PixelFormat.Rgba, PixelType.UnsignedByte, textureData[i]);
        }
        
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
        return "TextureArray";
    }
}

public enum TextureArrayLoadType
{
    CellSize,
    CellSizeFlipped,
    AtlasSize,
    AtlasSizeFlipped
}