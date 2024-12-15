using OpenTK.Mathematics;

public class EntityMesh : BoxMesh
{
    private VBO _textureVbo;

    public Vector3 Position;
    public Vector3 Size;
    
    public EntityMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
    }

    public void UpdatePosition()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Vertices[i] + Position;
        }
    }

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
}