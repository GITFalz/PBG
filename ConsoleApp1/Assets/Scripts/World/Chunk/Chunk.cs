using ConsoleApp1.Assets.Scripts.World.Blocks;
using OpenTK.Mathematics;
using ConsoleApp1.Engine.Scripts.Core.Data;

public class Chunk
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static Vector2[] spline = 
    [
        new Vector2(-1, 0),
        new Vector2(-0.4f, 0.05f),
        new Vector2(-0.3f, 0.2f),
        new Vector2(0f, 0.3f),
        new Vector2(0.2f, 0.7f),
        new Vector2(1, 0.8f)
    ];
    
    public static void GenerateChunk(ref ChunkData chunkData, Vector3i position)
    {
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                float specNoise = GetSpecNoise(new Vector3(x, 0, z) + position);
                
                float splineVector = GetSplineVector(specNoise);
                
                float noise = NoiseLib.Noise(4, 
                    ((float)x + position.X + 0.001f) / 20f,
                    ((float)z + position.Z + 0.001f) / 20f
                    );
                
                int height = Mathf.FloorToInt(Mathf.Lerp(20, 100, (float)(noise * 0.05 + splineVector)));
                
                //Console.WriteLine(noise + " Spline: " + specNoise  + " " + splineVector + " " + height);
                int terrainHeight = Mathf.Min(Mathf.Max((height - position.Y), 0), 32);

                for (int y = 0; y < terrainHeight; y++)
                {
                    Block? block = GetBlockAtHeight(height, y + position.Y);
                    chunkData.blockStorage.SetBlock(x, y, z, block);
                }
            }
        }
    }
    
    public static void GenerateFlatChunk(ref ChunkData chunkData, Vector3i position)
    {
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                int height = 10;
                int terrainHeight = Mathf.Min(Mathf.Max((height - position.Y), 0), 32);

                for (int y = 0; y < terrainHeight; y++)
                {
                    Block? block = GetBlockAtHeight(height, y + position.Y);
                    chunkData.blockStorage.SetBlock(x, y, z, block);
                }
            }
        }
    }

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

    private static float GetSpecNoise(Vector3 position)
    {
        return NoiseLib.Noise(4, ((float)position.X + 0.001f) / 100, ((float)position.Z + 0.001f) / 100);
    }

    public static void GenerateOcclusion(Block?[] blocks, int width = WIDTH, int lod = 0)
    {
        int index = 0;
        for (int y = 0; y < width; y++)
        {
            for (int z = 0; z < width; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Block? block = blocks[index];
                    
                    if (block != null)
                    {
                        byte occlusion = 0;
                            
                        for (int i = 0; i < 6; i++)
                        {
                            if (VoxelData.InBounds(x, y, z, i, width) && blocks[index + VoxelData.IndexOffsetLod[lod, i]] != null)
                                occlusion |= VoxelData.ShiftPosition[i];
                        }
                            
                        block.occlusion = occlusion;
                    }
                        
                    index++;
                }
            }
        }
    }
    
    public static void GenerateMesh(Block?[] blocks, ChunkData chunkData)
    {
        int index = 0;
        
        for (int y = 0; y < 32; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Block? block = blocks[index];
                    
                    if (block != null)
                    {
                        int[] ids;
                        try
                        {
                            ids = BlockManager.GetBlock(block.blockData).GetUVs();
                        }
                        catch (NullReferenceException e)
                        {
                            return;
                        }
                        
                        for (int side = 0; side < 6; side++)
                        {
                            if (((block.check >> side) & 1) == 0 && ((block.occlusion >> side) & 1) == 0)
                            {
                                block.check |= (byte)(1 << side);
                                
                                for (int tris = 0; tris < 6; tris++)
                                {
                                    chunkData.meshData.tris.Add(VoxelData.TrisIndexTable[tris] + (uint)chunkData.meshData.verts.Count);
                                }
                                
                                int i = index;
                                int loop = VoxelData.FirstLoopBase[side](y, z);
                                int height = 1;
                                int width = 1;
                                while (loop > 0)
                                {
                                    i += VoxelData.FirstOffsetBase[side];
                                    
                                    Block? blockI = blocks[i];
                                    
                                    if (blockI == null)
                                        break;

                                    if (((blockI.check >> side) & 1) != 0 ||
                                        ((blockI.occlusion >> side) & 1) != 0 ||
                                        blockI.blockData != block.blockData)
                                        break;

                                    blockI.check |= (byte)(1 << side);

                                    height++;
                                    loop--;
                                }

                                i = index;
                                loop = VoxelData.SecondLoopBase[side](x, z);
                                
                                bool quit = false;
                                
                                while (loop > 0)
                                {
                                    i += VoxelData.SecondOffsetBase[side];
                                    int up = i;
                                    
                                    for (int j = 0; j < height; j++)
                                    {
                                        Block? upBlock = blocks[up];
                                        
                                        if (upBlock == null)
                                        {
                                            quit = true;
                                            break;
                                        }

                                        if (((upBlock.check >> side) & 1) != 0 ||
                                            ((upBlock.occlusion >> side) & 1) != 0 ||
                                            upBlock.blockData != block.blockData)
                                        {
                                            quit = true;
                                            break;
                                        }
                                        
                                        up += VoxelData.FirstOffsetBase[side];
                                    }
                                    
                                    if (quit) break;
                                    
                                    up = i;
                                    
                                    for (int j = 0; j < height; j++) {
                                        
                                        Block? upBlock = blocks[up];
                                        
                                        if (upBlock == null)
                                            continue;
                                        
                                        upBlock.check |= (byte)(1 << side);
                                        up += VoxelData.FirstOffsetBase[side];
                                    }

                                    width++;
                                    loop--;
                                }

                                Vector3[] positions = VoxelData.PositionOffset[side](width, height);
                                Vector3 position = new Vector3(x, y, z) + chunkData.position;

                                int id = ids[side];
                                
                                chunkData.meshData.uvs.Add(new Vector2(width, height));
                                chunkData.meshData.uvs.Add(new Vector2(width, 0));
                                chunkData.meshData.uvs.Add(new Vector2(0, 0));
                                chunkData.meshData.uvs.Add(new Vector2(0, height));
                                
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                
                                chunkData.meshData.verts.Add(position + positions[0]);
                                chunkData.meshData.verts.Add(position + positions[1]);
                                chunkData.meshData.verts.Add(position + positions[2]);
                                chunkData.meshData.verts.Add(position + positions[3]);
                            }
                        }
                    }
                    
                    index++;
                }
            }
        }
    }
    public static void GenerateBox(ref ChunkData chunkData, Vector3i chunkPosition, Vector3i origin, Vector3i size)
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
                            chunkData.blockStorage.SetBlock(x, y, z, new Block(1, 0));
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
        Vector3i[] corners = new Vector3i[8];
        
        corners[0] = origin;
        corners[1] = origin + new Vector3i(size.X, 0, 0);
        corners[2] = origin + new Vector3i(0, 0, size.Z);
        corners[3] = origin + new Vector3i(size.X, 0, size.Z);
        corners[4] = origin + new Vector3i(0, size.Y, 0);
        corners[5] = origin + new Vector3i(size.X, size.Y, 0);
        corners[6] = origin + new Vector3i(0, size.Y, size.Z);
        corners[7] = origin + new Vector3i(size.X, size.Y, size.Z);
        
        return corners;
    }
    
    private static bool IsPointInBox(Vector3i min, Vector3i max, Vector3i point)
    {
        return point.X >= min.X && point.X < max.X &&
               point.Y >= min.Y && point.Y < max.Y &&
               point.Z >= min.Z && point.Z < max.Z;
    }

    private static Block? GetBlockAtHeight(float terrainHeight, int currentHeight)
    {
        //if (terrainHeight > currentHeight + 2)
            //return new Block(2, 0);
        
        return new Block(1, 0);
    }
}