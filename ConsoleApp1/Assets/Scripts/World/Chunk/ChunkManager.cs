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
        Vector3 regionPos = RegionPosition(chunk.GetRelativePosition());
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        InitializeFile(filePath);

        int chunkIndex = ChunkIndex(chunk.GetRelativePosition());
        
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
        Vector3 regionPos = RegionPosition(chunkData.GetRelativePosition());
        string filePath = Path.Combine(Game.chunkPath, $"Region_{regionPos.X}_{regionPos.Y}_{regionPos.Z}.chunk");

        if (!File.Exists(filePath))
            return false;

        int chunkIndex = ChunkIndex(chunkData.GetRelativePosition());

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