using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkManager
{
    /// <summary>
    /// All the chunks in the world
    /// </summary>
    public static ConcurrentDictionary<Vector3i, ChunkEntry> ActiveChunks = [];
    public static HashSet<Vector3i> Chunks = [];
    public static List<Vector3i> ChunkProcessDirection = [];

    public static Dictionary<Vector3i, Chunk> OpaqueChunks = new Dictionary<Vector3i, Chunk>();
    public static Dictionary<Vector3i, Chunk> TransparentChunks = new Dictionary<Vector3i, Chunk>();

    private static Vector3i _lastPlayerPosition = new Vector3i(0, 0, 0);
    private static Vector3i _currentPlayerChunk = new Vector3i(0, 0, 0);

    private static double _renderDistanceTimer = 0;
    private static int _oldRenderedChunks = 0;

    public static void HandleRenderDistance()
    {
        _currentPlayerChunk = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(Mathf.FloorToInt(PlayerData.Position)));
        _currentPlayerChunk.Y = 0;

        if (_currentPlayerChunk == _lastPlayerPosition) 
            return;

        CheckChunks();
        UpdateChunkNeighbours();

        _lastPlayerPosition = _currentPlayerChunk;
    }

    public static void CheckChunks()
    {
        Chunks = SetChunks();
        foreach (var key in ActiveChunks.Keys)
        {
            if (Chunks.Contains(key))
                continue;

            RemoveChunk(key);
        }
        foreach (var chunk in Chunks)
        {
            if (ActiveChunks.ContainsKey(chunk))
                continue;

            AddChunk(chunk);
        }

        _renderDistanceTimer = 5;
    }

    public static void CheckFrustum()
    {
        Camera camera = Game.Camera;

        Info.ChunkCount = 0;
        Info.VertexCount = 0;

        foreach (var chunkEntry in ActiveChunks.Values)
        {
            var chunk = chunkEntry.Chunk;

            bool frustum = camera.FrustumIntersects(chunk.boundingBox);
            bool baseDisabled = !chunkEntry.ShouldRender() || !frustum || chunk.BlockRendering;// || Vector2.Distance(PlayerData.Position.Xz, chunk.GetWorldPosition().Xz) > World.renderDistance * 32;

            bool isDisabled = chunk.IsDisabled;
            chunk.IsDisabled = baseDisabled || !chunk.HasBlocks;

            if (!isDisabled && chunk.IsDisabled)
            {
                OpaqueChunks.Remove(chunk.GetWorldPosition());
            }
            else if (isDisabled && !chunk.IsDisabled)
            {
                OpaqueChunks.TryAdd(chunk.GetWorldPosition(), chunk);
            }

            bool isTransparentDisabled = chunk.IsTransparentDisabled;
            chunk.IsTransparentDisabled = baseDisabled || !chunk.HasTransparentBlocks;

            if (!isTransparentDisabled && chunk.IsTransparentDisabled)
            {
                TransparentChunks.Remove(chunk.GetWorldPosition());
            }
            else if (isTransparentDisabled && !chunk.IsTransparentDisabled)
            {
                TransparentChunks.TryAdd(chunk.GetWorldPosition(), chunk);
            }

            if (chunk.IsDisabled && chunk.IsTransparentDisabled)
                continue;

            Info.ChunkCount++;
            Info.VertexCount += chunk.VertexCount;
        }

        Info.SetChunkVertexCount();
        Info.SetChunkRenderCount();
    }

    public static void GenerateNearbyPositions()
    {
        ChunkProcessDirection = [];

        Vector3i center = (0, 0, 0);
        int renderDistance = World.RenderDistance;
        int maxY = World.VerticalChunkCount;

        var result = new List<(Vector3i pos, int distanceSquared)>();

        for (int y = -1; y <= maxY; y++)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3i pos = new Vector3i(center.X + x, y, center.Z + z);

                    int dx = x;
                    int dy = center.Y - y;
                    int dz = z;

                    int distSq = dx * dx + dy * dy + dz * dz;

                    if (distSq <= renderDistance * renderDistance)
                    {
                        result.Add((pos, distSq));
                    }
                }
            }
        }

        // Sort by distance
        result.Sort((a, b) => a.distanceSquared.CompareTo(b.distanceSquared));

        // Return just the positions
        ChunkProcessDirection = result.Select(r => r.pos).ToList();
    }

    public static bool AddChunk(Vector3i position)
    {
        if (ActiveChunks.TryGetValue(position, out var _))
            return false;

        Chunk chunk = ChunkPool.GetChunk(position);
        ChunkEntry chunkEntry = new ChunkEntry(chunk) { 
            Stage = ChunkStage.ToBeGenerated,
            SetWantedStage = true
        };

        return ActiveChunks.TryAdd(position, chunkEntry);
    }

    public static void RemoveChunk(Vector3i position)
    {
        if (ActiveChunks.TryRemove(position, out var chunkEntry))
        {
            chunkEntry.FreeChunk = true;
            chunkEntry.Blocked = true;
            chunkEntry.SetWantedStage = true;
            chunkEntry.WantedStage = ChunkStage.ToBeFreed;
            chunkEntry.Chunk.HasBlocks = false;
            chunkEntry.Chunk.HasTransparentBlocks = false;
            if (chunkEntry.Process != null)
            {
                if (chunkEntry.Process.TryRemoveProcess())
                {
                    chunkEntry.Process = null;
                    chunkEntry.SetStage(ChunkStage.ToBeFreed);
                }
                else
                {
                    chunkEntry.Process.SetOnCompleteAction(() => { FreeChunk(chunkEntry); Console.WriteLine("Chunk freed after process: " + chunkEntry.Chunk.GetRelativePosition()); });
                }
            }
            else
            {
                FreeChunk(chunkEntry);
            }
        }
    }

    public static void FreeChunk(ChunkEntry chunkEntry)
    {
        ActiveChunks.TryRemove(chunkEntry.Chunk.GetRelativePosition(), out _);
        
        OpaqueChunks.Remove(chunkEntry.Chunk.GetWorldPosition());
        TransparentChunks.Remove(chunkEntry.Chunk.GetWorldPosition());
        ChunkPool.FreeChunk(chunkEntry.Chunk);
        chunkEntry.SetStage(ChunkStage.Free);
        chunkEntry.SetWantedStage = false;
        chunkEntry.FreeChunk = false;
        chunkEntry.Blocked = false;
        chunkEntry.Chunk = Chunk.Empty;
        chunkEntry.Process = null;
    }

    private static readonly Vector2i[] _moves = [(1, 0), (0, 1), (-1, 0), (0, -1)];
    private static HashSet<Vector3i> SetChunks()
    {
        Vector2i startPosition = (-1, -1);
        HashSet<Vector3i> chunkPositions = [];
        
        for (int x = 1; x < World.RenderDistance*2+1; x+=2)
        {
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < x; i++)
                {
                    startPosition += _moves[k];
                    for (int y = -1; y <= World.VerticalChunkCount; y++)
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
    /// Loops trough all the chunk entries and advances their stages.
    /// </summary>
    public static void Update()
    {
        for (int i = 0; i < ChunkProcessDirection.Count; i++)
        {
            var position = ChunkProcessDirection[i] + _currentPlayerChunk;
            if (ActiveChunks.TryGetValue(position, out var chunkEntry) && chunkEntry.IsReady())
            {
                ChunkStageHandler.ExecuteStage(chunkEntry);
            }
        }
    }

    public static void Reload()
    {
        var chunks = SetChunks();
        foreach (var (_, chunkEntry) in ActiveChunks)
        {
            if (!chunks.Contains(chunkEntry.Chunk.GetRelativePosition()))
                Console.WriteLine("Chunk not found: " + chunkEntry.Chunk.GetRelativePosition());
                
            chunkEntry.SetStage(ChunkStage.ToBeRendered);
            chunkEntry.SetWantedStage = false;
        }
    }

    public static bool GetChunk(Vector3i position, [NotNullWhen(true)] out ChunkEntry? entry)
    {
        entry = null;
        if (!ActiveChunks.TryGetValue(position, out var chunkEntry) || chunkEntry.Stage == ChunkStage.Free)
            return false;

        entry = chunkEntry;
        return true;
    }

    public static bool GetChunk(Vector3i position, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
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

    public static void FreeChunks()
    {
        foreach (var (_, chunkEntry) in ActiveChunks)
        {
            FreeChunk(chunkEntry);
        }
        Clear();
    }
    
    public static void Clear()
    {
        ActiveChunks = [];
        Chunks = [];
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
    public Stopwatch generationTime = new Stopwatch();
    public object Lock = new object();
    public double WaitTimer = 0;

    public ChunkStage WantedStage = ChunkStage.Empty;
    public bool SetWantedStage = false;
    
    public bool FreeChunk = false;
    public bool Blocked = false;

    public ThreadProcess? Process = null;

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

    public void TrySetStage(ChunkStage stage)
    {
        lock (Lock)
        {
            Stage = SetWantedStage ? WantedStage : stage;
        }
    }

    public void SetTimer(double timer)
    {
        lock (Lock)
        {
            WaitTimer = timer;
        }
    }

    public bool IsReady()
    {
        lock (Lock)
        {
            if (WaitTimer <= 0)
                return true;

            WaitTimer -= GameTime.DeltaTime;
            return false;
        }
    }

    public bool CheckDelete()
    {
        lock (Lock)
        {
            if (FreeChunk) 
            {
                Console.WriteLine("Chunk freed after delete: " + Chunk.GetRelativePosition());
                SetStage(ChunkStage.ToBeFreed);
                Process = null;
            }
        }
        return FreeChunk;
    }

    public bool ShouldRender()
    {
        lock (Lock)
        {
            return Stage >= ChunkStage.ToBeRendered && Stage <= ChunkStage.Created;
        }
    }
}