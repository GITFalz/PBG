using OpenTK.Mathematics;

public class DynamicTextMesh : TextMesh
{
    public List<Vector2i> TextUvs;
    
    public VBO _textUvVbo;
    public TBO _textTbo;

    public List<int> chars = new List<int>();
    
    public DynamicTextMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>(4);
        Uvs = new List<Vector2>(4);
        Indices = new List<uint>(6);
        TextUvs = new List<Vector2i>(4);
        
        transformedVertices = new List<Vector3>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        _textTbo = new TBO(chars);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 2, _textUvVbo);
    }
    
    public void SetQuad(Vector3 position, Quad quad)
    {
        Vertices.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextUvs.Clear();
        transformedVertices.Clear();
        
        foreach (Vector3 v in quad.Vertices)
        {
            Vertices.Add(v + position);
            transformedVertices.Add(v + position);
        }
        
        foreach (int i in quad.Indices)
        {
            Indices.Add((uint)i);
        }
        
        foreach (Vector2 uv in quad.Uvs)
        {
            Uvs.Add(uv);
        }
        
        foreach (Vector2i uv in quad.TextureUvs)
        {
            TextUvs.Add(uv);
        }
    }
    
    public void MoveUiElement(Vector3 position)
    {
        for (int i = 0; i < 4; i++)
        {
            transformedVertices[i] = Vertices[i] + position;
        }

        UpdateMesh();
    }

    public override void UpdateMesh()
    {
        _textTbo.Update(chars);
        base.UpdateMesh();
    }

    public override void RenderMesh()
    {
        _textTbo.Bind();
        base.RenderMesh();
    }

    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}