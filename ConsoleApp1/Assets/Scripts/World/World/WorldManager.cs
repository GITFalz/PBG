using System.Collections.Concurrent;
using OpenTK.Mathematics;

public class WorldManager
{
    public static WorldManager? Instance;
    
    public HashSet<Vector3i> chunks;
    
    public ConcurrentDictionary<Vector3i, ChunkData> activeChunks;
    public ConcurrentQueue<Vector3i> chunksToGenerate;
    public ConcurrentQueue<ChunkData> chunksToCreate;
    public ConcurrentQueue<ChunkData> chunksToStore;

    private ConcurrentBag<Vector3i> chunksToIgnore;
    
    private Task? currentTask = null;
    private Task? storeTask = null;
    
    private Vector3i playerPosition = new Vector3i(0, 0, 0);
    
    public ChunkRegionData regionData;
    
    public WorldManager()
    {
        Instance ??= this;
        
        chunks = new HashSet<Vector3i>();
        
        activeChunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        chunksToGenerate = new ConcurrentQueue<Vector3i>();
        chunksToCreate = new ConcurrentQueue<ChunkData>();
        chunksToStore = new ConcurrentQueue<ChunkData>();
        chunksToIgnore = new ConcurrentBag<Vector3i>();
        
        regionData = new ChunkRegionData(new Vector3i(0, 0, 0));
        regionData.SaveChunk(new Vector3i(0, 1, 0), new ChunkData(new Vector3i(0, 1, 0)));
        regionData.SaveChunk(new Vector3i(1, 1, 0), new ChunkData(new Vector3i(1, 1, 0)));
    }
    
    public void Start()
    {
        SetChunks();
    }
    
    public void Update()
    {
        CheckRenderDistance();
        GenerateChunks();
        StoreChunks();
        
        if (chunksToCreate.TryDequeue(out var chunk))
        {
            if (activeChunks.TryAdd(chunk.position, chunk))
            {
                chunk.CreateChunk();
            }
            else
            {
                chunksToGenerate.Enqueue(chunk.position);
                chunk.Clear();
            }
        }
    }
    
    public void SetPlayerPosition(Vector3 position)
    {
        playerPosition = new Vector3i((int)position.X, (int)position.Y, (int)position.Z);
    }

    public void Render()
    {
        //Console.WriteLine("Rendering chunk: " + activeChunks.Count);
        
        foreach (var chunk in activeChunks)
        {
            chunk.Value.RenderChunk();
        }
    }

    public void SetChunks()
    {
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunkPositions = new HashSet<Vector3i>();
        
        for (int x = -render; x < render; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = -render; z < render; z++)
                {
                    Vector3i position = new Vector3i(playerChunk.X + x, y, playerChunk.Z + z) * 32;
                    chunkPositions.Add(position);
                }
            }
        }
        
        chunks = chunkPositions;
    }

    public void CheckRenderDistance()
    {
        SetChunks();
        
        int render = World.renderDistance;
        //Vector3i playerChunk = new Vector3i(playerPosition.X / 32, playerPosition.Y / 32, playerPosition.Z / 32);
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in activeChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var chunk in chunks)
        {
            chunksToRemove.Remove(chunk);
            
            if (!activeChunks.ContainsKey(chunk) && !chunksToGenerate.Contains(chunk) && chunksToCreate.All(c => c.position != chunk))
            {
                chunksToGenerate.Enqueue(chunk);
            }
        }
        
        chunksToIgnore.Clear();
        
        foreach (var chunk in chunksToRemove)
        {
            chunksToIgnore.Add(chunk);
            
            if (activeChunks.TryRemove(chunk, out var data))
            {
                data.Clear();
            }
        }
    }

    public int GetBlock(Vector3i blockPosition, out Block? block)
    {
        block = null;
        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!activeChunks.TryGetValue(chunkPosition, out var chunk)) return chunks.Contains(chunkPosition) ? 2 : 1;
        
        block = chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition));
        return block == null ? 1 : 0;
    }

    private async void GenerateChunks()
    {
        if (currentTask == null)
        {
            currentTask = GenerateChunk();
            await currentTask;
            currentTask = null;
        }
    }

    private async Task GenerateChunk()
    {
        if (chunksToGenerate.TryDequeue(out var position))
        {
            if (activeChunks.ContainsKey(position) || chunksToIgnore.Contains(position)) return;

            ChunkData chunkData = new ChunkData(position);
            chunkData.meshData = new MeshData();

            await Task.Run(() =>
            {
                Block?[] blocks;
                
                /*
                if (!chunkData.FolderExists())
                {
                    
                    chunksToStore.Enqueue(chunkData);
                }
                else
                {
                    chunkData.LoadData();
                    blocks = chunkData.blockStorage.GetFullBlockArray();
                }
                */
                
                Chunk.GenerateFlatChunk(ref chunkData, position);
                Chunk.GenerateBox(ref chunkData, position, new Vector3i(20, 10, 20), new Vector3i(10, 10, 10));
                
                blocks = chunkData.blockStorage.GetFullBlockArray();
                
                Chunk.GenerateOcclusion(blocks);
                Chunk.GenerateMesh(blocks, chunkData);
                    
                chunksToCreate.Enqueue(chunkData);
            });
        }
    }
    
    private async void StoreChunks()
    {
        if (storeTask == null)
        {
            storeTask = StoreChunk();
            await storeTask;
            storeTask = null;
        }
    }
    
    private async Task StoreChunk()
    {
        if (chunksToStore.TryDequeue(out var chunk))
        {
            await Task.Run(() =>
            {
                chunk.StoreData();
            });
        }
    }
    
    public static bool IsBlockChecks(Vector3i[] positions)
    {
        Block? block;

        foreach (var position in positions)
        {
            int result = Instance.GetBlock(position, out block);

            if (result == 0 || result == 2)
                return true;
        }
        
        return false;
    }
    
    public static bool IsBlockChecks(Vector3i position)
    {
        Block? block;
        
        int result = Instance.GetBlock(position, out block);

        if (result == 0 || result == 2)
            return true;
            
        return false;
    }
}