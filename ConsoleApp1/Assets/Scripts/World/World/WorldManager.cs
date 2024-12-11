using System.Collections.Concurrent;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Engine.Scripts.Core.Voxel;
using OpenTK.Mathematics;

public class WorldManager
{
    public HashSet<Vector3> chunks;
    
    public ConcurrentDictionary<Vector3i, ChunkData> activeChunks;
    public ConcurrentQueue<Vector3i> chunksToGenerate;
    public ConcurrentQueue<ChunkData> chunksToCreate;
    public ConcurrentQueue<ChunkData> chunksToStore;

    private ConcurrentBag<Vector3i> chunksToIgnore;
    
    private Task? currentTask = null;
    private Task? storeTask = null;
    
    private Vector3i playerPosition = new Vector3i(0, 0, 0);
    
    public WorldManager()
    {
        chunks = new HashSet<Vector3>();
        
        activeChunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        chunksToGenerate = new ConcurrentQueue<Vector3i>();
        chunksToCreate = new ConcurrentQueue<ChunkData>();
        chunksToStore = new ConcurrentQueue<ChunkData>();
        chunksToIgnore = new ConcurrentBag<Vector3i>();
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

    public void CheckRenderDistance()
    {
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(playerPosition.X / 32, playerPosition.Y / 32, playerPosition.Z / 32);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in activeChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        chunks.Clear();
        
        for (int x = -render; x < render; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = -render; z < render; z++)
                {
                    Vector3i position = new Vector3i(playerChunk.X + x, y, playerChunk.Z + z) * 32;
                    chunksToRemove.Remove(position);
                    
                    chunks.Add(position);
                    
                    if (!activeChunks.ContainsKey(position) && !chunksToGenerate.Contains(position) && chunksToCreate.All(c => c.position != position))
                    {
                        chunksToGenerate.Enqueue(position);
                    }
                }
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

        if (!activeChunks.TryGetValue(chunkPosition, out var chunk)) 
            return chunks.Contains(chunkPosition) ? 2 : 1;
        
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
                if (!chunkData.FolderExists())
                {
                    Chunk.GenerateChunk(ref chunkData, position);
                    chunksToStore.Enqueue(chunkData);
                }
                else
                    chunkData.LoadData();
                
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