using System.Diagnostics;
using OpenTK.Mathematics;

public class ChunkLODGenerationProcess : ThreadProcess
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public LODChunk LODChunk;
    public bool GenerationSuccess = false;

    public ChunkLODGenerationProcess(LODChunk chunk) : base()
    {
        LODChunk = chunk;
    }

    public override void Function()
    {
        Console.WriteLine($"Generating LOD chunk at {LODChunk.Position} on thread {ThreadIndex}");
        GenerationSuccess = GenerateChunk(ref LODChunk, LODChunk.Position, ThreadIndex) != -1;
    }

    /// <summary>
    /// The basic function that is called when the process is completed.
    /// </summary>
    protected override void OnCompleteBase()
    {
        Console.WriteLine($"LOD chunk generation completed at {LODChunk.Position} on thread {ThreadIndex}");
        LODChunk.Mesh.CreateChunkSolid();
    }

    public static int GenerateChunk(ref LODChunk chunk, Vector3i position, int threadIndex)
    {
        if (!CWorldMultithreadNodeManager.GetNodeManager(threadIndex, out var nodeManager))
            return -1;

        FullBlockStorage chunkData = new FullBlockStorage();
        nodeManager.IsBeingUsed = true;
        int scale = (int)Mathf.Pow(2, chunk.Resolution);

        Vector2 chunkWorldPosition2D = new Vector2(position.X + 0.001f, position.Z + 0.001f);
        for (var x = 0; x < WIDTH; x++) 
        {
            for (var z = 0; z < DEPTH; z++)
            {
                if (chunk.Blocked)
                    return -1;

                nodeManager.Init(new Vector2(x * scale, z * scale) + chunkWorldPosition2D);

                for (int y = 0; y < HEIGHT; y++)
                {
                    chunkData[x, y, z] = nodeManager.GetBlock(y * scale + position.Y);
                }
            }
        }

        nodeManager.IsBeingUsed = false;

        int index = 0;
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (chunk.Blocked)
                        return -1;

                    Vector3i blockPosition = (x, y, z);
                    Block currentBlock = chunkData[x, y, z];

                    bool isSolid = currentBlock.IsSolid();
                    bool isLiquid = currentBlock.IsLiquid();

                    if (isSolid || isLiquid)
                    {
                        foreach (var cBlock in BlockManager.BlockPriorityList)
                        {
                            bool isValid = true;
                            if (cBlock.BlockChecker == null)
                                continue;   
                                
                            foreach (var mask in cBlock.BlockChecker.BlockMasks)
                            {
                                if (chunk.Blocked)
                                    return -1;

                                Vector3i offset = blockPosition + mask.Offset;
                                Block block;
                                if (offset.X < 0 || offset.X > 31 || offset.Y < 0 || offset.Y > 31 || offset.Z < 0 || offset.Z > 31)
                                {
                                    block = Block.Solid;
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
                                chunkData[blockPosition] = new Block(isSolid ? BlockState.Solid : BlockState.Liquid, cBlock.index); 
                                break;
                            }
                        }
                    }

                    index++;
                }
            }
        }

        index = 0;
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Vector3i blockPosition = (x, y, z);
                    Block block = chunkData[index];

                    if (block.IsSolid())
                    { 
                        block.ResetOcclusion();
                        
                        for (int i = 0; i < 6; i++)
                        {
                            if (chunk.Blocked)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, WIDTH))
                            {
                            }
                            else
                            {
                                Vector3i offset = blockPosition + VoxelData.SideNormal[i];
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
                            if (chunk.Blocked)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, WIDTH))
                            {

                            }
                            else
                            {
                                Vector3i offset = blockPosition + VoxelData.SideNormal[i];
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



        byte[] _checks = new byte[WIDTH * HEIGHT * DEPTH];
        int[] blockMap = new int[WIDTH * DEPTH];
        index = 0;
        chunk.Mesh.ClearMeshData();
        
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int x = 0; x < WIDTH; x++)
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

                                Vector3i blockPosition = (x, y, z);

                                int id = ids[side];

                                chunk.Mesh.AddFace(blockPosition, width, height, side, id, (0, 0, 0, 0));
                            }
                        }
                    }

                    index++;
                }
            }
        }

        return 1;
    }
}