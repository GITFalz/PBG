using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkGenerator
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public static List<CWorldBlock> BlockPriorityList = [];


    private static bool _blockPriority = true;
    
    public static Vector2[] spline = 
    [
        new Vector2(-1, 0),
        new Vector2(-0.4f, 0.05f),
        new Vector2(-0.3f, 0.2f),
        new Vector2(0f, 0.3f),
        new Vector2(0.2f, 0.7f),
        new Vector2(1, 0.8f)
    ];
    
    public static void GenerateChunk(ref Chunk chunkData, Vector3i position)
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

                int terrainHeight = Mathf.Min(Mathf.Max((height - position.Y), 0), 32);

                for (int y = 0; y < terrainHeight; y++)
                {
                    chunkData.blockStorage.SetBlock(x, y, z, new Block(true, 2));
                }
            }
        }
    }

    public static void PopulateChunk(ref Chunk chunkData)
    {
        if (_blockPriority)
        {
            BlockPriorityList = BlockManager.GetSortedPriorityList();
            _blockPriority = false;
        }

        Vector3i chunkWorldPosition = chunkData.GetWorldPosition();
        int index = 0;

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Vector3i position = (x, y, z);

                    if (chunkData[index].IsSolid())
                    {
                        foreach (var cBlock in BlockPriorityList)
                        {
                            bool isValid = true;

                            foreach (var mask in cBlock.BlockChecker.BlockMasks)
                            {
                                Vector3i offset = position + mask.Offset;
                                Block block;
                                if (offset.X < 0 || offset.X > 31 || offset.Y < 0 || offset.Y > 31 || offset.Z < 0 || offset.Z > 31)
                                {
                                    WorldManager.GetBlock(chunkWorldPosition + offset, out block);
                                } 
                                else
                                {
                                    int i = offset.X + offset.Z * WIDTH + offset.Y * WIDTH * HEIGHT;
                                    block = chunkData[i];
                                }
                                
                                if (!mask.IsValid(block))
                                {
                                    isValid = false;
                                    break;
                                }    
                            }

                            if (isValid)
                            {
                                chunkData.blockStorage.SetBlock(position, new Block(true, cBlock.index));
                                break;
                            }
                        }
                    }

                    index++;
                }
            }
        }
    }

    public static void GenerateFlatChunk(ref Chunk chunkData, Vector3i position)
    {
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                int height = 10;
                int terrainHeight = Mathf.Min(Mathf.Max((height - position.Y), 0), 32);

                for (int y = 0; y < terrainHeight; y++)
                {
                    Block block = GetBlockAtHeight(height, y + position.Y);
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

    public static void GenerateOcclusion(Chunk chunkData, int width = WIDTH, int lod = 0)
    {
        int index = 0;
        for (int y = 0; y < width; y++)
        {
            for (int z = 0; z < width; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3i position = (x, y, z);
                    Block block = chunkData[index];

                    if (block.IsSolid())
                    { 
                        block.ResetOcclusion();
                        for (int i = 0; i < 6; i++)
                        {
                            if (!VoxelData.InBounds(x, y, z, i, width))
                            {
                                Vector3i newPosition = position + VoxelData.SideNormal[i] + chunkData.GetWorldPosition();
                                if (WorldManager.GetBlock(newPosition, out var b) == 0 && b.IsSolid())
                                {
                                    block.SetOcclusion(i);
                                }
                            }
                            else if (chunkData[index + VoxelData.IndexOffsetLod[lod, i]].IsSolid())
                            {
                                block.SetOcclusion(i);
                            }
                        }
                    }

                    chunkData[index] = block;
                        
                    index++;
                }
            }
        }
    }
    
    public static void GenerateMesh(Chunk chunkData)
    {
        byte[] _checks = new byte[WIDTH * HEIGHT * DEPTH];
        int[] blockMap = new int[WIDTH * DEPTH];
        int index = 0;
        
        for (int y = 0; y < 32; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    int blockMapIndex = x + (z << 5);
                    Block block = chunkData[index];
                    
                    if (block.IsSolid())
                    {
                        int blockPillar = blockMap[blockMapIndex];
                        blockPillar |= 1 << y;
                        blockMap[blockMapIndex] = blockPillar;

                        int[] ids;
                        int blockId = block.blockData & 15;
                        try
                        {
                            ids = BlockManager.GetBlock(blockId).GetUVs();
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        
                        for (int side = 0; side < 6; side++)
                        {
                            byte sideShift = (byte)(1 << side);

                            if ((_checks[index] & sideShift) == 0 && (block.blockData & VoxelData.OcclusionMask[side]) == 0)
                            {
                                bool quit = false;
                                int i = index;
                                int loop = VoxelData.FirstLoopBase[side](y, z);
                                int height = 1;
                                int width = 1;

                                _checks[index] |= sideShift;

                                while (loop > 0)
                                {
                                    i += VoxelData.FirstOffsetBase[side];
                                    
                                    Block blockI = chunkData[i];
                                    
                                    if (blockI.IsAir() || (blockI.blockData & VoxelData.OcclusionMask[side]) != 0 || (_checks[i] & sideShift) != 0 || (blockI.blockData & 15) != blockId)
                                        break;

                                    _checks[i] |= sideShift;

                                    height++;
                                    loop--;
                                }

                                i = index;
                                loop = VoxelData.SecondLoopBase[side](x, z);

                                int[] ups = new int[height];
                                
                                while (loop > 0)
                                {
                                    i += VoxelData.SecondOffsetBase[side];
                                    int up = i;
                                    
                                    for (int j = 0; j < height; j++)
                                    {
                                        Block upBlock = chunkData[up];
                                        
                                        if (upBlock.IsAir() || (upBlock.blockData & VoxelData.OcclusionMask[side]) != 0 || (_checks[up] & sideShift) != 0 || (upBlock.blockData & 15) != blockId)
                                        {
                                            quit = true;
                                            break;
                                        }

                                        ups[j] = up;
                                        up += VoxelData.FirstOffsetBase[side];
                                    }
                                    
                                    if (quit) break;

                                    foreach (int upIndex in ups)
                                    {
                                        _checks[upIndex] |= sideShift;
                                    }

                                    width++;
                                    loop--;
                                }

                                Vector3 position = (x, y, z);

                                int id = ids[side];
                            
                                chunkData.HasBlocks = true;

                                chunkData.AddFace(position, (byte)(width - 1), (byte)(height - 1), id, (byte)side);
                            }
                        }
                    }
                    
                    index++;
                }
            }
        }

        chunkData.FullBlockMap = blockMap;
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
                            chunkData.blockStorage.SetBlock(x, y, z, new Block(true, 1));
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
            return new Block(true, 2);
        if (terrainHeight > currentHeight + 1)
            return new Block(true, 1);
        return new Block(true, 0);
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