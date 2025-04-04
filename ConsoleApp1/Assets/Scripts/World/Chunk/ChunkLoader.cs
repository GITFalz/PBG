using System.Collections.Concurrent;
using OpenTK.Mathematics;

public static class ChunkLoader
{
    public static readonly ConcurrentDictionary<string, object> fileLocks = [];

    public static bool IsChunkStored(Chunk chunkData)
    {
        Vector3 regionPos = ChunkGenerator.RegionPosition(chunkData.GetRelativePosition());
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        if (!File.Exists(filePath))
            return false;

        int chunkIndex = ChunkGenerator.ChunkIndex(chunkData.GetRelativePosition());

        FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader reader = new BinaryReader(fileStream);

        fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);

        int index = reader.ReadUInt16();
        return index != ushort.MaxValue;
    }

    public static bool LoadChunk(Chunk chunkData)
    {
        Vector3 regionPos = ChunkGenerator.RegionPosition(chunkData.GetRelativePosition());
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        if (!File.Exists(filePath))
            return false;

        int chunkIndex = ChunkGenerator.ChunkIndex(chunkData.GetRelativePosition());

        if (fileLocks.TryGetValue(filePath, out var fileLock))
        {
            lock (fileLock)
            {
                return Load(chunkData, chunkIndex, filePath);
            }
        }
        else
        {
            return Load(chunkData, chunkIndex, filePath);
        }
    }

    private static bool Load(Chunk chunkData, int chunkIndex, string filePath)
    {
        FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader reader = new BinaryReader(fileStream);

        fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);

        int index = reader.ReadUInt16();
        if (index == ushort.MaxValue)
            return false;

        int offset = 8194 + index * 32768 * 4; // Header size + chunk index * block count * 4 bytes
        fileStream.Seek(offset, SeekOrigin.Begin);

        for (int i = 0; i < 8; i++)
        {
            Block[] blocks = new Block[4096];
            int blockCount = 0;
            for (int j = 0; j < 4096; j++)
            {
                int data = reader.ReadInt32();
                if (data == 0)
                {
                    blocks[j] = new Block(false, 0);
                    continue;
                }

                blocks[j] = new Block(true, data);
                blockCount++;
            }

            if (blockCount == 0)
            {
                chunkData.blockStorage.Blocks[i] = null;
            }
            else
            {
                chunkData.blockStorage.Blocks[i] = blocks;
            }
                
        }
    
        return true;
    }
}