using System.Collections.Concurrent;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using OpenTK.Mathematics;

public class WorldManager
{
    public ConcurrentDictionary<Vector3i, ChunkData> activeChunks;
    public ConcurrentQueue<Vector3i> chunksToGenerate;
    public ConcurrentQueue<ChunkData> chunksToCreate;
    public ConcurrentQueue<ChunkData> chunksToStore;
    
    private Task? currentTask = null;
    private Task? storeTask = null;
    
    private Vector3i playerPosition = new Vector3i(0, 0, 0);
    
    public WorldManager()
    {
        activeChunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        chunksToGenerate = new ConcurrentQueue<Vector3i>();
        chunksToCreate = new ConcurrentQueue<ChunkData>();
        chunksToStore = new ConcurrentQueue<ChunkData>();
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
        Console.WriteLine("Rendering chunk: " + activeChunks.Count);
        
        foreach (var chunk in activeChunks)
        {
            chunk.Value.RenderChunk();
        }
    }

    public void CheckRenderDistance()
    {
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(playerPosition.X / 32, playerPosition.Y / 32, playerPosition.Z / 32);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in activeChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        for (int x = -render; x < render; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = -render; z < render; z++)
                {
                    Vector3i position = new Vector3i(playerChunk.X + x, y, playerChunk.Z + z) * 32;
                    chunksToRemove.Remove(position);
                    
                    if (!activeChunks.ContainsKey(position) && !chunksToGenerate.Contains(position) && chunksToCreate.All(c => c.position != position))
                    {
                        chunksToGenerate.Enqueue(position);
                    }
                }
            }
        }
        
        foreach (var chunk in chunksToRemove)
        {
            if (activeChunks.TryRemove(chunk, out var data))
            {
                chunksToStore.Enqueue(data);
                data.Clear();
            }
        }
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
            if (activeChunks.ContainsKey(position)) return;

            ChunkData chunkData = new ChunkData(position);
            chunkData.meshData = new MeshData();

            await Task.Run(() =>
            {
                Chunk.GenerateChunk(ref chunkData, position);
                
                
                /*
                if (!chunkData.FileExists())
                    Chunk.GenerateChunk(ref chunkData, position);
                else
                    chunkData.LoadChunk();
                    */
                
                Block?[] blocks = chunkData.blockStorage.GetFullBlockArray();
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
}