using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelMesh
{
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO([(0, 0, 0)]);
    private VBO _uvVbo = new VBO([(0, 0)]);
    private VBO _textureVbo = new VBO([0]);
    private VBO _normalVbo = new VBO([(0, 0, 0)]);
    
    public List<Vertex> VertexList = new List<Vertex>();
    public List<Triangle> TriangleList = new List<Triangle>();

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector3> _transformedVerts = new List<Vector3>();
    
    public void Init()
    {
        _transformedVerts = new List<Vector3>(TriangleList.Count * 3);

        foreach (var t in TriangleList)
        {
            _transformedVerts.AddRange(t.GetVerticesPosition());
        }
    }

    public void ApplyMirror()
    {
        List<Triangle> currentTriangles = [.. TriangleList];
        
        Vector3[] flip = ModelSettings.Mirrors;
        bool[] swaps = ModelSettings.Swaps;

        for (int j = 1; j < flip.Length; j++)
        {
            for (int i = 0; i < currentTriangles.Count; i++)
            {
                Vector3 aPosition = currentTriangles[i].A.Position * flip[j];
                Vector3 bPosition = currentTriangles[i].B.Position * flip[j];
                Vector3 cPosition = currentTriangles[i].C.Position * flip[j];

                if (swaps[j])
                {
                    (aPosition, bPosition) = (bPosition, aPosition);
                }

                Vertex A = new Vertex(aPosition);
                Vertex B = new Vertex(bPosition);
                Vertex C = new Vertex(cPosition);

                if (VertexSharePosition(A.Position, out var a)) A = a;
                if (VertexSharePosition(B.Position, out var b)) B = b;
                if (VertexSharePosition(C.Position, out var c)) C = c;

                Triangle triangle = new Triangle(A, B, C);
                
                AddTriangle(triangle);
            }
        }

        RecalculateNormals();
    }

    public bool VertexSharePosition(Vector3 position, out Vertex vertex)
    {
        vertex = new Vertex(Vector3.Zero);
        foreach (var v in VertexList)
        {
            if (v.Position == position)
            {
                vertex = v;
                return true;
            }
        }
        return false;
    }

    public void AddTriangle(Triangle triangle)
    {
        if (TriangleList.Contains(triangle))
            return;

        TriangleList.Add(triangle);
       
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);

        var vertices = triangle.GetVertices();

        if (!VertexList.Contains(vertices[0]))
            VertexList.Add(vertices[0]);
        if (!VertexList.Contains(vertices[1]))
            VertexList.Add(vertices[1]);
        if (!VertexList.Contains(vertices[2]))
            VertexList.Add(vertices[2]);

        Uvs.Add((0, 0));
        Uvs.Add((0, 1));
        Uvs.Add((1, 1));
        
        TextureIndices.Add(0);
        TextureIndices.Add(0);
        TextureIndices.Add(0);

        GenerateIndices();
    }

    public void RemoveTriangle(Triangle triangle)
    {
        if (Indices.Count < 3 || !TriangleList.Contains(triangle))
            return;

        int index = TriangleList.IndexOf(triangle) * 3;
            
        RemoveVertex(triangle.A);
        RemoveVertex(triangle.B);
        RemoveVertex(triangle.C);
        
        GenerateIndices();
        for (int i = 0; i < 3; i++)
        {
            Uvs.RemoveAt(index);
            Normals.RemoveAt(index);
            TextureIndices.RemoveAt(index);
        }
        TriangleList.Remove(triangle);
    }

    public void RemoveVertex(Vertex vertex)
    {
        if (VertexCount(vertex) == 1)
            VertexList.Remove(vertex);
    }

    public int VertexCount(Vertex vertex)
    {
        int count = 0;
        foreach (var t in TriangleList)
        {
            if (t.A == vertex || t.B == vertex || t.C == vertex)
                count++;
        }
        return count;
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
        int index = TriangleList.IndexOf(triangle) * 3;

        triangle.UpdateNormals();
        
        Normals[index+0] = triangle.Normal;
        Normals[index+1] = triangle.Normal;
        Normals[index+2] = triangle.Normal;
    }

    public void RecalculateNormals()
    {
        for (int i = 0; i < TriangleList.Count; i++)
        {
            UpdateNormals(TriangleList[i]);
        }
    }

    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < TriangleList.Count * 3; i++)
        {
            Indices.Add(i);
        }
    }

    public void MergeVertices(List<Vertex> vertices)
    {

        Vertex newVertex = new Vertex(Vector3.Zero);
        int count = 0;

        foreach (var vertex in vertices)
        {
            if (!VertexList.Contains(vertex))
                continue;

            newVertex.Position += vertex.Position;
            ChangeVertexTo(vertex, newVertex);
            RemoveVertex(vertex);
            count++;
        }

        newVertex.Position /= count;
        VertexList.Add(newVertex);
    }

    public void ChangeVertexTo(Vertex oldVertex, Vertex newVertex)
    {
        foreach (var triangle in oldVertex.ParentTriangles)
        {
            triangle.SetVertexTo(oldVertex, newVertex);
        }
    }

    public void UpdateMesh()
    {
        _normalVbo.Update(Normals);
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);
    }
    
    public void SaveModel(string modelName)
    {
        List<string> lines = new List<string>();

        string path = Path.Combine(Game.modelPath, $"{modelName}.model");
        
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

        Uvs = new List<Vector2>();
        Normals = new List<Vector3>();
        TextureIndices = new List<int>();

        for (int i = 0; i < vertCount; i+=3)
        {
            Vector3 vectorA = UiLoader.TextToVector3(lines[i + 1]);
            Vector3 vectorB = UiLoader.TextToVector3(lines[i + 2]);
            Vector3 vectorC = UiLoader.TextToVector3(lines[i + 3]);

            Vertex A = new Vertex(vectorA);
            Vertex B = new Vertex(vectorB);
            Vertex C = new Vertex(vectorC);

            foreach (var vert in VertexList)
            {
                if (A.Position == vert.Position) A = vert;
                if (B.Position == vert.Position) B = vert;
                if (C.Position == vert.Position) C = vert;
            }
            
            AddTriangle(new Triangle(A, B, C));
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

        return true;
    }
    
    public void GenerateBuffers()
    {
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

    public void Render()
    {
        _vao.Bind();
        _ibo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

        _vao.Unbind();
        _ibo.Unbind();
    }

    public void Delete()
    {
        _textureVbo.Delete();
    }
}