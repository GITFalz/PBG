using OpenTK.Mathematics;

public static class ChunkPool
{
    public static List<Chunk> Chunks = [];
    public static List<Chunk> FreeChunks = [];

    public static void Update()
    {
        if (FreeChunks.Count > 20)
        {
            for (int i = 0; i < FreeChunks.Count - 20; i++)
            {
                Chunk chunk = FreeChunks[i];
                chunk.Delete();
                FreeChunks.RemoveAt(i);
                i--;
            }
        }
    }


    /// <summary>
    /// Get a chunk from the pool. If no chunks are available, create a new one. Make sure the position is in relative coordinates.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Chunk GetChunk(Vector3i position)
    {
        if (FreeChunks.Count == 0)
        {
            Chunk chunk = new Chunk(RenderType.Solid, position);
            Chunks.Add(chunk);
            return chunk;
        }
        else
        {
            Chunk chunk = FreeChunks[0];
            FreeChunks.RemoveAt(0);
            chunk.SetPosition(position);
            return chunk;
        }
    }

    public static void FreeChunk(Chunk chunk)
    {
        FreeChunks.Add(chunk);
        chunk.Clear();
    }

    public static void Clear()
    {
        foreach (var chunk in Chunks)
        {
            chunk.Delete();
            chunk.Clear();
        }
        
        Chunks.Clear();
        FreeChunks.Clear();
    }

    public static void Print()
    {
        Console.WriteLine($"Chunk Pool: {Chunks.Count} chunks, {FreeChunks.Count} free chunks.");
    }
}