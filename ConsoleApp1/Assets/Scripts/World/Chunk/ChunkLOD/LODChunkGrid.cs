using OpenTK.Mathematics;

public class LODChunkGrid : LODChunkBase
{
    public LODChunkBase[] Chunks = new LODChunkBase[8];

    public LODChunkGrid(Vector3i position, int size, int resolution) : base(position, size, resolution) { Init(); }

    public void Init()
    {
        int scale = Scale * 32;
        int index = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Chunks[index] = new LODChunk(Position + new Vector3i(x * scale, y * scale, z * scale), scale, Resolution);
                    index++;
                }
            }
        }
    }

    public void UpdateResolution(Vector3i position)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            LODChunkBase child = Chunks[i];
            int res = child.GetResolution(position);
            res = res > Resolution ? Resolution : res;
            if (res < Resolution)
            {
                if (child is LODChunk)
                {
                    LODChunkGrid grid = new LODChunkGrid(child.Position, child.Size, Resolution - 1);
                    grid.UpdateResolution(position);
                    Chunks[i] = grid;
                }
            }
            else
            {
                if (child is LODChunkGrid grid)
                {
                    grid.Clear();
                    Chunks[i] = new LODChunk(child.Position, child.Size, Resolution);
                }
            }
        }
    }

    public override void RenderChunk(int modelLocation)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].RenderChunk(modelLocation);
        }
    }

    public override void RenderChunkTransparent()
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].RenderChunkTransparent();
        }
    }

    public override void Clear()
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].Clear();
        }
        Array.Clear(Chunks, 0, Chunks.Length);
    }

    public override void GenerateInfo()
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].GenerateInfo();
        }
    }

    public override void GenerateChunk()
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].GenerateChunk();
        }
    }
    
    public override void GetChunks(List<LODChunk> chunks)
    {
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i].GetChunks(chunks);
        }
    }
}