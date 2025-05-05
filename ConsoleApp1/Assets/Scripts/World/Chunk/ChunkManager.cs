using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkManager
{
    /// <summary>
    /// All the chunks in the world
    /// </summary>
    public static ConcurrentDictionary<Vector3i, ChunkEntry> ActiveChunks = [];
    private static readonly object _chunkLock = new object();

    private static Vector3i _lastPlayerPosition = new Vector3i(0, 0, 0);
    private static Vector3i _currentPlayerChunk = new Vector3i(0, 0, 0);

    public static void HandleRenderDistance()
    {
        _currentPlayerChunk = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(Mathf.FloorToInt(PlayerData.Position)));
        _currentPlayerChunk.Y = 0;

        if (_currentPlayerChunk == _lastPlayerPosition) 
            return;

        var chunks = SetChunks();
        foreach (var (key, entry) in ActiveChunks)
        {
            if (!chunks.Contains(key))
            {
                RemoveChunk(key);
            }
            chunks.Remove(key);
        }
        foreach (var chunk in chunks)
        {
            AddChunk(chunk);
        }

        _lastPlayerPosition = _currentPlayerChunk;
    }

    public static void AddChunk(Vector3i position)
    {
        if (ActiveChunks.TryGetValue(position, out var chunkEntry))
        {
            chunkEntry.Stage = ChunkStage.Loading;
            return;
        }

        Chunk chunk = ChunkPool.GetChunk(position);
        chunkEntry = new ChunkEntry(chunk) { Stage = ChunkStage.Loading };
        ActiveChunks.TryAdd(position, chunkEntry);
    }

    public static void RemoveChunk(Vector3i position)
    {
        if (ActiveChunks.TryRemove(position, out var chunkEntry) && ChunkPool.FreeChunk(chunkEntry.Chunk))
        {
            chunkEntry.Chunk = Chunk.Empty;
            chunkEntry.Stage = ChunkStage.Free;
        }
    }

    private static readonly Vector2i[] _moves = [(1, 0), (0, 1), (-1, 0), (0, -1)];
    private static HashSet<Vector3i> SetChunks()
    {
        Vector2i startPosition = (-1, -1);
        HashSet<Vector3i> chunkPositions = [];
        
        for (int x = 1; x < World.renderDistance*2+1; x+=2)
        {
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < x; i++)
                {
                    startPosition += _moves[k];
                    for (int y = -1; y < World.yChunkCount; y++)
                    {
                        chunkPositions.Add(new Vector3i(startPosition.X, y, startPosition.Y) + _currentPlayerChunk);
                    }
                }   
            }

            startPosition -= (1, 1);
        }
        
        return chunkPositions;
    }

    /// <summary>
    /// Generates all the chunk entries in the world. This should only be called once at the start of the game or when changing the render distance.
    /// </summary>
    public static void GenerateChunkEntries(Vector3i playerChunkPosition)
    {
        ActiveChunks = new ConcurrentDictionary<Vector3i, ChunkEntry>();

        int renderDistance = World.renderDistance;
        int height = World.yChunkCount;

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3i chunkPosition = (x, y, z) + playerChunkPosition;
                    ActiveChunks.TryAdd(chunkPosition, new ChunkEntry(Chunk.Empty) { Stage = ChunkStage.Loading });
                }
            }
        }
    }

    /// <summary>
    /// Loops trough all the chunk entries and advances their stages.
    /// </summary>
    public static void Update()
    {

    }

    public static bool GetChunk(Vector3i position, out Chunk chunk)
    {
        chunk = Chunk.Empty;
        if (!ActiveChunks.TryGetValue(position, out var chunkEntry))
            return false;

        chunk = chunkEntry.Chunk;
        return true;
    }

    public static bool HasChunk(Chunk chunk)
    {
        return HasChunk(chunk.GetRelativePosition());
    }

    public static bool HasChunk(Vector3i position)
    {
        return ActiveChunks.ContainsKey(position);
    }

    public static void UpdateChunkNeighbours()
    {
        foreach (var (_, chunkEntry) in ActiveChunks)
        {
            chunkEntry.Chunk.UpdateNeighbours();
        }
    }

    public static void Unload()
    {
        ChunkPool.Clear();
        Clear();
    }
    
    public static void Clear()
    {
        ActiveChunks = [];
    }

    public static void Print()
    {
        Console.WriteLine("Active Chunks: " + ActiveChunks.Count);
    }
}

public class ChunkEntry
{
    public ChunkStage Stage = ChunkStage.Empty;
    public Chunk Chunk = Chunk.Empty;
    public object Lock = new object();

    public Action<ChunkEntry> UpdateAction = (entry) => { };

    public ChunkEntry(Chunk chunk)
    {
        Chunk = chunk;
    }

    public void SetStage(ChunkStage stage)
    {
        lock (Lock)
        {
            Stage = stage;
        }
    }

    public void SetUpdateAction(Action<ChunkEntry> action)
    {
        lock (Lock)
        {
            UpdateAction = action;
        }
    }
}