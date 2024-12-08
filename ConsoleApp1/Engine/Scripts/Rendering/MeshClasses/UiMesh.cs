using OpenTK.Mathematics;

public class UiMesh : Mesh
{
    public List<int> TextureIndexes;
    
    public VBO _textUvVbo;

    public int[] chars = new int[] { 0, 0, 0, 0, 0, 0 };
    
    public UiMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndexes = new List<int>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextureIndexes);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 2, _textUvVbo);
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}