using OpenTK.Mathematics;

public static class ChunkSaver
{
    /*
    * File format:
    * 2 bytes: chunk count saved in the file
    * 8192 bytes: header => 2 bytes: chunk position in the file
    * total header size: 2 + 8192 = 8194 bytes
    *
    * a lot of bytes: chunk data
    */
    
    public static void SaveChunk(Chunk chunk)
    {
        Vector3 regionPos = ChunkGenerator.RegionPosition(chunk.GetRelativePosition());

        Console.WriteLine($"Region: {regionPos} Chunk: {chunk.GetRelativePosition()}");

        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        ChunkLoader.fileLocks.GetOrAdd(filePath, new object());

        lock (ChunkLoader.fileLocks[filePath])
        {
            InitializeFile(filePath);

            int chunkIndex = ChunkGenerator.ChunkIndex(chunk.GetRelativePosition());
            
            using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
            using (BinaryReader reader = new BinaryReader(fileStream))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                short chunkCount = reader.ReadInt16();

                fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);
                ushort chunkData = reader.ReadUInt16();

                if (chunkCount == -1)
                    return;

                int index = chunkData;

                if (chunkData == ushort.MaxValue)
                {
                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write((short)(chunkCount + 1));

                    fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);
                    writer.Write((ushort)chunkCount);
                    index = chunkCount;
                }

                int offset = 8194 + index * 32768 * 4; // Header size + chunk index * block count * 4 bytes
                fileStream.Seek(offset, SeekOrigin.Begin);

                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            Block block = chunk.blockStorage.GetBlock(x, y, z);
                            writer.Write(block.blockData);
                        }
                    }
                }
            }
        }
    }

    public static void SaveChunkAsAnalysed(Chunk chunk)
    {
        Vector3 regionPos = ChunkGenerator.RegionPosition(chunk.GetRelativePosition());

        Console.WriteLine($"Region: {regionPos} Chunk: {chunk.GetRelativePosition()}");

        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        ChunkLoader.fileLocks.GetOrAdd(filePath, new object());

        lock (ChunkLoader.fileLocks[filePath])
        {
            InitializeFile(filePath);

            int chunkIndex = ChunkGenerator.ChunkIndex(chunk.GetRelativePosition());
            
            using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
            using (BinaryReader reader = new BinaryReader(fileStream))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                short chunkCount = reader.ReadInt16();

                fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);
                ushort chunkData = reader.ReadUInt16();

                if (chunkCount == -1)
                    return;

                int index = chunkData;

                if (chunkData == ushort.MaxValue)
                {
                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write((short)(chunkCount + 1));

                    fileStream.Seek(2 + chunkIndex * 2, SeekOrigin.Begin);
                    writer.Write((ushort)chunkCount);
                    index = chunkCount;
                }

                int offset = 8194 + index * 32768 * 4; // Header size + chunk index * block count * 4 bytes
                fileStream.Seek(offset, SeekOrigin.Begin);

                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            Block block = chunk.blockStorage.GetBlock(x, y, z);
                            writer.Write(block.blockData);
                        }
                    }
                }
            }
        }
    }

    private static void InitializeFile(string filePath)
    {
        if (File.Exists(filePath))
            return;

        using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create));
        writer.Write((short)0);

        for (int i = 0; i < 4096; i++)
        {
            writer.Write(ushort.MaxValue);
        }
    }
}