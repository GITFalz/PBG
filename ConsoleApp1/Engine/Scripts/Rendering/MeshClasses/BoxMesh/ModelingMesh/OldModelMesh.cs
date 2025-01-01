using OpenTK.Mathematics;

public class OldModelMesh : BoxMesh
{
    private VBO _textureVbo;
    
    public Vector3 WorldPosition = Vector3.Zero;
    public Quaternion LastRotation = Quaternion.Identity;
    
    private bool print = true;

    private int _modelCount = 0;
    
    public OldModelMesh()
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
        _transformedVerts = new List<Vector3>(Vertices.Count);

        foreach (var t in Vertices)
        {
            _transformedVerts.Add(t);
        }
    }
    
    public void Center()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] += WorldPosition;
      
   
        }
    }
    
    public void AddModelNode(ModelNode node)
    {
        _modelCount++;
        
        foreach (var quad in node.Quads)
        {
            AddQuad(quad);
        }
    }

    public void RemoveModelNode(int index)
    {
        if (index >= _modelCount) 
            throw new IndexOutOfRangeException("Index out of range");
        
        _modelCount--;
        
        int startIndex = index * 24;
        
        for (int i = 0; i < 24; i++)
        {
            Vertices.RemoveAt(startIndex);
            TextureIndices.RemoveAt(startIndex);
            Uvs.RemoveAt(startIndex);
        }

        for (int i = 0; i < 36; i++)
        {
            Indices.RemoveAt(Indices.Count - 1);
        }
    }

    public void UpdateNode(int index, ModelNode node)
    {
        index *= 24;
        
        for (int i = 0; i < 6; i++)
        {
            UpdateQuad(index, i, node.Quads[i], node.Offsets);
        }
    }

    public void AddQuad(OldModelQuad quad)
    {
        int index = Vertices.Count;
        
        Indices.Add((uint)index);
        Indices.Add((uint)index + 1);
        Indices.Add((uint)index + 2);
        Indices.Add((uint)index + 2);
        Indices.Add((uint)index + 3);
        Indices.Add((uint)index);
        
        Uvs.Add(new Vector2(0, 0));
        Uvs.Add(new Vector2(0, 1));
        Uvs.Add(new Vector2(1, 1));
        Uvs.Add(new Vector2(1, 0));
        
        TextureIndices.Add(0);
        TextureIndices.Add(0);
        TextureIndices.Add(0);
        TextureIndices.Add(0);
        
        foreach (var vertex in quad.Vertices)
        {
            Vertices.Add(vertex - new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    public void ChangeModelColor(ref int index, int color)
    {
        index %= _modelCount;
        int newIndex = index * 24;
        
        for (int i = 0; i < 24; i++)
        {
            TextureIndices[newIndex + i] = color;
        }
    }
    
    public void UpdateQuad(int pos, int index, OldModelQuad quad, Vector3[] Offsets)
    {
        int vertIndex = index * 4 + pos;
        
        for (int i = 0; i < 4; i++)
        {
            Vertices[vertIndex + i] = quad.Vertices[i] + Offsets[_offsets[index][i]] - new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
    
    public void UpdateRotation(Quaternion rotation)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], new Vector3(0, 0, 0), rotation);  
        }
    }

    public override void UpdateMesh()
    {
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);
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

    private readonly Vector4i[] _offsets =
    [
        (0, 3, 2, 1),
        (1, 2, 6, 5),
        (3, 7, 6, 2),
        (4, 7, 3, 0),
        (1, 5, 4, 0),
        (5, 6, 7, 4),
    ];
}