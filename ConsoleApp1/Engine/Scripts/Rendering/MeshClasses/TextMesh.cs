using OpenTK.Mathematics;

public class TextMesh : Mesh
{
    public List<Vector2i> TextUvs;
    
    public VBO _textUvVbo;

    public int[] chars = new int[256];
    
    public TextMesh()
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
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}