using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class ChunkData
{
    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage = new(new Vector3i(0, 0, 0));
    public BoundingBox boundingBox = new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 0));

    public List<Vector3> Wireframe = [];

    private VAO _edgeVao = new VAO();
    private VBO _edgeVbo = new VBO([(0, 0, 0)]);

    public bool Save = true;

    public Action Render = () => { };
    public Action CreateChunk = () => { };

    public bool IsDisabled = true;
    public bool HasBlocks = false;

    private VAO _chunkVao = new VAO();
    public SSBO VertexSSBO = new SSBO(new List<Vector2i>());
    public List<Vector2i> GridAlignedFaces = new List<Vector2i>();

    public void AddFace(byte posX, byte posY, byte posZ, byte width, byte height, int blockIndex, byte side)
    {
        Vector3i size = FaceVertices[side];
        int vertex = posX | (posY << 5) | (posZ << 10) | (width << 15) | (height << 20);
        int blockData = blockIndex | (side << 16) | (size.X << 19) | (size.Y << 21) | (size.Z << 23);

        GridAlignedFaces.Add(new Vector2i(vertex, blockData));
    }

    public void AddFace(Vector3 position, byte width, byte height, int blockIndex, byte side)
    {
        AddFace((byte)position.X, (byte)position.Y, (byte)position.Z, width, height, blockIndex, side);
    }

    public ChunkData(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new BlockStorage(position * 32);
        
        System.Numerics.Vector3 min = Mathf.ToNumerics(position * 32);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(Chunk.WIDTH, Chunk.HEIGHT, Chunk.DEPTH);
        
        boundingBox = new BoundingBox(min, max);

        Render = renderType == RenderType.Solid ? RenderChunk : RenderWireframe;
        CreateChunk = renderType == RenderType.Solid ? CreateChunkSolid : CreateChunkWireframe;
    }

    public void SetRenderType(RenderType type)
    {
        if (type == RenderType.Solid)
        {
            Wireframe.Clear();
            Render = RenderChunk;
            CreateChunk = CreateChunkSolid;
        }
        else if (type == RenderType.Wireframe)
        {
            _edgeVbo = new VBO(Wireframe);
            _edgeVao.LinkToVAO(0, 3, _edgeVbo);

            Render = RenderWireframe;
            CreateChunk = CreateChunkWireframe;
        }
    }

    public Vector3i GetWorldPosition()
    {
        return position * 32;
    }

    public Vector3i GetRelativePosition()
    {
        return position;
    }

    public void Clear()
    {
        GridAlignedFaces.Clear();
        Wireframe.Clear();
    }

    public void Delete()
    {
        _edgeVao.Delete();
        _edgeVbo.Delete();

        blockStorage.Clear();
    }

    
    public void CreateChunkSolid()
    {
        _chunkVao = new VAO();
        VertexSSBO = new SSBO(GridAlignedFaces);
    }

    public void CreateChunkWireframe()
    {
        _edgeVao = new VAO();
        _edgeVbo = new VBO(Wireframe);
        
        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
    }


    public void RenderChunk()
    {
        _chunkVao.Bind();
        VertexSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, GridAlignedFaces.Count * 6);
        
        VertexSSBO.Unbind();
        _chunkVao.Unbind();
    }

    public void RenderWireframe()
    {
        _edgeVao.Bind();

        GL.LineWidth(2.0f);
        GL.DrawArrays(PrimitiveType.Lines, 0, Wireframe.Count);

        _edgeVao.Unbind();
    }

    public void SaveChunk()
    {
        if (!Save) return;
        ChunkManager.SaveChunk(this);
    }

    public bool LoadChunk()
    {
        return ChunkManager.LoadChunk(this);
    }

    public static readonly Vector3i[] FaceVertices =
    [
        (0, 1, 2),
        (2, 1, 0),
        (0, 2, 1),
        (2, 1, 0),
        (0, 2, 1),
        (0, 1, 2),
    ];
}

public struct Bounds
{
    public Vector3 Min;
    public Vector3 Max;

    public Bounds(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }
}

public enum RenderType
{
    Solid,
    Wireframe
}