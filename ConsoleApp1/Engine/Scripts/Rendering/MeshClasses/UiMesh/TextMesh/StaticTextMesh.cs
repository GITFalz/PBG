using OpenTK.Mathematics;

public class StaticTextMesh : TextMesh
{
    public List<Vector2i> TextUvs;
    
    public VBO _textUvVbo;
    public TBO _textTbo;

    public List<int> chars = new List<int>();
    public int elementCount = 0;
    public int charCount = 0;
    
    public StaticTextMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextUvs = new List<Vector2i>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        _textTbo = new TBO(chars);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 2, _textUvVbo);
    }
    
    public override void AddQuad(Vector3 position, Quad quad)
    {
        base.AddQuad(position, quad);
        
        foreach(Vector2i uv in quad.TextureUvs)
        {
            TextUvs.Add(uv);
        }
    }
    
    public void MoveUiElement(int index, Vector3 position)
    {
        index *= 4;
        
        for (int i = 0; i < 4; i++)
        {
            transformedVertices[index + i] = Vertices[index + i] + position;
        }
        
        UpdateMesh();
    }

    public override void RenderMesh()
    {
        _textTbo.Bind();
        base.RenderMesh();
    }

    public override void UpdateMesh()
    {
        _textTbo.Update(chars);
        base.UpdateMesh();
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}