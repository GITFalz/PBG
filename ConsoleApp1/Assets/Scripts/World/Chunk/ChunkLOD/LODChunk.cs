using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class LODChunk : LODChunkBase
{
    /* Generation info */
    public bool Blocked = false;
    public ChunkMesh Mesh;
    public Vortice.Mathematics.BoundingBox boundingBox;
    public Matrix4 ModelMatrix; 

    public LODChunk(Vector3i position, int size, int resolution) : base(position, size, resolution)
    {
        Mesh = new ChunkMesh(position);
        boundingBox = new Vortice.Mathematics.BoundingBox(Mathf.Num(position), Mathf.Num(position + new Vector3i(size)));
        ModelMatrix = Matrix4.CreateScale(Scale, Scale, Scale) * Matrix4.CreateTranslation(Position);
    }

    public override void RenderChunk(int modelLocation)
    {
        GL.UniformMatrix4(modelLocation, false, ref ModelMatrix);
        Mesh.RenderChunk();
    }

    public override void RenderChunkTransparent()
    {
        Mesh.RenderChunkTransparent();
    }

    public override void Clear()
    {
        Mesh.Delete();
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

    public override void GenerateChunk()
    {
        ChunkLODGenerationProcess process = new ChunkLODGenerationProcess(this);
        ThreadPool.QueueAction(process);
    }

    public override void GetChunks(List<LODChunk> chunks)
    {
        chunks.Add(this);
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