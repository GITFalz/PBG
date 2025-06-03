using OpenTK.Mathematics;

public class ChunkRenderingProcess : ThreadProcess
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public ChunkEntry Entry;
    public Chunk chunk => Entry.Chunk;

    private bool GenerationSuccess = false;
    private bool OcclusionSuccess = false;
    private bool Success = false;

    public ChunkRenderingProcess(ChunkEntry entry) : base()
    {
        Entry = entry;
    }

    public override void Function()
    {
        OcclusionSuccess = GenerateOcclusion(Entry) != -1;
        if (!OcclusionSuccess) return;
        GenerationSuccess = GenerateGreedyMesh(Entry.Chunk) != -1;
        Success = OcclusionSuccess && GenerationSuccess;
    }

    protected override void OnCompleteBase()
    {
        if (!Success)   
            Console.WriteLine($"Chunk Rendering Process completed for chunk {Entry.Chunk.GetWorldPosition()} with success false and free chunk: {Entry.FreeChunk}");

        Entry.Process = null;
        if (Entry.CheckDelete()) return;

        if (!Success)
        { 
            Entry.TrySetStage(ChunkStage.ToBeFreed);
            return;
        }
        
        Entry.TrySetStage(ChunkStage.Rendered);
    }

    public static int GenerateOcclusion(ChunkEntry entry, int width = WIDTH, int lod = 0)
    {
        Chunk chunkData = entry.Chunk;

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
                            if (entry.Blocked)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, width))
                            {
                                Vector3i newPosition = position + VoxelData.SideNormal[i] + chunkData.GetWorldPosition();
                                if (WorldManager.GetBlockState(newPosition, out var b) == 0 && b.IsSolid())
                                {
                                    block.SetOcclusion(i);
                                }
                            }
                            else
                            {
                                Vector3i offset = position + VoxelData.SideNormal[i];
                                int newIndex = offset.X + (offset.Z << 5) + (offset.Y << 10);
                                
                                if (chunkData[newIndex].IsSolid())
                                    block.SetOcclusion(i);      
                            }
                        }
                    }
                    else if (block.IsLiquid())
                    {
                        block.ResetOcclusion();
                        
                        for (int i = 0; i < 6; i++)
                        {
                            if (entry.Blocked)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, width))
                            {
                                Vector3i newPosition = position + VoxelData.SideNormal[i] + chunkData.GetWorldPosition();
                                if (WorldManager.GetBlockState(newPosition, out var b) == 0 && (b.IsSolid() || b.IsLiquid()))
                                {
                                    block.SetOcclusion(i);
                                }
                            }
                            else
                            {
                                Vector3i offset = position + VoxelData.SideNormal[i];
                                int newIndex = offset.X + (offset.Z << 5) + (offset.Y << 10);
                                
                                var b = chunkData[newIndex];
                                if (b.IsSolid() || b.IsLiquid())
                                    block.SetOcclusion(i);      
                            }
                        }
                    }

                    chunkData[index] = block;
                        
                    index++;
                }
            }
        }

        return 1;
    }

    public static void GenerateLOD(Chunk chunkData)
    {
        
    }
    
    public static int GenerateGreedyMesh(Chunk chunkData)
    {
        byte[] _checks = new byte[WIDTH * HEIGHT * DEPTH];
        int[] blockMap = new int[WIDTH * DEPTH];
        int index = 0;
        chunkData.ClearMeshData();
        Vector3i chunkWorldPosition = chunkData.GetWorldPosition();
        
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
                        uint blockId = block.ID;
                        try
                        {
                            ids = BlockManager.GetBlock(blockId).GetIndices();
                        }
                        catch (Exception)
                        {
                            return -1;
                        }

                        for (int side = 0; side < 6; side++)
                        {
                            byte sideShift = (byte)(1 << side);

                            if ((_checks[index] & sideShift) == 0 && (block.blockData & VoxelData.OcclusionMask[side]) == 0)
                            {
                                bool quit = false;
                                int i = index;
                                int height = 1;
                                int width = 1;

                                _checks[index] |= sideShift;

                                for (int a = 0; a < VoxelData.FirstLoopBase[side](y, z); a++)
                                {
                                    i += VoxelData.FirstOffsetBase[side];

                                    Block blockI = chunkData[i];

                                    if (blockI.IsAir() || (blockI.blockData & VoxelData.OcclusionMask[side]) != 0 || (_checks[i] & sideShift) != 0 || (blockI.blockData & 15) != blockId)
                                        break;

                                    _checks[i] |= sideShift;

                                    height++;
                                }

                                i = index;
                                int[] ups = new int[height];

                                for (int a = 0; a < VoxelData.SecondLoopBase[side](x, z); a++)
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

                                    for (int j = 0; j < ups.Length; j++)
                                    {
                                        _checks[ups[j]] |= sideShift;
                                    }

                                    width++;
                                }

                                Vector3i position = (x, y, z);

                                int id = ids[side];

                                chunkData.AddFace(position, width, height, side, id, (0, 0, 0, 0));
                            }
                        }
                    }

                    index++;
                }
            }
        }

        return 1;
    }

    public static int GenerateMesh(ChunkEntry entry)
    {
        Chunk chunkData = entry.Chunk;
        lock (chunkData)
        {
            Vector3i chunkWorldPosition = chunkData.GetWorldPosition();
            chunkData.ClearMeshData();

            int index = 0;
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        if (entry.Blocked)
                            return -1;

                        Block block = chunkData[index];

                        if (block.FullyOccluded())
                        {
                            index++;
                            continue;
                        }
                        
                        if (block.IsSolid() || block.IsLiquid())
                        {
                            int[] ids;

                            try
                            {
                                ids = BlockManager.GetBlock(block.ID).GetIndices(); 
                            }
                            catch (Exception)
                            {
                                return -1;
                            }
                            
                            for (int side = 0; side < 6; side++)
                            {
                                if (entry.Blocked)
                                    return -1;

                                if (block.Occluded(side))
                                    continue;

                                int id = ids[side];
                                Vector3i position = (x, y, z);
                                Vector3i worldBlockPosition = position + chunkWorldPosition;

                                Vector4i ao = (0, 0, 0, 0);
                                for (int i = 0; i < 4; i++)
                                {
                                    var corners = VoxelData.AoOffset[side][i];

                                    Vector3i offset1 = corners[0];
                                    Vector3i offset2 = corners[1];
                                    Vector3i offset3 = corners[2];

                                    bool isOffset1 = HasBlock(worldBlockPosition + offset1, chunkData);
                                    bool isOffset2 = HasBlock(worldBlockPosition + offset2, chunkData);
                                    bool isOffset3 = HasBlock(worldBlockPosition + offset3, chunkData);

                                    if (isOffset1 && isOffset3)
                                        ao[i] = 3;
                                    else if (isOffset2 && (isOffset1 || isOffset3))
                                        ao[i] = 2;
                                    else if (isOffset1 || isOffset2 || isOffset3)
                                        ao[i] = 1;
                                    else
                                        ao[i] = 0;
                                }

                                if (block.IsLiquid())
                                {
                                    chunkData.AddTransparentFace(position, 1, 1, side, id, ao);
                                }
                                else if (block.IsSolid())
                                {
                                    chunkData.AddFace(position, 1, 1, side, id, ao);
                                }
                            }
                        }

                        index++;
                    }
                }
            }
        }
        
        return 1;
    }

    public static bool HasBlock(Vector3i position, Chunk chunkData)
    {
        if (position.X < 0 || position.X >= WIDTH || position.Y < 0 || position.Y >= HEIGHT || position.Z < 0 || position.Z >= DEPTH)
            return WorldManager.GetBlock(position);

        return chunkData[position].IsSolid();
    }
}