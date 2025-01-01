using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class TextMesh : Mesh
{
    public List<Vector2i> TextUvs;
    
    public VBO _textUvVbo;
    public TBO _textTbo;

    public List<int> chars;
    public int CharCount;
    
    public TextMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>(4);
        Uvs = new List<Vector2>(4);
        Indices = new List<uint>(6);
        TextUvs = new List<Vector2i>(4);
        
        transformedVertices = new List<Vector3>();
        
        chars = new List<int>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        _textTbo = new TBO(chars);
        CharCount = chars.Count;
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 2, _textUvVbo);
    }
    
    public void SetQuad(Vector3 position, MeshQuad meshQuad)
    {
        Vertices.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextUvs.Clear();
        transformedVertices.Clear();
        chars.Clear();
        
        foreach (Vector3 v in meshQuad.Vertices)
        {
            Vertices.Add(v + position);
            transformedVertices.Add(v + position);
        }
        
        foreach (int i in meshQuad.Indices)
        {
            Indices.Add((uint)i);
        }
        
        foreach (Vector2 uv in meshQuad.Uvs)
        {
            Uvs.Add(uv);
        }
        
        foreach (Vector2i uv in meshQuad.TextureUvs)
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
    }

    public override void UpdateMesh()
    {
        if (chars.Count != CharCount)
        {
            _textTbo = new TBO(chars);
            CharCount = chars.Count;
            _textUvVbo.Update(TextUvs);
        }
        _textTbo.Update(chars);
        base.UpdateMesh();
    }
    
    public override void RenderMesh()
    {
        _textTbo.Bind(TextureUnit.Texture1);
        base.RenderMesh();
        _textTbo.Unbind();
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }

    public override void Clear()
    {
        base.Clear();
        
        TextUvs.Clear();
        chars.Clear();
    }
}