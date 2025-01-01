using OpenTK.Mathematics;

public class VoxelMesh : BoxMesh
{
    public Vector3 WorldPosition = Vector3.Zero;
    
    public VBO _textUvVbo;

    private int _elementCount = 0;
    
    public VoxelMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        
        transformedVertices = new List<Vector3>();
        
        TextureIndices = new List<int>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextureIndices);
        
        transformedVertices.Clear();

        foreach (var t in Vertices)
        {
            transformedVertices.Add(t + Position);
        }
        
        _vertVbo = new VBO(transformedVertices);
        _uvVbo = new VBO(Uvs);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textUvVbo);
        
        _ibo = new IBO(Indices);
        
        _elementCount = Vertices.Count / 24;
    }
    
    public void SetVoxelColor(int index, int color)
    {
        index = Mathf.Clamp(0, _elementCount, index);
        index *= 24;
        
        for (int i = 0; i < 24; i++)
        {
            TextureIndices[index + i] = color;
        }
    }
    
    public void SetFaceColor(int voxelIndex, int faceIndex, int color)
    {
        voxelIndex = Mathf.Clamp(0, _elementCount, voxelIndex);
        voxelIndex *= 24;
        
        faceIndex = Mathf.Clamp(0, 5, faceIndex);
        int index = voxelIndex + faceIndex * 4;
        
        for (int i = 0; i < 4; i++)
        {
            TextureIndices[index + i] = color;
        }
    }
    
    public void ResetColor()
    {
        for (int i = 0; i < TextureIndices.Count; i++)
        {
            int index = i / 24;
            TextureIndices[i] = Mathf.Clamp(0, 1, index);
        }
    }
    
    
    public void Init()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            transformedVertices[i] = Vertices[i];
        }
    }
        
    public void UpdateRotation(Quaternion rotation)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            transformedVertices[i] = Mathf.RotateAround(transformedVertices[i], new Vector3(0, 0, 0), rotation);
        }
    }
    
    public void Center()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            transformedVertices[i] += WorldPosition;
        }
    }
    
    public override void UpdateMesh()
    {
        _vertVbo.Update(transformedVertices);
    }
    
    public void UpdateTextureIndices()
    {
        _textUvVbo.Update(TextureIndices);
    }
    
    
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}