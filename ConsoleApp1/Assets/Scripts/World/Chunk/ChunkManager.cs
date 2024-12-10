using System.Collections.Concurrent;
using System.Numerics;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

public class ChunkManager
{
    public ConcurrentDictionary<Vector3i, ChunkData> chunks;
    public ConcurrentQueue<ChunkData> toBeCreated;
    
    public ChunkManager()
    {
        chunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        toBeCreated = new ConcurrentQueue<ChunkData>();
    }
    
    public void GenerateChunk(Vector3i position)
    {
        /*
        if (chunks.ContainsKey(position)) return;

        ChunkData chunkData = new ChunkData(position);
        chunkData.meshData = new MeshData();
        
        if (!chunkData.FileExists())
            Chunk.GenerateChunk(ref chunkData, position);
        else
            chunkData.LoadChunk();


        Chunk.GenerateOcclusion(chunkData.blocks);
        Chunk.GenerateMesh(chunkData);

        toBeCreated.Enqueue(chunkData);
        */
    }
    
    public void CreateChunks()
    {
        while (toBeCreated.TryDequeue(out var data))
        {
            if (!chunks.TryAdd(data.position, data))
                toBeCreated.Enqueue(data);
            
            data.CreateChunk();
            data.StoreData();
        }
    }
    
    public void RenderChunks()
    {
        foreach (var chunk in chunks)
        {
            chunk.Value.RenderChunk();
        }
    }

    public void Delete()
    {
        foreach (var chunk in chunks)
        {
            chunk.Value.Delete();
        }
    }
}