using OpenTK.Mathematics;

public class ChunkGenerationProcess : ThreadProcess
{
    public Chunk ChunkData;
    public bool Loaded = false;

    public ChunkGenerationProcess(Chunk chunkData, bool loaded = false)
    {
        ChunkData = chunkData;
        Loaded = loaded;
    }

    protected override void Function()
    {
        if (ChunkData.Stage >= ChunkStage.Generated)
            return;
            
        if (!Loaded)
        {
            if (GenerateChunk(ref ChunkData, ChunkData.GetWorldPosition(), ThreadIndex) == -1)
                return;

            ChunkData.Stage = ChunkStage.Generated; 
            ChunkManager.WaitingToPopulateQueue.Enqueue(ChunkData);
        } 
        else
        {
            ChunkData.LoadChunk();
            ChunkData.Stage = ChunkStage.Populated;
            ChunkManager.GenerateMeshQueue.Enqueue(ChunkData);
            ChunkData.Save = false;
        }
    }

    private static int GenerateChunk(ref Chunk chunkData, Vector3i position, int threadIndex)
    {
        if (!CWorldMultithreadNodeManager.GetNodeManager(threadIndex, out var nodeManager) || chunkData.Stage == ChunkStage.Empty)
            return -1;

        nodeManager.IsBeingUsed = true;

        Vector2 chunkWorldPosition2D = new Vector2(position.X + 0.001f, position.Z + 0.001f);
        for (var x = 0; x < Chunk.WIDTH; x++) 
        {
            for (var z = 0; z < Chunk.DEPTH; z++)
            {
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