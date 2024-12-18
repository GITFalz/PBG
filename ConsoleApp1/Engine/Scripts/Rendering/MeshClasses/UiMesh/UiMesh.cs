using OpenTK.Mathematics;

public class UiMesh : Mesh
{
    public List<int> TextUvs;
    public VBO _textUvVbo;
    
    public int elementCount = 0;
    
    public UiMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextUvs = new List<int>();
        
        transformedVertices = new List<Vector3>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 1, _textUvVbo);
    }

    public void MoveUiElement(int index, Vector3 position)
    {
        index *= 36;
        
        for (int i = 0; i < 36; i++)
        {
            transformedVertices[index + i] = Vertices[index + i] + position;
        }
        
        UpdateMesh();
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}