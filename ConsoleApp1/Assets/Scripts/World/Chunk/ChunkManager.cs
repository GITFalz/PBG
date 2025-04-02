using System.Collections.Concurrent;
using OpenTK.Mathematics;

public static class ChunkManager
{
    /// <summary>
    /// All the chunks in the world
    /// </summary>
    public static ConcurrentDictionary<Vector3i, Chunk> ActiveChunks = [];
    public static ConcurrentQueue<Vector3i> GenerateChunkQueue = [];
    public static ConcurrentQueue<Chunk> PopulateChunkQueue = [];
    public static ConcurrentQueue<Chunk> GenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> RegenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> CreateQueue = [];
    public static ConcurrentBag<Vector3i> IgnoreList = [];

    private static readonly object _chunkLock = new object();

    public static bool TryAddActiveChunk(Chunk chunk)
    {
        if (!ActiveChunks.TryAdd(chunk.GetWorldPosition(), chunk))
            return false;

        foreach (var pos in chunk.GetSideChunkPositions())
        {
            if (ActiveChunks.TryGetValue(pos, out var sideChunk))
            {
                lock (_chunkLock)
                {
                    chunk.AddChunk(sideChunk);
                }
            }
        }
        return true;
    }

    

    public static void Clear()
    {
        ActiveChunks.Clear();
        GenerateChunkQueue.Clear();
        PopulateChunkQueue.Clear();
        GenerateMeshQueue.Clear();
        RegenerateMeshQueue.Clear();
        CreateQueue.Clear();
        IgnoreList.Clear();
    }
}