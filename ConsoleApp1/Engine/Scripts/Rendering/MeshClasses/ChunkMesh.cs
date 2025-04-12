using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ChunkMesh
{
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new([(0, 0, 0)]);
    private VBO<Vector2> _uvVbo = new([(0, 0)]);
    private VBO<int> _textureVbo = new([0]);
    private VBO<Vector3> _normalVbo = new([(0, 0, 0)]);

    private TBO<int> _blockTbo = new([]);

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector3> Vertices = new List<Vector3>();

    public List<ChunkPlane> Planes = [new()];

    public void Init()
    {
        Vertices.Clear();
        Indices.Clear();
        Uvs.Clear();
        TextureIndices.Clear();
        Normals.Clear();

        uint count = 0;
        foreach (var plane in Planes)
        {
            if (plane.IsDisabled)
                continue;

            Vertices.AddRange(plane.A, plane.D, plane.C, plane.C, plane.B, plane.A);
            Indices.AddRange([count+0, count+1, count+2, count+3, count+4, count+5]);

            // Basic values
            Uvs.AddRange([(0, 0), (0, 1), (1, 1), (1, 1), (1, 0), (0, 0)]);
            TextureIndices.AddRange([0, 0, 0, 0, 0, 0]);
            Normals.AddRange([(0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0)]);
            count += 6;
        }
    }

    public void GenerateBuffers()
    {
        _vertVbo.Renew(Vertices);
        _uvVbo.Renew(Uvs);
        _textureVbo.Renew(TextureIndices);
        _normalVbo.Renew(Normals);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);
        
        _ibo.Renew(Indices);
    }

    public void RenderMesh()
    {
        _vao.Bind();
        _ibo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        
        _vao.Unbind();
        _ibo.Unbind();
    }
}

public class ChunkPlane
{
    public Vector3 A = new Vector3(0, 0, 0);
    public Vector3 B = new Vector3(32, 0, 0);
    public Vector3 C = new Vector3(32, 32, 0);
    public Vector3 D = new Vector3(0, 32, 0);

    public bool IsDisabled = false;
}