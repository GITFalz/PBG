using OpenTK.Mathematics;

public class AnimationMesh : BoxMesh
{
    private VBO _textureVbo;

    public List<Vector3> Sizes;
    public List<Vector3> Rotation;
    public List<Vector3> Position;
    
    public Vector3 basePosition = Vector3.Zero;
    
    public AnimationMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
        
        Sizes = new List<Vector3>();
        Rotation = new List<Vector3>();
        Position = new List<Vector3>();
    }

    /// <summary>
    /// !!!NOTE: This function NEEDS to be called before scaling or rotating the mesh!!!
    /// </summary>
    public void UpdatePosition(Vector3 position)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Vertices[i] + position;
        }
    }
    
    /// <summary>
    /// !!!NOTE: This function NEEDS to be called before scaling or rotating the mesh!!!
    /// </summary>
    public void UpdatePosition()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int PositionIndex = i / 24;
            _transformedVerts[i] += Position[PositionIndex] + basePosition;
        }
    }
    
    /// <summary>
    /// NOTE: This function NEEDS to be called after positioning and before rotating!!!
    /// </summary>
    /// <param name="size"></param>
    public void UpdateScale()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int sizeIndex = i / 24;
            _transformedVerts[i] = Vertices[i] * (Mathf.Abs(Sizes[sizeIndex]));
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
            _transformedVerts[i] += Vertices[i] * size;
        }
    }

    /// <summary>
    /// NOTE: This function NEEDS to be called after positioning and scaling!!!
    /// </summary>
    /// <param name="center"></param>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    public void UpdateRotation(Vector3 axis, float angle)
    {
        UpdateScale();
        
        for (int i = 0; i < Vertices.Count; i++)
        {
            int rotationIndex = i / 24;
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], Rotation[rotationIndex], axis, angle);
        }
        
        UpdatePosition();
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
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        
        _ibo = new IBO(Indices);
    }

    public override void Delete()
    {
        _textureVbo.Delete();
        
        base.Delete();
    }
}