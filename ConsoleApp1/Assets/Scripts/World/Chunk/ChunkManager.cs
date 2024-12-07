using OpenTK.Mathematics;

namespace ConsoleApp1.Assets.Scripts.World.Chunk;

public class ChunkManager
{
    public Dictionary<Vector3, ChunkData> chunks;
    
    public ChunkManager()
    {
        chunks = new Dictionary<Vector3, ChunkData>();
    }
    
    public void GenerateChunk(Vector3i position)
    {
        if (chunks.ContainsKey(position)) return;

        ChunkData chunkData = new ChunkData(position);
        Chunk.GenerateChunk(ref chunkData, position);
        
        chunkData.CreateChunk();
        
        chunks.Add(position, chunkData);
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