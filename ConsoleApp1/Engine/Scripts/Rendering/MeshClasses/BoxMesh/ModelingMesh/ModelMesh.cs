using OpenTK.Mathematics;

public class ModelMesh : BoxMesh
{
    private VBO _textureVbo;
    
    public Vector3 WorldPosition = Vector3.Zero;
    public Quaternion LastRotation = Quaternion.Identity;
    
    private bool print = true;

    private int _modelCount = 0;
    
    public List<Vertex> VertexList = new List<Vertex>();
    
    public ModelMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
    }
    
    public void Init()
    {
        _transformedVerts = new List<Vector3>(VertexList.Count);

        foreach (var t in VertexList)
        {
            _transformedVerts.Add(t.Position);
            //Console.WriteLine(t.Position);
        }
    }
    
    public void Center()
    {
        for (int i = 0; i < VertexList.Count; i++)
        {
            _transformedVerts[i] += WorldPosition;
        }
    }

    public void ChangeModelColor(ref int index, int color)
    {
        index %= _modelCount;
        int newIndex = index * 24;
        
        for (int i = 0; i < 24; i++)
        {
            TextureIndices[newIndex + i] = color;
        }
    }

    public void AddTriangle(Triangle triangle)
    {
        Indices.Add((uint)VertexList.Count);
        Indices.Add((uint)VertexList.Count + 1);
        Indices.Add((uint)VertexList.Count + 2);
        
        VertexList.Add(triangle.A);
        VertexList.Add(triangle.B);
        VertexList.Add(triangle.C);
        
        Uvs.Add((0, 0));
        Uvs.Add((0, 1));
        Uvs.Add((1, 1));
        
        TextureIndices.Add(0);
        TextureIndices.Add(0);
        TextureIndices.Add(0);
    }
    
    public bool SwapVertices(Vertex A, Vertex B)
    {
        if (!VertexList.Contains(A) || !VertexList.Contains(B)) 
            return false;
        
        int indexA = VertexList.IndexOf(A);
        int indexB = VertexList.IndexOf(B);
            
        VertexList[indexA] = B;
        VertexList[indexB] = A;
            
        return true;

    }
    
    public void UpdateRotation(Quaternion rotation)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts[i] = Mathf.RotateAround(_transformedVerts[i], new Vector3(0, 0, 0), rotation);  
        }
    }

    public override void UpdateMesh()
    {
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);
    }
    
    public void ResetVertex()
    {
        foreach (var t in VertexList)
        {
            t.WentThrough = false;
        }
    }
    
    public override void GenerateBuffers()
    {
        foreach (var t in VertexList)
        {
            _transformedVerts.Add(t.Position);
        }
        
        _vertVbo = new VBO(_transformedVerts);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        
        _ibo = new IBO(Indices);
    }

    public override void Delete()
    {
        _textureVbo.Delete();
        
        base.Delete();
    }
}