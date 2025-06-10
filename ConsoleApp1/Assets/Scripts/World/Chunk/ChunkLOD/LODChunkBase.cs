using OpenTK.Mathematics;

public abstract class LODChunkBase
{
    public Vector3i Position = Vector3i.Zero;
    public Vector3 Center;
    public int Size = 1;
    public int Resolution = 1;

    public LODChunkBase(Vector3i position, int size, int resolution)
    {
        Position = position;
        Size = size;
        Resolution = resolution;
        Center = Position + new Vector3(Size / 2f);
    }

    public int X => Position.X;
    public int Y => Position.Y;
    public int Z => Position.Z;

    public int GetResolution(Vector3i position)
    {
        float distance = Vector3.Distance(Center, position);
        if (distance < 128)
        {
            return 0;
        }
        else if (distance < 512)
        {
            return 1;
        }
        else if (distance < 1024)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    public int SetResolution(Vector3i position, int max = 3)
    {
        Resolution = GetResolution(position) > max ? max : GetResolution(position);
        return Resolution;
    }

    public abstract void RenderChunk(int modelLocation);
    public abstract void RenderChunkTransparent();
    public abstract void Clear();
    public abstract void GenerateInfo();
    public abstract void GenerateChunk();
}