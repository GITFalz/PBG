using OpenTK.Mathematics;

public class LODChunk : LODChunkBase
{
    public LODChunk(Vector3i position, int size, int resolution) : base(position, size, resolution) { }
    public LODChunk(): base(Vector3i.Zero, 32, 0) { }

    public override void Clear()
    {
        // Implement clearing logic for LODChunk if necessary
    }

    public override void GenerateInfo()
    {
        Info.AddBlock(new InfoBlockData()
        {
            Position = Position + new Vector3i((Resolution + 1) * 4),
            Size = new Vector3i(Size - ((Resolution + 1) * 8)),
            Color = GetColor(),
        });
    }

    public Vector4 GetColor()
    {
        switch (Resolution)
        {
            case 0: return new Vector4(1, 0, 0, 1);
            case 1: return new Vector4(1, 1, 0, 1);
            case 2: return new Vector4(0, 1, 0, 1);
            case 3: return new Vector4(0, 1, 1, 1);
            default: return new Vector4(1, 1, 1, 1);
        }
    }
}