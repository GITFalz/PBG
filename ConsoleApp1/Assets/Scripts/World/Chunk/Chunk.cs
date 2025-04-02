using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class Chunk
{
    public static Chunk Empty = new();

    public ChunkStage Stage = ChunkStage.Empty;

    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage = BlockStorage.Empty;
    public BoundingBox boundingBox = new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 0));

    public List<Vector3> Wireframe = [];

    public Chunk?[] NeighbourCunks = 
    [
        null, null, null, 
        null, null, null, 
        null, null, null,

        null, null, null, 
        null,       null, 
        null, null, null,

        null, null, null, 
        null, null, null, 
        null, null, null,  
    ];
    public int ChunkCount = 0;

    private VAO _edgeVao = new VAO();
    private VBO<Vector3> _edgeVbo = new([(0, 0, 0)]);

    public bool Save = true;
    public bool Loaded = false;

    public Action Render = () => { };
    public Action CreateChunk = () => { };

    public bool IsDisabled = true;
    public bool HasBlocks = false;

    private VAO _chunkVao = new VAO();
    public SSBO <Vector2i>VertexSSBO = new(new List<Vector2i>());
    private List<Vector2i> _gridAlignedData = [];
    public List<Vector2i> GridAlignedFaces = [];

    public Chunk() { }

    public void AddFace(byte posX, byte posY, byte posZ, byte width, byte height, int blockIndex, byte side)
    {
        int vertex = posX | (posY << 5) | (posZ << 10) | (width << 15) | (height << 20) | (side << 25);
        int blockData = blockIndex;

        GridAlignedFaces.Add(new Vector2i(vertex, blockData));
    }

    public void AddFace(Vector3 position, byte width, byte height, int blockIndex, byte side)
    {
        AddFace((byte)position.X, (byte)position.Y, (byte)position.Z, width, height, blockIndex, side);
    }

    public Chunk(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new BlockStorage(position * 32);
        
        System.Numerics.Vector3 min = Mathf.ToNumerics(position * 32);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);
        
        boundingBox = new BoundingBox(min, max);

        Render = renderType == RenderType.Solid ? RenderChunk : RenderWireframe;
        CreateChunk = renderType == RenderType.Solid ? CreateChunkSolid : CreateChunkWireframe;
    }

    public Block this[int index]
    {
        get
        {
            return blockStorage[index];
        }
        set
        {
            blockStorage[index] = value;
        }
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
            _edgeVbo = new(Wireframe);
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
        RemoveChunkFromAll();
        GridAlignedFaces.Clear();
        Wireframe.Clear();
    }

    public void Delete()
    {
        blockStorage.Clear();
    }

    
    public void CreateChunkSolid()
    {   
        _gridAlignedData.Clear();
        _gridAlignedData = [.. GridAlignedFaces];
        GridAlignedFaces.Clear();

        _chunkVao = new VAO();
        VertexSSBO = new(_gridAlignedData);
    }

    public void CreateChunkWireframe()
    {
        _edgeVao = new VAO();
        _edgeVbo = new(Wireframe);
        
        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
    }


    public void RenderChunk()
    {
        _chunkVao.Bind();
        VertexSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _gridAlignedData.Count * 6);
        
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
        ChunkSaver.SaveChunk(this);
    }

    public bool LoadChunk()
    {
        Loaded = ChunkLoader.LoadChunk(this);
        return Loaded;
    }

    public bool AddChunk(Chunk chunk)
    {
        Vector3i sidePosition = GetRelativePosition() - chunk.GetRelativePosition();
        if (!ChunkData.SideChunkIndices.TryGetValue(sidePosition, out int index1) || !ChunkData.SideChunkIndices.TryGetValue(-sidePosition, out int index2))
            return false;

        lock (this)
        {
            lock (chunk)
            {
                NeighbourCunks[index1] = chunk;
                chunk.NeighbourCunks[index2] = this;

                Interlocked.Increment(ref ChunkCount);
                Interlocked.Increment(ref chunk.ChunkCount);
            }
        }
        return true;
    } 

    public bool RemoveChunk(Chunk chunk)
    {
        Vector3i sidePosition = GetRelativePosition() - chunk.GetRelativePosition();
        if (!ChunkData.SideChunkIndices.TryGetValue(sidePosition, out int index1) || !ChunkData.SideChunkIndices.TryGetValue(-sidePosition, out int index2))
            return false;

        lock (this)
        {
            lock (chunk)
            {
                NeighbourCunks[index1] = null;
                chunk.NeighbourCunks[index2] = null;

                Interlocked.Decrement(ref ChunkCount);
                Interlocked.Decrement(ref chunk.ChunkCount);
            }
        }
        return true;
    } 

    public void RemoveChunkFromAll()
    {
        foreach (var chunk in NeighbourCunks)
        {
            chunk?.RemoveChunk(this);
        }
    }

    public bool GetSideChunk(int side, out Chunk chunk)
    {
        chunk = Empty;
        if (side >= 0 && side <= 5)
        {   
            Chunk? sideChunk = NeighbourCunks[SideIndices[side]];
            if (sideChunk == null)
                return false;

            chunk = sideChunk;
            return true;
        }
        return false;
    }

    public bool AllNeighbourChunkStageSuperiorOrEqual(ChunkStage stage)
    {
        lock (this)
        {
            foreach (var chunk in NeighbourCunks)
            {
                if (chunk == null || (int)chunk.Stage < (int)stage)
                    return false;
            }
            return true;
        }   
    }

    public Vector3i[] GetSideChunkPositions()
    {
        Vector3i[] coords = new Vector3i[ChunkData.SidePositions.Length];
        for (int i = 0; i < ChunkData.SidePositions.Length; i++)
        {
            coords[i] = (GetRelativePosition() + ChunkData.SidePositions[i]) * 32;
        }
        return coords;
    }

    public List<Chunk> GetNeighbourChunks()
    {
        lock (this)
        {
            List<Chunk> chunks = [];
            foreach (var chunk in NeighbourCunks)
            {
                if (chunk != null)
                    chunks.Add(chunk);
            }
            return chunks;
        }
    }

    private static readonly int[] SideIndices = [
        12, 21, 15, 4, 10, 13
    ];
}

public enum RenderType
{
    Solid,
    Wireframe
}