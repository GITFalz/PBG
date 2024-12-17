using OpenTK.Mathematics;

public class UiMesh : Mesh
{
    public List<int> TextureIndexes;
    
    public VBO _textUvVbo;
    
    public UiMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndexes = new List<int>();
        
        transformedVertices = new List<Vector3>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextureIndexes);
        
        base.GenerateBuffers();

        _vao.LinkToVAO(2, 1, _textUvVbo);
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}