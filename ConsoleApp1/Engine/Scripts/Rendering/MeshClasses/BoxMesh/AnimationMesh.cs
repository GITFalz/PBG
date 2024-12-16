using OpenTK.Mathematics;

public class AnimationMesh : BoxMesh
{
    private VBO _textureVbo;

    public List<Vector3> Sizes;
    public List<Vector3> Rotation;
    public List<Vector3> Position;

    public Vector3 Pivot = Vector3.Zero;
    public Vector3 WorldPosition = Vector3.Zero;
    
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
    /// !!!NOTE: This function needs to be called after generating the buffers
    /// </summary>
    public void Init()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Vertices[i];
        }
    }
    
    public void UpdatePosition()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int PositionIndex = i / 24;
            if (PositionIndex >= Position.Count)
                break;
            _transformedVerts[i] += Position[PositionIndex];
        }
    }
    
    public void UpdateScale()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int sizeIndex = i / 24;
            if (sizeIndex >= Sizes.Count)
                break;
            _transformedVerts[i] = Vertices[i] * (Mathf.Abs(Sizes[sizeIndex]));
        }
    }

    public void UpdateScale(Vector3 size)
    {
        size = Mathf.Abs(size);
        size -= Vector3.One;
        
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] += Vertices[i] * size;
        }
    }

    public void UpdateRotation(Vector3 axis, float angle)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int rotationIndex = i / 24;
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], Rotation[rotationIndex], axis, angle);
        }
    }
        
    public void UpdateRotation(Quaternion rotation)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            int rotationIndex = i / 24;
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], Rotation[rotationIndex], rotation);
        }
    }

    public void Center()
    {
        Console.WriteLine("Centering");
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