using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class LODChunk : LODChunkBase
{
    /* Generation info */
    public bool Blocked = false;
    public ChunkMesh Mesh;

    public LODChunk(Vector3i position, int size, int resolution) : base(position, size, resolution)
    {
        Mesh = new ChunkMesh(position);
    }
    public LODChunk() : base(Vector3i.Zero, 32, 0)
    {
        Mesh = new ChunkMesh(Vector3i.Zero);
    }

    public override void RenderChunk(int modelLocation)
    {
        int scale = (int)Mathf.Pow(2, Resolution);
        Matrix4 model = Matrix4.CreateTranslation(Position) * Matrix4.CreateScale(scale, scale, scale);
        GL.UniformMatrix4(modelLocation, false, ref model);
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
        Mesh.AddFace(Position, 20, 20, 0, 0, (0, 0, 0, 0));
        LODChunk chunk = this;
        ChunkLODGenerationProcess.GenerateChunk(ref chunk, Position, 0);
        Mesh.CreateChunkSolid();

        //ChunkLODGenerationProcess process = new ChunkLODGenerationProcess(this);
        //ThreadPool.QueueAction(process);
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