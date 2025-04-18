using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkManager
{
    /// <summary>
    /// All the chunks in the world
    /// </summary>
    public static ConcurrentDictionary<Vector3i, Chunk> ActiveChunks = [];
    public static ConcurrentQueue<Chunk> GenerateChunkQueue = [];
    public static ConcurrentQueue<Chunk> PopulateChunkQueue = [];
    public static ConcurrentQueue<Chunk> GenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> RegenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> CreateQueue = [];
    public static ConcurrentBag<Vector3i> IgnoreList = [];

    private static readonly object _chunkLock = new object();

    // Make sure the position is in world coordinates
    public static bool TryAddActiveChunk(Vector3i position, out Chunk newChunk)
    {
        newChunk = Chunk.Empty;
        if (ActiveChunks.ContainsKey(position))
            return false;

        newChunk = ChunkPool.GetChunk(position / 32);
        ChunkNeighbourCheck(newChunk);
        ActiveChunks.TryAdd(position, newChunk);
        return true;
    }

    public static bool RemoveChunk(Vector3i position, out Chunk chunk)
    {
        chunk = Chunk.Empty;
        if (!ActiveChunks.TryRemove(position, out var c))
            return false;

        chunk = c;
        ChunkPool.FreeChunk(chunk);
        return true;
    }

    public static void ReloadChunks()
    {
        RegenerateMeshQueue.Clear();

        foreach (var chunk in ActiveChunks.Values)
        {
            if (chunk.Stage != ChunkStage.Rendered)
                continue;

            chunk.Reload();
            RegenerateMeshQueue.Enqueue(chunk);
        }
    }

    public static void DisplayChunkBorders()
    {
        Info.ClearBlocks();
        foreach (var chunk in ActiveChunks.Values)
        {
            Vector3 position = chunk.GetWorldPosition();
            InfoBlockData blockData = new InfoBlockData(
                position,
                (32, 32, 32),
                (0, 1, 0, 0.1f)
            );
            Info.AddBlock(blockData);
        }
        Info.UpdateBlocks();
    }

    public static void DisplayChunkBordersNotAllNeighbours()
    {
        Info.ClearBlocks();
        foreach (var chunk in ActiveChunks.Values)
        {
            if (chunk.HasAllNeighbourChunks())
                continue;

            Vector3 position = chunk.GetWorldPosition();
            InfoBlockData blockData = new InfoBlockData(
                position,
                (32, 32, 32),
                (0, 1, 0, 0.1f)
            );
            Info.AddBlock(blockData);
        }
        Info.UpdateBlocks();
    }

    public static void ChunkNeighbourChecks()
    {
        foreach (var chunk in ActiveChunks.Values)
        {
            ChunkNeighbourCheck(chunk);
        }
    }

    public static void ChunkNeighbourCheck(Chunk chunk)
    {
        lock (_chunkLock)
        {
            foreach (var pos in chunk.GetSideChunkPositions())
            {
                if (ActiveChunks.TryGetValue(pos, out var sideChunk))
                {
                    chunk.AddChunk(sideChunk);
                }
            }
        }
    }

    public static void Unload()
    {
        ChunkPool.Clear();
        Clear();
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

public struct ModelUniform
{
    public Matrix4 model;
    public int offset;
}