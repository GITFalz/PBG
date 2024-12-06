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
        
        List<byte[]> textures = new List<byte[]>();

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
}