using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class Chunk
{
    public static List<Chunk> Chunks = [];

    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static Chunk Empty = new();

    public ChunkStatus Status = ChunkStatus.Empty;    
    private ChunkStatus _lastStatus = ChunkStatus.Empty;

    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage;
    public BoundingBox boundingBox = new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 0));

    public List<Vector3> Wireframe = [];

    public Dictionary<Vector3i, ChunkEntry?> NeighbourCunks = new Dictionary<Vector3i, ChunkEntry?>
    {
        { (-1, -1, -1), null }, { (-1, -1,  0), null }, { (-1, -1,  1), null },
        { (-1,  0, -1), null }, { (-1,  0,  0), null }, { (-1,  0,  1), null },
        { (-1,  1, -1), null }, { (-1,  1,  0), null }, { (-1,  1,  1), null },

        { ( 0, -1, -1), null }, { ( 0, -1,  0), null }, { ( 0, -1,  1), null },
        { ( 0,  0, -1), null },                         { ( 0,  0,  1), null },
        { ( 0,  1, -1), null }, { ( 0,  1,  0), null }, { ( 0,  1,  1), null },

        { ( 1, -1, -1), null }, { ( 1, -1,  0), null }, { ( 1, -1,  1), null },
        { ( 1,  0, -1), null }, { ( 1,  0,  0), null }, { ( 1,  0,  1), null },
        { ( 1,  1, -1), null }, { ( 1,  1,  0), null }, { ( 1,  1,  1), null },
    };
    
    public int ChunkCount = 0;

    private VAO _edgeVao = new VAO();
    private VBO<Vector3> _edgeVbo = new([(0, 0, 0)]);

    public bool Save = true;
    public bool Loaded = false;

    public Action Render = () => { };
    public Func<bool> CreateChunk = () => { return false;};

    public bool IsDisabled = true;
    public bool HasBlocks = false;
    public bool BlockRendering = false;

    public bool IsTransparentDisabled = true;
    public bool HasTransparentBlocks = false;


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        public Vector3 Position;
        public Vector2i TextureIndex;

        public override string ToString()
        {
            return $"Position: {Position}, TextureIndex: {TextureIndex}";
        }
    }

    private VAO _chunkVao = new VAO();
    public IBO _ibo = new([]);
    public VBO<VertexData> VertexVBO = new([]);

    private VAO _transparentVao = new VAO();
    public IBO _transparentIbo = new([]);
    private VBO<VertexData> _transparentVbo = new([]);

    public int IndicesCount = 0;
    public int VertexCount = 0;
    private int _vertexCount = 0; // used when rendering

    public int TransparentIndicesCount = 0;
    public int TransparentVertexCount = 0;
    private int _transparentVertexCount = 0; // used when rendering

    public List<uint> _indices = [];
    public List<VertexData> VertexDataList = [];

    public List<uint> TransparentIndices = [];
    public List<VertexData> TransparentVertexDataList = [];

    public object Lock = new object();

    public Chunk()
    { 
        blockStorage = CornerBlockStorage.Empty; 
        Chunks.Add(this);
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
        Chunks.Add(this);
    }

    public void SetPosition(Vector3i position)
    {
        this.position = position;
        boundingBox.Min = Mathf.Num(position * 32);
        boundingBox.Max = boundingBox.Min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);
    }

    public void AddFace(Vector3i position, int width, int height, int side, int textureIndex, Vector4i ambientOcclusion)
    {
        var vertices = VoxelData.GetSideOffsets[side](width, height);

        AddVertex(position + vertices[0], 0, width, height, side, textureIndex, ambientOcclusion[0]);
        AddVertex(position + vertices[1], 1, width, height, side, textureIndex, ambientOcclusion[1]);
        AddVertex(position + vertices[2], 2, width, height, side, textureIndex, ambientOcclusion[2]);
        AddVertex(position + vertices[3], 3, width, height, side, textureIndex, ambientOcclusion[3]);

        uint v = (uint)VertexCount;
        _indices.AddRange(v,v+1,v+2,v+2,v+3,v);
        VertexCount += 4;
    }

    public void AddTransparentFace(Vector3i position, int width, int height, int side, int textureIndex, Vector4i ambientOcclusion)
    {
        var vertices = VoxelData.GetSideOffsets[side](width, height);

        AddTransparentVertex(position + vertices[0], 0, width, height, side, textureIndex, ambientOcclusion[0]);
        AddTransparentVertex(position + vertices[1], 1, width, height, side, textureIndex, ambientOcclusion[1]);
        AddTransparentVertex(position + vertices[2], 2, width, height, side, textureIndex, ambientOcclusion[2]);
        AddTransparentVertex(position + vertices[3], 3, width, height, side, textureIndex, ambientOcclusion[3]);

        uint v = (uint)TransparentVertexCount;
        TransparentIndices.AddRange(v,v+1,v+2,v+2,v+3,v);
        TransparentVertexCount += 4;
    }
    
    public void AddVertex(Vector3 position, int uvIndex, int width, int height, int side, int textureIndex, int ambientOcclusion)
    {
        VertexData vertexData = new()
        {
            Position = position,
            TextureIndex = ((uvIndex << 29) | (side << 26) | ((width - 1) << 21) | ((height - 1) << 16) | textureIndex, ambientOcclusion)
        };
        VertexDataList.Add(vertexData);
    }

    public void AddTransparentVertex(Vector3 position, int uvIndex, int width, int height, int side, int textureIndex, int ambientOcclusion)
    {
        VertexData vertexData = new()
        {
            Position = position,
            TextureIndex = ((uvIndex << 29) | (side << 26) | ((width - 1) << 21) | ((height - 1) << 16) | textureIndex, ambientOcclusion)
        };
        TransparentVertexDataList.Add(vertexData);
    }

    public Block this[int index]
    {
        get => blockStorage[index];
        set => blockStorage[index] = value;
    }

    public Block this[int x, int y, int z]
    {
        get => this[x + (z * 32) + (y * 1024)];
        set => this[x + (z * 32) + (y * 1024)] = value;
    }

    public Block this[Vector3i position]
    {
        get => this[position.X, position.Y, position.Z];
        set => this[position.X, position.Y, position.Z] = value;
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
            _edgeVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _edgeVbo);

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

    public Matrix4 GetModel()
    {
        return Matrix4.CreateTranslation(GetWorldPosition());
    }

    public void Clear()
    {
        ClearMeshData();

        blockStorage.Clear();
        Wireframe = [];

        VertexCount = 0;
        Status = ChunkStatus.Empty;
        Loaded = false;
        Save = true;
    }

    public void ClearMeshData()
    {
        VertexDataList = new List<VertexData>();
        _indices = new List<uint>();

        TransparentVertexDataList = new List<VertexData>();
        TransparentIndices = new List<uint>();

        VertexCount = 0;
        IndicesCount = 0;

        TransparentVertexCount = 0;
        TransparentIndicesCount = 0;
    }

    public void Delete()
    {
        Clear();
        _edgeVao.DeleteBuffer();
        _edgeVbo.DeleteBuffer();
        _chunkVao.DeleteBuffer();
        VertexVBO.DeleteBuffer();
        _ibo.DeleteBuffer();
        _transparentVao.DeleteBuffer();
        _transparentVbo.DeleteBuffer();
        _transparentIbo.DeleteBuffer();
        Chunks.Remove(this);
    }

    
    public bool CreateChunkSolid()
    {   
        lock(Lock)
        {  
            // Opaque chunk
            _chunkVao.Renew();
            _chunkVao.Bind();

            try
            {
                _ibo.Renew(_indices);
                Shader.Error("Creating the IBO for chunk: " + position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create IBO for chunk: " + position);
                for (int i = 0; i < _indices.Count; i++)
                {
                    if (_indices[i] >= VertexDataList.Count)
                    {
                        Console.WriteLine("Index out of range: " + _indices[i] + " >= " + VertexDataList.Count);
                    }
                }

                ClearMeshData();

                BlockRendering = true;
                return false;
            }
            
            try
            {
                VertexVBO.Renew(VertexDataList);
                Shader.Error("Creating the VBO for chunk: " + position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create VBO for chunk: " + position);
                
                ClearMeshData();

                BlockRendering = true;
                return false;
            }

            int stride = Marshal.SizeOf(typeof(VertexData));
            VertexVBO.Bind();

            _chunkVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
            Shader.Error("Linking position attribute for chunk: " + position);
            _chunkVao.IntLink(1, 2, VertexAttribIntegerType.Int, stride, 3 * sizeof(float));
            Shader.Error("Linking texture index attribute for chunk: " + position);

            _chunkVao.Unbind();
            VertexVBO.Unbind();


            // Transparent chunk
            _transparentVao.Renew();
            _transparentVao.Bind();

            try
            {
                _transparentIbo.Renew(TransparentIndices);
                Shader.Error("Creating the IBO for transparent chunk: " + position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ClearMeshData();

                BlockRendering = true;
                return false;
            }

            try
            {
                _transparentVbo.Renew(TransparentVertexDataList);
                Shader.Error("Creating the VBO for transparent chunk: " + position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create VBO for transparent chunk: " + position);

                ClearMeshData();

                BlockRendering = true;
                return false;
            }

            stride = Marshal.SizeOf(typeof(VertexData));
            _transparentVbo.Bind();

            _transparentVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
            Shader.Error("Linking position attribute for transparent chunk: " + position);
            _transparentVao.IntLink(1, 2, VertexAttribIntegerType.Int, stride, 3 * sizeof(float));
            Shader.Error("Linking texture index attribute for transparent chunk: " + position);

            _transparentVao.Unbind();
            _transparentVbo.Unbind();


            // Setting values
            int indicesCount = _indices.Count;
            int vertexCount = VertexDataList.Count;
            int transparentIndicesCount = TransparentIndices.Count;
            int transparentVertexCount = TransparentVertexDataList.Count;

            ClearMeshData();

            IndicesCount = indicesCount;
            VertexCount = vertexCount;
            _vertexCount = IndicesCount;

            TransparentIndicesCount = transparentIndicesCount;
            TransparentVertexCount = transparentVertexCount;
            _transparentVertexCount = TransparentIndicesCount;

            GL.Finish();
        }

        BlockRendering = false;
        return true;
    }

    public void Reload()
    {
        ClearMeshData();
    }

    public bool CreateChunkWireframe()
    {
        _edgeVao.Renew();
        _edgeVbo.Renew(Wireframe.ToArray());
        
        _edgeVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _edgeVbo);
        return true;
    }


    public void RenderChunk()
    {
        _chunkVao.Bind();
        _ibo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);
        
        _ibo.Unbind();
        _chunkVao.Unbind();
    }

    public void RenderChunkTransparent()
    {
        _transparentVao.Bind();
        _transparentIbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _transparentVertexCount, DrawElementsType.UnsignedInt, 0);

        _transparentIbo.Unbind();
        _transparentVao.Unbind();
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

    public void UpdateNeighbours()
    {
        ChunkCount = 0;
        foreach (var (side, _) in ChunkData.SideChunkIndices)
        {
            Vector3i position = GetRelativePosition() + side;
            if (ChunkManager.GetChunk(position, out ChunkEntry? chunk))
            {
                ChunkCount++;
                NeighbourCunks[side] = chunk;
            }
            else
            {
                NeighbourCunks[side] = null;
            }
        }
    }

    public bool AllChunksStageBetween(ChunkStage stage1, ChunkStage stage2)
    {
        foreach (var (_, chunk) in NeighbourCunks)
        {
            if (chunk == null || chunk.Stage < stage1 || chunk.Stage > stage2)
            {
                return false;
            }
        }

        return true;
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