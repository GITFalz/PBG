﻿using ConsoleApp1.Engine.Scripts.Core.Data;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class TextureArray
{
    public static List<TextureArray> TextureArrays = new List<TextureArray>();

    public int ID;

    public TextureArray(string atlasPath, int cellWidth, int cellHeight)
    {
        List<byte[]> textureData = TextureData.SplitTextureAtlas(Path.Combine(Game.texturePath, atlasPath), cellWidth, cellHeight);
        
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
        TextureArrays.Add(this);
    }

    public void Bind()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, ID);
    }
    public void Unbind() { GL.BindTexture(TextureTarget.Texture2DArray, 0); }
    public static void Delete() 
    {
        foreach (var textureArray in TextureArrays)
        {
            GL.DeleteTexture(textureArray.ID);
        }
        TextureArrays.Clear();
    }
}