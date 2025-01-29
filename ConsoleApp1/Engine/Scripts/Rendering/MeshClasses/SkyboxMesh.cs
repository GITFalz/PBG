using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class SkyboxMesh
{
    private VAO _vao = new VAO();

    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO([(0, 0, 0)]);
    private VBO _uvVbo = new VBO([(0, 0)]);
    private VBO _textureVbo = new VBO([0]);

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> _transformedVerts = new List<Vector3>();

    public SkyboxMesh()
    {
        _transformedVerts =
        [
            // Front
            (-1, -1, -1),
            (1, -1, -1),
            (1, 1, -1),
            (-1, 1, -1),

            // Right
            (1, -1, -1),
            (1, -1, 1),
            (1, 1, 1),
            (1, 1, -1),

            // Top
            (-1, 1, -1),
            (1, 1, -1),
            (1, 1, 1),
            (-1, 1, 1),

            // Left
            (-1, -1, -1),
            (-1, 1, -1),
            (-1, 1, 1),
            (-1, -1, 1),

            // Bottom
            (-1, -1, -1),
            (1, -1, -1),
            (1, -1, 1),
            (-1, -1, 1),

            // Back
            (-1, -1, 1),
            (1, -1, 1),
            (1, 1, 1),
            (-1, 1, 1)
        ];

        Indices =
        [
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 18, 17, 18, 16, 19,
            20, 22, 21, 22, 20, 23
        ];

        Uvs =
        [
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1),
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1),
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1),
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1),
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1),
            (0, 0),
            (1, 0),
            (1, 1),
            (0, 1)
        ];

        TextureIndices = [ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ];

        GenerateBuffers();
    }
    
    public void GenerateBuffers()
    {
        _vertVbo = new VBO(_transformedVerts);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        
        _ibo = new IBO(Indices);
    }

    public void Render()
    {
        _vao.Bind();
        _ibo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

        _vao.Unbind();
        _ibo.Unbind();
    }

    public void Delete()
    {
        _textureVbo.Delete();
    }
}