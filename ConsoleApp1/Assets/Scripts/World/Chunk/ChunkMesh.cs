using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ChunkMesh
{
    private Vector3i _position;

    public ChunkMesh(Vector3i position)
    {
        _position = position;
    }

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
    }

    private bool _generateBuffers = false;

    private VAO _chunkVao;
    public IBO _ibo;
    public VBO<VertexData> VertexVBO;

    private VAO _transparentVao;
    public IBO _transparentIbo;
    private VBO<VertexData> _transparentVbo;

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

    public void AddFace(Vector3i position, int width, int height, int side, int textureIndex, Vector4i ambientOcclusion)
    {
        var vertices = VoxelData.GetSideOffsets[side](width, height);

        AddVertex(position + vertices[0], 0, width, height, side, textureIndex, ambientOcclusion[0]);
        AddVertex(position + vertices[1], 1, width, height, side, textureIndex, ambientOcclusion[1]);
        AddVertex(position + vertices[2], 2, width, height, side, textureIndex, ambientOcclusion[2]);
        AddVertex(position + vertices[3], 3, width, height, side, textureIndex, ambientOcclusion[3]);

        uint v = (uint)VertexCount;
        _indices.AddRange(v, v + 1, v + 2, v + 2, v + 3, v);
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
        TransparentIndices.AddRange(v, v + 1, v + 2, v + 2, v + 3, v);
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

    public bool CreateChunkSolid()
    {
        lock (Lock)
        {
            // Opaque chunk
            _chunkVao = new();
            _chunkVao.Bind();

            try
            {
                _ibo = new(_indices);
                Shader.Error("Creating the IBO for chunk: " + _position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create IBO for chunk: " + _position);
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
                VertexVBO = new(VertexDataList);
                Shader.Error("Creating the VBO for chunk: " + _position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create VBO for chunk: " + _position);

                ClearMeshData();

                BlockRendering = true;
                return false;
            }

            int stride = Marshal.SizeOf(typeof(VertexData));
            VertexVBO.Bind();

            _chunkVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
            Shader.Error("Linking position attribute for chunk: " + _position);
            _chunkVao.IntLink(1, 2, VertexAttribIntegerType.Int, stride, 3 * sizeof(float));
            Shader.Error("Linking texture index attribute for chunk: " + _position);

            _chunkVao.Unbind();
            VertexVBO.Unbind();


            // Transparent chunk
            _transparentVao = new();
            _transparentVao.Bind();

            try
            {
                _transparentIbo = new(TransparentIndices);
                Shader.Error("Creating the IBO for transparent chunk: " + _position);
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
                _transparentVbo = new(TransparentVertexDataList);
                Shader.Error("Creating the VBO for transparent chunk: " + _position);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to create VBO for transparent chunk: " + _position);

                ClearMeshData();

                BlockRendering = true;
                return false;
            }

            stride = Marshal.SizeOf(typeof(VertexData));
            _transparentVbo.Bind();

            _transparentVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
            Shader.Error("Linking position attribute for transparent chunk: " + _position);
            _transparentVao.IntLink(1, 2, VertexAttribIntegerType.Int, stride, 3 * sizeof(float));
            Shader.Error("Linking texture index attribute for transparent chunk: " + _position);

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
            HasBlocks = vertexCount > 0;

            TransparentIndicesCount = transparentIndicesCount;
            TransparentVertexCount = transparentVertexCount;
            _transparentVertexCount = TransparentIndicesCount;
            HasTransparentBlocks = transparentVertexCount > 0;

            _generateBuffers = true;

            GL.Finish();
        }

        BlockRendering = false;
        return true;
    }

    public void ClearMeshData()
    {
        VertexDataList = [];
        _indices = [];

        TransparentVertexDataList = [];
        TransparentIndices = [];

        VertexCount = 0;
        IndicesCount = 0;

        TransparentVertexCount = 0;
        TransparentIndicesCount = 0;
    }

    public void RenderChunk()
    {
        if (!_generateBuffers)
            return;

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

    public void Delete()
    {
        if (!_generateBuffers)
            return;

        _chunkVao.DeleteBuffer();
        VertexVBO.DeleteBuffer();
        _ibo.DeleteBuffer();
        _transparentVao.DeleteBuffer();
        _transparentVbo.DeleteBuffer();
        _transparentIbo.DeleteBuffer();

        _generateBuffers = false;
    }
}