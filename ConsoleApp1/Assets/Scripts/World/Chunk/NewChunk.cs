using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class NewChunk
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

    public bool Save = true;
    public bool Loaded = false;

    public bool IsDisabled = true;
    public bool HasBlocks = false;
    public uint VertexCount = 0;

    public struct VertexData
    {
        public Vector3 Position;
        public uint TextureIndex;
    }

    private VAO _chunkVao = new VAO();
    private IBO _ibo = new([]);
    private VBO<VertexData> VertexVBO = new([]);

    private List<uint> _indices = [];
    public List<VertexData> VertexDataList = [];

    public NewChunk()
    { 
        blockStorage = CornerBlockStorage.Empty; 
    }

    public NewChunk(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new FullBlockStorage();
        
        System.Numerics.Vector3 min = Mathf.Num(position * 32);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH); 
    }

    public void AddFace(Vector3 position, int width, int height, int side, int textureIndex)
    {
        var vertices = VoxelData.GetSideOffsets[side](width, height);

        AddVertex(position + vertices[0], 0, (uint)side, (uint)textureIndex);
        AddVertex(position + vertices[1], 1, (uint)side, (uint)textureIndex);
        AddVertex(position + vertices[2], 2, (uint)side, (uint)textureIndex);
        AddVertex(position + vertices[2], 2, (uint)side, (uint)textureIndex);
        AddVertex(position + vertices[3], 3, (uint)side, (uint)textureIndex);
        AddVertex(position + vertices[0], 0, (uint)side, (uint)textureIndex);

        _indices.Add(VertexCount + 0);
        _indices.Add(VertexCount + 1);
        _indices.Add(VertexCount + 2);
        _indices.Add(VertexCount + 3);
        _indices.Add(VertexCount + 4);
        _indices.Add(VertexCount + 5);

        VertexCount += 6;
    }
    
    public void AddVertex(Vector3 position, uint uvIndex, uint side, uint textureIndex)
    {
        VertexData vertexData = new()
        {
            Position = position,
            TextureIndex = (uvIndex << 29) | (side << 26) | textureIndex
        };
        VertexDataList.Add(vertexData);
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
        _indices.Clear();
        VertexDataList.Clear();
        VertexCount = 0;
        Stage = ChunkStage.Empty;
        Status = ChunkStatus.Empty;
        Loaded = false;
        Save = true;
    }

    public void Delete()
    {
        Clear();
        _chunkVao.DeleteBuffer();
        VertexVBO.DeleteBuffer();
        _ibo.DeleteBuffer();
    }

    
    public void CreateChunkSolid()
    {   
        _ibo.Renew(_indices);
        VertexVBO.Renew(VertexDataList);

         int stride = Marshal.SizeOf(typeof(VertexData));
        _chunkVao.Bind();
        VertexVBO.Bind();

        _chunkVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
        _chunkVao.IntLink(1, 1, VertexAttribIntegerType.UnsignedInt, stride, 3 * sizeof(float));

        VertexVBO.Unbind();
        _chunkVao.Unbind();
    }

    public void RenderChunk()
    {
        _chunkVao.Bind();
        Shader.Error("After VAO Bind: ");
        _ibo.Bind();
        Shader.Error("After IBO Bind: ");

        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        Shader.Error("After DrawElements: ");
        
        _ibo.Unbind();
        _chunkVao.Unbind();
    }
}