using OpenTK.Mathematics;

public class ModelMesh : BoxMesh
{
    private VBO _textureVbo;
    
    public Vector3 WorldPosition = Vector3.Zero;
    public Quaternion LastRotation = Quaternion.Identity;
    
    private bool print = true;

    private int _modelCount = 0;
    
    public List<Vertex> VertexList = new List<Vertex>();
    
    public List<Vector3> Normals = new List<Vector3>();
    public VBO _normalVbo;
    
    private bool smoothShading = true;
    
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
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);
        
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

    public void UpdateNormals(Triangle triangle)
    {
        triangle.UpdateNormals();
        
        int indexA = VertexList.IndexOf(triangle.A);
        int indexB = VertexList.IndexOf(triangle.B);
        int indexC = VertexList.IndexOf(triangle.C);
        
        Normals[indexA] = triangle.Normal;
        Normals[indexB] = triangle.Normal;
        Normals[indexC] = triangle.Normal;
    }

    public void RecalculateNormals()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>(); 
        for (int i = 0; i < VertexList.Count; i++)
        {
            Vertex vertex = VertexList[i];
            if (vertex.ParentTriangle != null)
            {
                Triangle triangle = vertex.ParentTriangle;
                triangles.Add(triangle);
                triangle.UpdateNormals();
            }
            Normals[i] = vertex.GetNormal();
        }
    }

    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < VertexList.Count; i++)
        {
            Indices.Add(i);
        }
    }

    public void SmoothShading()
    {
        for (int i = 0; i < VertexList.Count; i++)
        {
            Vertex vertex = VertexList[i];
            Normals[i] = vertex.GetAverageNormal();
        }
    }

    public void CheckUselessTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
        HashSet<Triangle> toDelete = new HashSet<Triangle>();
        
        foreach (var vertex in VertexList)
        {
            if (vertex.ParentTriangle == null || triangles.Contains(vertex.ParentTriangle))
                continue;
            
            if (vertex.ParentTriangle.TwoVertSamePosition())
                toDelete.Add(vertex.ParentTriangle);
        }
        
        foreach (var triangle in toDelete)
        {
            RemoveTriangle(triangle);
        }
    }

    public void CombineDuplicateVertices()
    {
        CombineDuplicateVertices(VertexList);
    }
    
    public void CombineDuplicateVertices(List<Vertex> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex1 = vertices[i];
            
            for (int j = i + 1; j < vertices.Count; j++)
            {
                Vertex vertex2 = vertices[j];
                if (vertex1.Position == vertex2.Position)
                {
                    vertex1.AddSharedVertexToAll(vertex1.ToList(), vertex2);
                }
            }
        }
    }

    public void RemoveVertex(Vertex vertex)
    {
        if (VertexList.Contains(vertex))
        {
            int index = VertexList.IndexOf(vertex);
            VertexList[index].RemoveInstanceFromAll();
            VertexList.Remove(vertex);
        }
    }

    public void RemoveTriangle(Triangle triangle)
    {
        if (Indices.Count < 3)
            return;
            
        RemoveVertex(triangle.A);
        RemoveVertex(triangle.B);
        RemoveVertex(triangle.C);
        
        Indices.RemoveRange(Indices.Count - 3, 3);
    }
    
    

    public override void UpdateMesh()
    {
        _normalVbo.Update(Normals);
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
    
    public void SaveModel(string modelName)
    {
        List<string> lines = new List<string>();

        string path = Path.Combine(Game.modelPath, $"{modelName}.model");
        
        lines.Add(VertexList.Count.ToString());
        foreach (var vertex in VertexList)
        {
            lines.Add(vertex.Position.ToString());
        }
        
        lines.Add(Uvs.Count.ToString());
        foreach (var uv in Uvs)
        {
            lines.Add(uv.ToString());
        }
        
        lines.Add((Normals.Count / 3).ToString());
        for (int i = 0; i < Normals.Count; i += 3)
        {
            lines.Add(Normals[i].ToString());
        }
        
        lines.Add((TextureIndices.Count / 3).ToString());
        for (int i = 0; i < TextureIndices.Count; i += 3)
        {
            lines.Add(TextureIndices[i].ToString());
        }
        
        File.WriteAllLines(path, lines);
    }

    public bool LoadModel(string modelName)
    {
        string path = Path.Combine(Game.modelPath, $"{modelName}.model");
        if (!File.Exists(path))
            return false;

        string[] lines = File.ReadAllLines(path);

        int vertCount = int.Parse(lines[0]);

        int uvIndex = vertCount + 1;
        int uvCount = int.Parse(lines[uvIndex]);

        int normalIndex = vertCount + uvCount + 2;
        int normalCount = int.Parse(lines[normalIndex]);

        int textureIndex = vertCount + uvCount + normalCount + 3;
        int textureCount = int.Parse(lines[textureIndex]);

        VertexList = new List<Vertex>();
        Uvs = new List<Vector2>();
        Normals = new List<Vector3>();
        TextureIndices = new List<int>();

        for (int i = 0; i < vertCount; i+=3)
        {
            Vertex A = new Vertex(UiLoader.TextToVector3(lines[i + 1]));
            Vertex B = new Vertex(UiLoader.TextToVector3(lines[i + 2]));
            Vertex C = new Vertex(UiLoader.TextToVector3(lines[i + 3]));
            new Triangle(A, B, C);
            VertexList.Add(A);
            VertexList.Add(B);
            VertexList.Add(C);
        }
        
        for (int i = 0; i < uvCount; i++)
        {
            Uvs.Add(UiLoader.TextToVector2(lines[i+uvIndex+1]));
        }
        
        for (int i = 0; i < normalCount; i++)
        {
            int index = i + normalIndex + 1;
            Normals.Add(UiLoader.TextToVector3(lines[index]));
            Normals.Add(UiLoader.TextToVector3(lines[index]));
            Normals.Add(UiLoader.TextToVector3(lines[index]));
        }
        
        for (int i = 0; i < textureCount; i++)
        {
            int index = i + textureIndex + 1;
            TextureIndices.Add(int.Parse(lines[index]));
            TextureIndices.Add(int.Parse(lines[index]));
            TextureIndices.Add(int.Parse(lines[index]));
        }

        CombineDuplicateVertices(VertexList);

        return true;
    }
    
    public override void GenerateBuffers()
    {
        foreach (var t in VertexList)
        {
            _transformedVerts.Add(t.Position);
        }
        
        GenerateIndices();
        
        _vertVbo = new VBO(_transformedVerts);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        _normalVbo = new VBO(Normals);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);
        
        _ibo = new IBO(Indices);
    }

    public override void Delete()
    {
        _textureVbo.Delete();
        
        base.Delete();
    }
}