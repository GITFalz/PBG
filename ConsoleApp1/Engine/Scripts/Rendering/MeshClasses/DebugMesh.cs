using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class DebugMesh : Mesh
{
    public List<int> TextureIndices;
    
    public List<Vector3> _transformedVerts;
    
    private VBO _textureVbo;
    
    public DebugMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
    }
    
    /// <summary>
    /// !!!NOTE: This function NEEDS to be called before scaling or rotating the mesh!!!
    /// </summary>
    public void UpdatePosition()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Vertices[i] + Position;
        }
    }
    
    /// <summary>
    /// NOTE: This function NEEDS to be called after positioning and before rotating!!!
    /// </summary>
    /// <param name="size"></param>
    public void UpdateScale(Vector3 size)
    {
        size = Mathf.Abs(size);
        size -= Vector3.One;
        
        for (int i = 0; i < Vertices.Count; i++)
        {
            int offset = i % 4;
            int face = i / 4;
            _transformedVerts[i] += Vertices[i] * size;
        }
    }

    /// <summary>
    /// NOTE: This function NEEDS to be called after positioning and scaling!!!
    /// </summary>
    /// <param name="center"></param>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    public void UpdateRotation(Vector3 center, Vector3 axis, float angle)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], center, axis, angle);
        }
    }

    public override void UpdateMesh()
    {
        _vertVbo.Update(_transformedVerts);
    }

    public override void GenerateBuffers()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts.Add(Vertices[i] + Position);
        }
        
        _vertVbo = new VBO(_transformedVerts);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        
        _ibo = new IBO(Indices);
    }

    public override void Delete()
    {
        _textureVbo.Delete();
        
        base.Delete();
    }

    private readonly Vector3[][] _sizeOffset =
    [
        [new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)],
        [new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1)],
        [new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1)],
        [new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 0, 1)],
        [new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)],
        [new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1)]
    ];
}