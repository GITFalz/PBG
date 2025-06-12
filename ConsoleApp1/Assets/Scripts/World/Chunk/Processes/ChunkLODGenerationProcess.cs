using System.Diagnostics;
using OpenTK.Mathematics;

public class ChunkLODGenerationProcess : ThreadProcess
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public LODChunk LODChunk;

    public ChunkLODGenerationProcess(LODChunk chunk) : base()
    {
        LODChunk = chunk;
    }

    public override bool Function()
    {
        Status = ThreadProcessStatus.Running;
        return GenerateChunk(ref LODChunk, LODChunk.Position, this) != -1;
    }

    /// <summary>
    /// The basic function that is called when the process is completed.
    /// </summary>
    protected override void OnCompleteBase()
    {
        if (Succeded)
        {
            ChunkLODManager.ChunksToCreateMesh.Enqueue(LODChunk);
        }
    }

    public static int GenerateChunk(ref LODChunk chunk, Vector3i position, ThreadProcess process)
    {
        if (!CWorldMultithreadNodeManager.GetNodeManager(process.ThreadIndex, out var nodeManager))
            return -1;

        FullBlockStorage chunkData = new();
        Dictionary<Vector3i, FullBlockStorage> chunks = [];

        // Initialize the chunks
        for (int i = 0; i < 26; i++)
        {
            chunks.Add(ChunkData.SidePositions[i], new());
        }

        nodeManager.IsBeingUsed = true;
        int scale = (int)Mathf.Pow(2, chunk.Resolution);

        bool HasBlocks = false;
        Vector2 chunkWorldPosition2D = (position.X + 0.001f, position.Z + 0.001f);
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                if (process.Cancel) 
                    return -1;

                nodeManager.Init(new Vector2(x * scale, z * scale) + chunkWorldPosition2D);

                for (int y = 0; y < HEIGHT; y++)
                {
                    Block block = nodeManager.GetBlock(y * scale + position.Y);
                    chunkData[x, y, z] = block;
                    if (!block.IsAir())
                    {
                        HasBlocks = true;
                    }
                }
            }
        }

        if (!HasBlocks)
        {
            chunk.Mesh.ClearMeshData();
            chunk.Mesh.IsDisabled = true;
            chunk.Mesh.HasBlocks = false;
            nodeManager.IsBeingUsed = false;
            foreach (var c in chunks)
            {
                c.Value.Clear();
            }
            return -1;
        }

        foreach (var (offset, blocks) in chunks)
        {
            Vector3i fullOffset = offset * HEIGHT * scale;
            int startX = offset.X == 0 ? 0 : (offset.X == -1 ? 31 : 0);
            int endX = offset.X == 0 ? WIDTH : startX + 1;
            int startZ = offset.Z == 0 ? 0 : (offset.Z == -1 ? 31 : 0);
            int endZ = offset.Z == 0 ? DEPTH : startZ + 1;

            for (var x = startX; x < endX; x++)
            {
                for (var z = startZ; z < endZ; z++)
                {
                    if (process.Cancel)
                        return -1;

                    nodeManager.Init(new Vector2(x * scale, z * scale) + chunkWorldPosition2D + fullOffset.Xz);

                    for (int y = 0; y < HEIGHT; y++)
                    {
                        Block block = nodeManager.GetBlock(y * scale + position.Y + fullOffset.Y);
                        blocks[x, y, z] = block;
                    }
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
                    if (process.Cancel)
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
                                if (process.Cancel)
                                    return -1;

                                Vector3i offset = blockPosition + mask.Offset;
                                Block block;
                                Vector3i chunkOffset = Vector3i.Zero;
                                if (offset.X < 0)
                                {
                                    chunkOffset.X = -1;
                                }
                                else if (offset.X > 31)
                                {
                                    chunkOffset.X = 1;
                                }
                                else if (offset.Y < 0)
                                {
                                    chunkOffset.Y = -1;
                                }
                                else if (offset.Y > 31)
                                {
                                    chunkOffset.Y = 1;
                                }
                                else if (offset.Z < 0)
                                {
                                    chunkOffset.Z = -1;
                                }
                                else if (offset.Z > 31)
                                {
                                    chunkOffset.Z = 1;
                                }

                                if (chunkOffset != Vector3i.Zero)
                                {
                                    Vector3i sideChunkBlockPosition = VoxelData.BlockToRelativePosition(offset);
                                    block = chunks[chunkOffset][sideChunkBlockPosition];
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
                            if (process.Cancel)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, WIDTH))
                            {
                                Vector3i sideChunkBlockPosition = VoxelData.BlockToRelativePosition(x, y, z);
                                FullBlockStorage? blocks = null;
                                switch (i)
                                {
                                    case 0:
                                        blocks = chunks[(0, 0, -1)];
                                        break;
                                    case 1:
                                        blocks = chunks[(1, 0, 0)];
                                        break;
                                    case 2:
                                        blocks = chunks[(0, 1, 0)];
                                        break;
                                    case 3:
                                        blocks = chunks[(-1, 0, 0)];
                                        break;
                                    case 4:
                                        blocks = chunks[(0, -1, 0)];
                                        break;
                                    case 5:
                                        blocks = chunks[(0, 0, 1)];
                                        break;
                                    default:
                                        break;
                                }
                                if (blocks != null)
                                {
                                    if (blocks[sideChunkBlockPosition].IsSolid())
                                        block.SetOcclusion(i);
                                }
                            }
                            else
                            {
                                Vector3i offset = blockPosition + VoxelData.SideNormal[i];
                                if (chunkData[offset].IsSolid())
                                    block.SetOcclusion(i);
                            }
                        }
                    }
                    else if (block.IsLiquid())
                    {
                        block.ResetOcclusion();
                        
                        for (int i = 0; i < 6; i++)
                        {
                            if (process.Cancel)
                                return -1;

                            if (!VoxelData.InBounds(x, y, z, i, WIDTH))
                            {
                                Vector3i sideChunkBlockPosition = VoxelData.BlockToRelativePosition(x, y, z);
                                FullBlockStorage? blocks = null;
                                switch (i)
                                {
                                    case 0:
                                        blocks = chunks[(0, 0, -1)];
                                        break;
                                    case 1:
                                        blocks = chunks[(1, 0, 0)];
                                        break;
                                    case 2:
                                        blocks = chunks[(0, 1, 0)];
                                        break;
                                    case 3:
                                        blocks = chunks[(-1, 0, 0)];
                                        break;
                                    case 4:
                                        blocks = chunks[(0, -1, 0)];
                                        break;
                                    case 5:
                                        blocks = chunks[(0, 0, 1)];
                                        break;
                                    default:
                                        break;
                                }
                                if (blocks != null)
                                {
                                    var b = blocks[sideChunkBlockPosition];
                                    if (b.IsSolid() || b.IsLiquid())
                                        block.SetOcclusion(i);
                                }
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


        bool hasSolidOrLiquid = false;
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
                        if (!hasSolidOrLiquid)
                            hasSolidOrLiquid = true;
                            
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

        if (!hasSolidOrLiquid)
        {
            chunk.Mesh.ClearMeshData();
            chunk.Mesh.IsDisabled = true;
            chunk.Mesh.HasBlocks = false;
            foreach (var c in chunks)
            {
                c.Value.Clear();
            }
            return -1;
        }
        
        foreach (var c in chunks)
        {
            c.Value.Clear();
        }
        return 1;
    }
}