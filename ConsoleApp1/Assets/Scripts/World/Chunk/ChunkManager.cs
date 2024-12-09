using System.Collections.Concurrent;
using System.Numerics;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace ConsoleApp1.Assets.Scripts.World.Chunk;

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
        if (chunks.ContainsKey(position)) return;

        ChunkData chunkData = new ChunkData(position);
        Chunk.GenerateChunk(ref chunkData, position);
        
        toBeCreated.Enqueue(chunkData);
    }
    
    public void CreateChunks()
    {
        while (toBeCreated.TryDequeue(out var data))
        {
            if (!chunks.TryAdd(data.position, data))
                toBeCreated.Enqueue(data);
            
            data.CreateChunk();
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
    
    public static bool IsAABBVisible(Bounds bounds, Plane[] frustumPlanes)
    {
        foreach (var plane in frustumPlanes)
        {
            // Find the most positive vertex relative to the plane normal
            Vector3 positiveVertex = new Vector3(
                (plane.Normal.X >= 0) ? bounds.Max.X : bounds.Min.X,
                (plane.Normal.Y >= 0) ? bounds.Max.Y : bounds.Min.Y,
                (plane.Normal.Z >= 0) ? bounds.Max.Z : bounds.Min.Z
            );

            // Check if the positive vertex is outside the plane
            if (System.Numerics.Vector3.Dot(plane.Normal, Mathf.ToNumericsVector3(positiveVertex)) + plane.D < 0)
            {
                return false; // Completely outside, not visible
            }
        }

        return true; // Visible (intersects or inside)
    }
}