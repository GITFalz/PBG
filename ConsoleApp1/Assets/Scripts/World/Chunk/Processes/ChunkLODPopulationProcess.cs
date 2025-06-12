using OpenTK.Mathematics;

public class ChunkLODPopulationProcess : ThreadProcess
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public ChunkEntry Entry;
    public Chunk chunk => Entry.Chunk;

    public ChunkLODPopulationProcess(int Resolution, ChunkEntry entry) : base()
    {
        Entry = entry;
    }

    public override bool Function()
    {
        Entry.generationTime.Start();
        bool success = PopulateChunk(Entry) != -1;
        Entry.generationTime.Stop();
        return success;
    }

    protected override void OnCompleteBase()
    {
        Entry.Process = null;
        if (Entry.CheckDelete()) return;

        if (Failed)
        {
            Entry.TrySetStage(ChunkStage.ToBeFreed);
            return;
        }
        
        Entry.TrySetStage(ChunkStage.Populated);
    }

    public static int PopulateChunk(ChunkEntry entry)
    {
        Chunk chunk = entry.Chunk;
        Vector3i chunkWorldPosition = chunk.GetWorldPosition();
        int index = 0;

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (entry.Blocked)
                        return -1;

                    Vector3i position = (x, y, z);
                    Block currentBlock = chunk[index];

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
                                if (entry.Blocked)
                                    return -1;

                                Vector3i offset = position + mask.Offset;
                                Block block;
                                if (offset.X < 0 || offset.X > 31 || offset.Y < 0 || offset.Y > 31 || offset.Z < 0 || offset.Z > 31)
                                {
                                    WorldManager.GetBlockState(chunkWorldPosition + offset, out block);
                                } 
                                else
                                {
                                    int i = offset.X + offset.Z * WIDTH + offset.Y * WIDTH * HEIGHT;
                                    block = chunk[i];
                                }
                                
                                if (!mask.IsValid(block))
                                {
                                    isValid = false;
                                    break;
                                }    
                            }

                            if (isValid)
                            {
                                chunk[position] = new Block(isSolid ? BlockState.Solid : BlockState.Liquid, (uint)cBlock.index);
                                break;
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