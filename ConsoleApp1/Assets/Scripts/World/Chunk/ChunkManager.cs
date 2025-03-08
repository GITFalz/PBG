using OpenTK.Mathematics;

public static class ChunkManager
{
    /*
    * File format:
    * 2 bytes: chunk count saved in the file
    * 8192 bytes: header => 16 bits: chunk position in the file
    * total header size: 2 + 8192 = 8194 bytes
    *
    * a lot of bytes: chunk data
    */

    public static void SaveChunk(ChunkData chunk)
    {
        Vector3 regionPos = RegionPosition(chunk.position);
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        InitializeFile(filePath);

        int chunkIndex = ChunkIndex(chunk.position);
        
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

            foreach (var storage in chunk.blockStorage.Blocks)
            {
                if (storage == null)
                {
                    for (int i = 0; i < 4096; i++)
                        writer.Write(0);
                }
                else
                {
                    foreach (var block in storage)
                        writer.Write(block.blockData);
                }
            }
        }
    }

    public static bool LoadChunk(ChunkData chunkData)
    {
        Vector3 regionPos = RegionPosition(chunkData.position);
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        if (!File.Exists(filePath))
            return false;

        int chunkIndex = ChunkIndex(chunkData.position);

        FileStream fileStream = File.Open(filePath, FileMode.Open);
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

            Console.WriteLine($"Region: {regionPos}, Chunk: {chunkData.position}, SubChunk: {chunkData.blockStorage.SubPositions[i]}, BlockCount: {blockCount}");

            if (blockCount == 0)
            {
                chunkData.blockStorage.Blocks[i] = null;
                chunkData.blockStorage.BlockCount[i] = 0;
            }
            else
            {
                chunkData.blockStorage.Blocks[i] = blocks;
                chunkData.blockStorage.BlockCount[i] = blockCount;
            }
                
        }
    
        return true;
    }

    private static void InitializeFile(string filePath)
    {
        if (File.Exists(filePath))
            return;

        using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

        // Write chunk count
        writer.Write((short)0);

        // Write header 16 x 16 x 16 = 4096 chunks
        for (int i = 0; i < 4096; i++)
        {
            writer.Write(ushort.MaxValue);
        }
    }

    /*
    public void StoreData()
    {
        int i = 0;
        
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        
        foreach (var subPos in blockStorage.SubPositions)
        {
            string subFilePath = Path.Combine(filePath, $"SubChunk_{subPos.X}_{subPos.Y}_{subPos.Z}.chunk");
            
            Block[]? blocks = blockStorage.Blocks[i];
            
            if (blockStorage.BlockCount[i] == 0 || blocks == null)
                SaveEmptyChunk(subFilePath);
            else
                SaveChunk(blocks, subFilePath);
            
            i++;
        }
    }
    
    public void SaveChunk(Block[] data, string filePath)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            foreach (Block block in data)
            {
                if (block.IsAir())
                    writer.Write((short)-1);
                else
                    writer.Write(block.blockData);
            }
        }
    }
    
    public void SaveEmptyChunk(string filePath)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(-2);
        }
    }
    
    public bool FolderExists()
    {
        return Directory.Exists(filePath);
    }
    
    public void LoadData()
    {
        int i = 0;
        
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        
        foreach (var subPos in blockStorage.SubPositions)
        {
            string subFilePath = Path.Combine(filePath, $"SubChunk_{subPos.X}_{subPos.Y}_{subPos.Z}.chunk");
            blockStorage.Blocks[i] = LoadChunk(subFilePath, out int count);
            blockStorage.BlockCount[i] = count;
            
            i++;
        }
    }

    public Block[]? LoadChunk(string subFilePath, out int count)
    {
        count = 0;
        
        using (BinaryReader reader = new BinaryReader(File.Open(subFilePath, FileMode.Open)))
        {
            short value = reader.ReadInt16();
            if (value == -2)
                return null;
            
            Block[] blocks = new Block[4096];
            blocks[0] = value == -1 ? new Block(false, 0) : new Block(true, value);
            
            for (int i = 1; i < 4096; i++)
            {
                short data = reader.ReadInt16();
                if (data == -1)
                    blocks[i] = new Block(false, 0);
                else
                {
                    blocks[i] = new Block(true, data);
                    count++;
                }
            }
            
            return blocks;
        }
    }
    */

    private static Vector3i RegionPosition(Vector3i position)
    {
        return (position.X >> 4, position.Y >> 4, position.Z >> 4);
    }
    
    private static Vector3i ChunkPosition(Vector3i position)
    {
        return (position.X & 15, position.Y & 15, position.Z & 15);
    }

    private static int ChunkIndex(Vector3i position)
    {
        position = ChunkPosition(position);
        return position.X + (position.Y << 4) + (position.Z << 8);
    }
}