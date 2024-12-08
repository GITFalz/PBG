using OpenTK.Mathematics;

public class VoxelMesh : Mesh
{
    public List<int> TextureIndices;
    
    private VBO _textureVbo;
    
    public VoxelMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
    }

    public override void GenerateBuffers()
    {
        _textureVbo = new VBO(TextureIndices);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 1, _textureVbo);
    }

    public override void Delete()
    {
        _textureVbo.Delete();
        
        base.Delete();
    }
}