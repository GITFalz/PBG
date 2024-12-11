using ConsoleApp1.Engine.Scripts.Core.Voxel;
using OpenTK.Mathematics;

public class EntityMesh : Mesh
{
    public List<int> TextureIndices;
    
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
    }
    
    public void GenerateMesh(Vector3 position, Vector3 size, int textureIndex)
    {
        Position = position;
        Size = size;

        VoxelData.GetEntityBoxMesh(this, size, position, textureIndex);
    }

    public override void UpdateMesh()
    {
        foreach (var vertex in Vertices)
        {
            
        }
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