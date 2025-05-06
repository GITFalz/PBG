using OpenTK.Mathematics;

public class ChunkGenerationProcess : ThreadProcess
{
    public ChunkEntry Entry;
    public bool Loaded = false;

    public ChunkGenerationProcess(ChunkEntry entry, bool loaded = false)
    {
        Entry = entry;
        Loaded = loaded;
    }

    protected override void Function()
    {
        Chunk chunk = Entry.Chunk;

        if (!Loaded)
        {
            if (Entry.CheckDelete()) return;
            
            if (GenerateChunk(ref Entry, chunk.GetWorldPosition(), ThreadIndex) == -1)
            {
                if (Entry.CheckDelete()) return;

                Entry.SetTimer(1f);
                Entry.TrySetStage(ChunkStage.ToBeGenerated);
                return;
            }

            if (Entry.CheckDelete()) return;

            Entry.TrySetStage(ChunkStage.Generated);
        } 
        else
        {   
            if (Entry.CheckDelete()) return;

            Entry.TrySetStage(ChunkStage.Populated);
        }
    }

    private static int GenerateChunk(ref ChunkEntry entry, Vector3i position, int threadIndex)
    {
        if (!CWorldMultithreadNodeManager.GetNodeManager(threadIndex, out var nodeManager))
            return -1;

        Chunk chunkData = entry.Chunk;
        nodeManager.IsBeingUsed = true;

        Vector2 chunkWorldPosition2D = new Vector2(position.X + 0.001f, position.Z + 0.001f);
        for (var x = 0; x < Chunk.WIDTH; x++) 
        {
            for (var z = 0; z < Chunk.DEPTH; z++)
            {
                if (entry.Blocked)
                    return -1;

                nodeManager.Init(new Vector2(x, z) + chunkWorldPosition2D);
                float value = nodeManager.GetValue();

                //float specNoise = GetSpecNoise(new Vector3(x, 0, z) + position, 200);
                
                float splineVector = value;//GetSplineVector(specNoise);
                
                //float noise = NoiseLib.Noise(4, ((float)x + position.X + 0.001f) / 20f, ((float)z + position.Z + 0.001f) / 20f);
                //int height = Mathf.FloorToInt(Mathf.Lerp(20, 100, (float)(noise * 0.05 + splineVector)));
                int height = Mathf.FloorToInt(Mathf.Lerp(20, 100, splineVector));

                int terrainHeight = Mathf.Min(Mathf.Max((height - position.Y), 0), 32);
                int waterHeight = Mathf.Min(Mathf.Max((45 - position.Y), 0), 32);

                for (int y = 0; y < terrainHeight; y++)
                {   
                    if (entry.Blocked)
                        return -1;

                    chunkData[x, y, z] = new Block(BlockState.Solid, 0);
                }
                
                if (height <= 45)
                {
                    for (int y = terrainHeight; y < waterHeight; y++)
                    {
                        chunkData[x, y, z] = new Block(BlockState.Liquid, 0);
                    }
                }
            }
        }

        nodeManager.IsBeingUsed = false;

        return 1;
    }
}