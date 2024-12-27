using OpenTK.Mathematics;

public class AnimationMesh : BoxMesh
{
    private VBO _textureVbo;
    
    public Vector3 WorldPosition = Vector3.Zero;
    public Quaternion LastRotation = Quaternion.Identity;
    
    public AnimationMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
    }

    /// <summary>
    /// !!!NOTE: This function needs to be called after generating the buffers
    /// </summary>
    public void Init()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Vertices[i];
        }
    }
        
    public void UpdateRotation(Quaternion rotation)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], new Vector3(0, 0, 0), rotation);
        }
    }
    
    public void UpdateRotation(Vector3 axis, float angle)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], new Vector3(0, 0, 0), axis, angle);
        }
    }
    
    public void UpdateRotation(Quaternion rotation, Vector3 pivot)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], pivot, rotation);
        }
    }

    public void UpdatePosition(Vector3 position)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] += position;
        }
    }
    
    public void Center()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] += WorldPosition;
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
            _transformedVerts.Add(Vertices[i]);
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
}