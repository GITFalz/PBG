using OpenTK.Mathematics;

public abstract class LODChunkBase
{
    public Vector3i Position = Vector3i.Zero;
    public Vector3 Center;
    public int Size = 1;
    public int Resolution = 1;
    public int Scale; 

    public LODChunkBase(Vector3i position, int size, int resolution)
    {
        Position = position;
        Size = size;
        Resolution = resolution;
        Center = Position + new Vector3(Size / 2f);
        Scale = (int)Mathf.Pow(2, resolution);
    }

    public int X => Position.X;
    public int Y => Position.Y;
    public int Z => Position.Z;

    public int GetResolution(Vector3i position)
    {
        float distance = Vector3.Distance(Center, position);
        if (distance < 256)
        {
            return 0;
        }
        else if (distance < 512)
        {
            return 1;
        }
        else if (distance < 2048)
        {
            return 2;
        }
        else if (distance < 8196)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    public abstract void RenderChunk(int modelLocation);
    public abstract void RenderChunkTransparent();
    public abstract void Clear();
    public abstract void GenerateInfo();
    public abstract void GenerateChunk();
}