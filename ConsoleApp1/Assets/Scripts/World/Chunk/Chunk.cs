using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class Chunk
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static Chunk Empty = new();

    public ChunkStage Stage = ChunkStage.Empty;
    public ChunkStatus Status = ChunkStatus.Empty;
    private ChunkStatus _lastStatus = ChunkStatus.Empty;

    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage;
    public int[] FullBlockMap = new int[32 * 32]; // every full block in the chunk represented by a single bit
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
    public int VertexCount = 0;

    private VAO _chunkVao = new VAO();
    public SSBO <Vector2i>VertexSSBO = new(new List<Vector2i>());
    public SSBO<int> BlockMapSSBO = new(new int[1024]);
    private List<Vector2i> _gridAlignedData = [];
    public List<Vector2i> GridAlignedFaces = [];

    public Chunk()
    { 
        blockStorage = CornerBlockStorage.Empty; 
    }

    public Chunk(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new FullBlockStorage();
        
        System.Numerics.Vector3 min = Mathf.Num(position * 32);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);
        
        boundingBox = new BoundingBox(min, max);

        Render = renderType == RenderType.Solid ? RenderChunk : RenderWireframe;
        CreateChunk = renderType == RenderType.Solid ? CreateChunkSolid : CreateChunkWireframe;
    }

    public void SetPosition(Vector3i position)
    {
        this.position = position;
        boundingBox.Min = Mathf.Num(position * 32);
        boundingBox.Max = boundingBox.Min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);
    }

    public void AddFace(byte posX, byte posY, byte posZ, byte width, byte height, int blockIndex, byte side)
    {
        int vertex = posX | (posY << 5) | (posZ << 10) | (width << 15) | (height << 20) | (side << 25);
        int blockData = blockIndex;

        lock (this)
        {
            GridAlignedFaces.Add(new Vector2i(vertex, blockData));
        }
    }

    public void AddFace(Vector3 position, byte width, byte height, int blockIndex, byte side)
    {
        AddFace((byte)position.X, (byte)position.Y, (byte)position.Z, width, height, blockIndex, side);
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
            _edgeVbo.Renew(Wireframe.ToArray());
            _edgeVao.LinkToVAO(0, 3, _edgeVbo);

            Render = RenderWireframe;
            CreateChunk = CreateChunkWireframe;
        }
    }

    public Vector3i GetWorldPosition()
    {
        return position * 32;
    }

    public Vector3i GetCenterPosition()
    {
        return GetWorldPosition() + (16, 16, 16);
    }

    public Vector3i GetRelativePosition()
    {
        return position;
    }

    public Matrix4 GetModel()
    {
        return Matrix4.CreateTranslation(GetWorldPosition());
    }

    public void Clear()
    {
        blockStorage.Clear();
        GridAlignedFaces.Clear();
        _gridAlignedData.Clear();
        Wireframe.Clear();
        RemoveChunkFromAll();
        VertexCount = 0;
        Stage = ChunkStage.Empty;
        Status = ChunkStatus.Empty;
        Loaded = false;
        Save = true;
    }

    public void Delete()
    {
        Clear();
        _edgeVao.DeleteBuffer();
        _edgeVbo.DeleteBuffer();
        _chunkVao.DeleteBuffer();
        VertexSSBO.DeleteBuffer();
        BlockMapSSBO.DeleteBuffer();
    }

    
    public void CreateChunkSolid()
    {   
        lock(this)
        {
            _gridAlignedData.Clear();
            _gridAlignedData = [.. GridAlignedFaces];
            VertexCount = _gridAlignedData.Count * 6;
            GridAlignedFaces.Clear();

            _chunkVao.Renew();
            VertexSSBO.Renew(_gridAlignedData.ToArray());
            BlockMapSSBO.Update(FullBlockMap, 1);
        }
    }

    public void Reload()
    {
        _gridAlignedData.Clear();
        GridAlignedFaces.Clear();
        VertexCount = 0;
        Stage = ChunkStage.Populated;
    }

    public void CreateChunkWireframe()
    {
        _edgeVao.Renew();
        _edgeVbo.Renew(Wireframe.ToArray());
        
        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
    }


    public void RenderChunk()
    {
        _chunkVao.Bind();
        VertexSSBO.Bind(0);
        BlockMapSSBO.Bind(1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _gridAlignedData.Count * 6);
        
        BlockMapSSBO.Unbind();
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

    public bool AllNeighbourChunkStageEqual(ChunkStage stage)
    {
        lock (this)
        {
            foreach (var chunk in NeighbourCunks)
            {
                if (chunk == null || (int)chunk.Stage != (int)stage)
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

    public bool HasAllNeighbourChunks()
    {
        lock (this)
        {
            foreach (var chunk in NeighbourCunks)
            {
                if (chunk == null)
                    return false;
            }
            return true;
        }
    }

    public bool IsActive()
    {
        return Status == ChunkStatus.Active;
    }

    public bool IsInactive()
    {
        return Status == ChunkStatus.Inactive;
    }

    public bool IsIndependent()
    {
        return Status == ChunkStatus.Independent;
    }

    public bool WasActive()
    {
        return _lastStatus == ChunkStatus.Active;
    }

    public bool WasInactive()
    {
        return _lastStatus == ChunkStatus.Inactive;
    }

    public bool WasIndependent()
    {
        return _lastStatus == ChunkStatus.Independent;
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