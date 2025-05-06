using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ChunkGenerator
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static Vector2[] spline = 
    [
        new Vector2(-1, 0),
        new Vector2(1, 1f)
    ];

    public static float GetSplineVector(float noise)
    {
        if (spline.Length == 0)
            return 0;
    
        // Handle noise below the first spline point
        if (noise <= spline[0].X)
            return spline[0].Y;
    
        // Iterate through the spline segments
        for (int i = 0; i < spline.Length - 1; i++)
        {
            if (noise >= spline[i].X && noise <= spline[i + 1].X)
            {
                // Calculate t as the normalized position between spline[i].X and spline[i + 1].X
                float t = (noise - spline[i].X) / (spline[i + 1].X - spline[i].X);
                return Mathf.Lerp(spline[i].Y, spline[i + 1].Y, t);
            }
        }

        // Handle noise above the last spline point
        return spline[^1].Y;
    }

    public static float GetSpecNoise(Vector3 position, float size = 100)
    {
        return NoiseLib.Noise(1, ((float)position.X + 0.001f) / size, ((float)position.Z + 0.001f) / size); 
    }

    public static void GenerateBox(ref Chunk chunkData, Vector3i chunkPosition, Vector3i origin, Vector3i size)
    {
        Vector3i[] corners = GetBoxCorners(origin, size);
        
        foreach (Vector3i corner in corners)
        {
            if (IsPointInChunk(chunkPosition, corner))
            {
                Vector3i min = new Vector3i(
                    Math.Min(origin.X, origin.X + size.X),
                    Math.Min(origin.Y, origin.Y + size.Y),
                    Math.Min(origin.Z, origin.Z + size.Z)
                );

                Vector3i max = new Vector3i(
                    Math.Max(origin.X, origin.X + size.X),
                    Math.Max(origin.Y, origin.Y + size.Y),
                    Math.Max(origin.Z, origin.Z + size.Z)
                );
                
                int startX = Mathf.Max(min.X - chunkPosition.X, 0);
                int sizeX = Mathf.Min(max.X - chunkPosition.X, 32);
                
                int startY = Mathf.Max(min.Y - chunkPosition.Y, 0);
                int sizeY = Mathf.Min(max.Y - chunkPosition.Y, 32);
                
                int startZ = Mathf.Max(min.Z - chunkPosition.Z, 0);
                int sizeZ = Mathf.Min(max.Z - chunkPosition.Z, 32);

                for (int x = startX; x < sizeX; x++)
                {
                    for (int y = startY; y < sizeY; y++)
                    {
                        for (int z = startZ; z < sizeZ; z++)
                        {
                            chunkData.blockStorage.SetBlock(x, y, z, new Block(BlockState.Solid, 1));
                        }
                    }
                }
                
                return;
            }
        }
    }
    
    private static bool IsPointInChunk(Vector3i chunkPosition, Vector3i point)
    {
        return point.X >= chunkPosition.X && point.X < chunkPosition.X + WIDTH &&
               point.Y >= chunkPosition.Y && point.Y < chunkPosition.Y + HEIGHT &&
               point.Z >= chunkPosition.Z && point.Z < chunkPosition.Z + DEPTH;
    }

    private static Vector3i[] GetBoxCorners(Vector3i origin, Vector3i size)
    {
        Vector3i[] corners = [
            origin,
            origin + new Vector3i(size.X, 0, 0),
            origin + new Vector3i(0, 0, size.Z),
            origin + new Vector3i(size.X, 0, size.Z),
            origin + new Vector3i(0, size.Y, 0),
            origin + new Vector3i(size.X, size.Y, 0),
            origin + new Vector3i(0, size.Y, size.Z),
            origin + new Vector3i(size.X, size.Y, size.Z)
        ];
        
        return corners;
    }

    private static Block GetBlockAtHeight(float terrainHeight, int currentHeight)
    {
        if (terrainHeight > currentHeight + 3)
            return new Block(BlockState.Solid, 2);
        if (terrainHeight > currentHeight + 1)
            return new Block(BlockState.Solid, 1);
        return new Block(BlockState.Solid, 0);
    }

    public static Vector3i RegionPosition(Vector3i position)
    {
        return (position.X >> 4, position.Y >> 4, position.Z >> 4);
    }
    
    public static Vector3i ChunkPosition(Vector3i position)
    {
        return (position.X & 15, position.Y & 15, position.Z & 15);
    }

    public static int ChunkIndex(Vector3i position)
    {
        position = ChunkPosition(position);
        return position.X + (position.Y << 4) + (position.Z << 8);
    }
}