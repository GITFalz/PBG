using OpenTK.Mathematics;
using StbImageSharp;

namespace ConsoleApp1.Engine.Scripts.Core.Data;

public static class TextureData
{
    public static List<byte[]> SplitTextureAtlas(string path, int width, int height)
    {
        ImageResult atlas = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
        
        int atlasWidth = atlas.Width;
        int atlasHeight = atlas.Height;
        
        int cols = atlasWidth / width;
        int rows = atlasHeight / height;
        
        List<byte[]> textures = [];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                byte[] subImage = ExtractSubImage(atlas, col * width, row * height, width, height);
                textures.Add(subImage);
            }
        }
        
        return textures;
    }

    public static List<Vector3> GetAverageColors(string path, int width, int height)
    {
        ImageResult atlas = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
        
        int atlasWidth = atlas.Width;
        int atlasHeight = atlas.Height;
        
        int cols = atlasWidth / width;
        int rows = atlasHeight / height;
        
        List<Vector3> colors = [];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 avgColor = ExtractAverageColor(atlas, col * width, row * height, width, height);
                colors.Add(avgColor);
            }
        }
        
        return colors;
    }
    
    private static byte[] ExtractSubImage(ImageResult atlas, int startX, int startY, int width, int height)
    {
        byte[] subImage = new byte[width * height * 4];
        byte[] data = atlas.Data;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int atlasIndex = ((startY + y) * atlas.Width + (startX + x)) * 4;
                int subImageIndex = (y * width + x) * 4;
                
                Array.Copy(data, atlasIndex, subImage, subImageIndex, 4);
            }
        }
        
        return subImage;
    }

    private static Vector3 ExtractAverageColor(ImageResult atlas, int startX, int startY, int width, int height)
    {
        Vector3 avgColor = Vector3.Zero;
        byte[] data = atlas.Data;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int atlasIndex = ((startY + y) * atlas.Width + (startX + x)) * 4;
                byte r = data[atlasIndex];
                byte g = data[atlasIndex + 1];
                byte b = data[atlasIndex + 2];
                
                avgColor += new Vector3(r, g, b);
            }
        }
        
        avgColor /= width * height;
        
        return avgColor / 255.0f;
    }
}