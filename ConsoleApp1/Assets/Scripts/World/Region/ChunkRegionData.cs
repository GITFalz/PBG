using System.Collections.Concurrent;
using OpenTK.Mathematics;

public class ChunkRegionData
{
    public Vector3i Position;
    public ConcurrentDictionary<Vector3i, ChunkData> Chunks;
    public string FilePath;
    
    public ChunkRegionData(Vector3i position)
    {
        Position = position;
        Chunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        
        FilePath = Path.Combine(Game.chunkPath, $"Region_{position.X}_{position.Y}_{position.Z}");
        
        using FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        {
            using BinaryWriter br = new BinaryWriter(fs);
            {
                br.Write((short)0);
                
                for (int i = 0; i < 4096; i++)
                {
                    short size = -1;
                    br.Write(size);
                }
            }
        }
    }

    public void SaveChunk(Vector3i relativePosition, ChunkData chunkData)
    {
        int index = relativePosition.X + relativePosition.Y * 256 + relativePosition.Z * 16;
        
        short chunkCount = 0;
        
        using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (BinaryReader reader = new BinaryReader(fs))
            {
                chunkCount = reader.ReadInt16();
                fs.Seek(2 + index * 2, SeekOrigin.Begin);
            }
        }
        
        using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            if (chunkData.Save)
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write((short)(chunkCount+1));
                    fs.Seek((short)(2 + index * 2), SeekOrigin.Begin);
                    writer.Write(chunkCount);
                }
            }
        }
    }
}