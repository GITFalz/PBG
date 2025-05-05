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
    public static ConcurrentQueue<Chunk> WaitingToPopulateQueue = [];

    public static ConcurrentQueue<Chunk> PopulateChunkQueue = [];
    public static ConcurrentDictionary<Vector3i, Chunk> ChunksBeingPopulated = [];

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
        AddNeighbourChunk(newChunk);
        return ActiveChunks.TryAdd(position, newChunk);
    }

    public static bool GetChunk(Vector3i position, out Chunk chunk)
    {
        chunk = Chunk.Empty;
        if (!ActiveChunks.TryGetValue(position, out var c))
            return false;

        chunk = c;
        return true;
    }

    public static bool HasChunk(Chunk chunk)
    {
        return HasChunk(chunk.GetWorldPosition());
    }

    public static bool HasChunk(Vector3i position)
    {
        return ActiveChunks.ContainsKey(position);
    }

    public static bool RemoveChunk(Vector3i position, out Chunk chunk)
    {
        chunk = Chunk.Empty;
        if (!ActiveChunks.TryRemove(position, out var c))
            return false;

        chunk = c;
        RemoveNeighbourChunk(chunk);
        ChunkPool.FreeChunk(chunk);
        return true;
    }

    public static void ReloadChunks()
    {
        RegenerateMeshQueue.Clear();

        foreach (var chunk in ActiveChunks.Values)
        {
            ReloadChunk(chunk);
        }
    }

    public static void ReloadChunk(Chunk chunk)
    {
        if (chunk.Stage != ChunkStage.Rendered || PopulateChunkQueue.Contains(chunk))
            return;

        chunk.Reload();
        RegenerateMeshQueue.Enqueue(chunk);
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
            AddNeighbourChunk(chunk);
        }
    }

    public static void AddNeighbourChunk(Chunk chunk)
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

    public static void RemoveNeighbourChunk(Chunk chunk)
    {
        lock (_chunkLock)
        {
            foreach (var pos in chunk.GetSideChunkPositions())
            {
                if (ActiveChunks.TryGetValue(pos, out var sideChunk))
                {
                    chunk.RemoveChunk(sideChunk);
                }
            }
        }
    }

    /// <summary>
    /// Check if all the neighbours of a chunk have a stage supperior to the given stage.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public static bool ChunkHasAllNeighbourSupOrEqual(Chunk chunk, ChunkStage stage)
    {
        lock (_chunkLock)
        {
            foreach (var pos in chunk.GetSideChunkPositions())
            {
                if (GetChunk(pos, out var sideChunk) && sideChunk.Stage >= stage)
                    continue;

                return false;
            }
        }
        return true;
    }

    public static List<Chunk> GetChunkNeighbours(Chunk chunk)
    {
        List<Chunk> neighbours = [];
        foreach (var pos in chunk.GetSideChunkPositions())
        {
            if (GetChunk(pos, out var sideChunk))
                neighbours.Add(sideChunk);
        }
        return neighbours;
    }

    public static void Unload()
    {
        ChunkPool.Clear();
        Clear();
    }
    
    public static void Clear()
    {
        ActiveChunks = [];
        GenerateChunkQueue = [];
        PopulateChunkQueue = [];
        GenerateMeshQueue = [];
        RegenerateMeshQueue = [];
        CreateQueue = [];
        IgnoreList = [];
    }


    public static void PopulateChunk(Chunk chunk)
    {
        if (chunk.Stage == ChunkStage.Generated && chunk.AllNeighbourChunkStageSuperiorOrEqual(ChunkStage.Generated) && !IsBeingPopulated(chunk) && ChunksBeingPopulated.TryAdd(chunk.GetWorldPosition(), chunk))
        {   
            PopulateChunkQueue.Enqueue(chunk);
            chunk.Save = true;
        }    
    }

    public static bool IsBeingPopulated(Chunk chunk)
    {
        return ChunksBeingPopulated.ContainsKey(chunk.GetWorldPosition()) || PopulateChunkQueue.Contains(chunk);
    }

    public static bool RemoveBeingPopulated(Chunk chunk)
    {
        return ChunksBeingPopulated.TryRemove(chunk.GetWorldPosition(), out var c) && c == chunk;
    }

    public static void Print()
    {
        Console.WriteLine("Active Chunks: " + ActiveChunks.Count);
        Console.WriteLine("Generate Chunk Queue: " + GenerateChunkQueue.Count);
        Console.WriteLine("Waiting To Populate Queue: " + WaitingToPopulateQueue.Count);
        Console.WriteLine("Populate Chunk Queue: " + PopulateChunkQueue.Count);
        Console.WriteLine("Chunks Being Populated: " + ChunksBeingPopulated.Count);
        Console.WriteLine("Generate Mesh Queue: " + GenerateMeshQueue.Count);
        Console.WriteLine("Regenerate Mesh Queue: " + RegenerateMeshQueue.Count);
        Console.WriteLine("Create Queue: " + CreateQueue.Count);
        Console.WriteLine("Ignore List: " + IgnoreList.Count);
    }
}

public struct ModelUniform
{
    public Matrix4 model;
    public int offset;
}