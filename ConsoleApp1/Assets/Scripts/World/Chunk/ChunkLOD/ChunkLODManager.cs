using OpenTK.Mathematics;

public static class ChunkLODManager
{
    public static LODChunkGrid[] LODChunks = [];

    public static void Initialize(int x, int y, int z, int startX, int startY, int startZ, int resolution)
    {
        int scale = (int)Mathf.Pow(2, resolution) * 32;
        LODChunks = new LODChunkGrid[x * y * z];
        int index = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Vector3i position = new Vector3i(startX + i, startY + j, startZ + k) * scale;
                    LODChunkGrid newGrid = new LODChunkGrid(position, scale, resolution - 1);
                    LODChunks[index] = newGrid;
                    index++;
                }
            }
        }
    }

    public static void CheckChunkResolution(Vector3i position)
    {
        Info.ClearBlocks();
        for (int i = 0; i < LODChunks.Length; i++)
        {
            LODChunkGrid chunk = LODChunks[i];
            chunk.UpdateResolution(position);
            chunk.GenerateInfo();
        }
        Info.UpdateBlocks();
    }
}