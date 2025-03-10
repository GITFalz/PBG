using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelMesh
{
    // Mesh
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO([(0, 0, 0)]);
    private VBO _uvVbo = new VBO([(0, 0)]);
    private VBO _textureVbo = new VBO([0]);
    private VBO _normalVbo = new VBO([(0, 0, 0)]);

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector3> _transformedVerts = new List<Vector3>();

    // Vertex
    private VAO _vertexVao = new VAO();
    private VBO _vertexVbo = new VBO([new Vector3(0, 0, 0)]);
    private VBO _vertexColorVbo = new VBO([new Vector3(0, 0, 0)]);

    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> VertexColors = new List<Vector3>();

    // Edge
    private VAO _edgeVao = new VAO();
    private VBO _edgeVbo = new VBO([(0, 0, 0)]);

    public List<Vector3> EdgeVertices = new List<Vector3>();


    public List<Vertex> VertexList = new List<Vertex>();
    public List<Edge> EdgeList = new List<Edge>();
    public List<Triangle> TriangleList = new List<Triangle>();
    
    public void Init()
    {
        Vertices.Clear();
        foreach (var v in VertexList)
        {
            Vertices.Add(v.Position);
        }

        EdgeVertices.Clear();
        foreach (var e in EdgeList)
        {
            EdgeVertices.Add(e.A.Position);
            EdgeVertices.Add(e.B.Position);
        }

        _transformedVerts.Clear();
        foreach (var t in TriangleList)
        {
            _transformedVerts.AddRange(t.GetVerticesPosition());
        }
    }

    public void ApplyMirror()
    {
        List<Vertex> currentVertices = [.. VertexList];
        List<Edge> currentEdges = [.. EdgeList];
        List<Triangle> currentTriangles = [.. TriangleList];
        
        Vector3[] flip = ModelSettings.Mirrors;
        bool[] swaps = ModelSettings.Swaps;

        for (int j = 1; j < flip.Length; j++)
        {
            int vertexIndex = currentVertices.Count * j;
            int edgeIndex = currentEdges.Count * j;

            for (int i = 0; i < currentVertices.Count; i++)
            {
                Vector3 position = currentVertices[i].Position * flip[j];
                Vertex vertex = new Vertex(position);

                if (VertexSharePosition(vertex.Position, out var v)) vertex = v;

                VertexList.Add(vertex);
                Vertices.Add(vertex.Position);
                VertexColors.Add(new Vector3(0f, 0f, 0f));
            }

            for (int i = 0; i < currentEdges.Count; i++)
            {
                int indexA = VertexList.IndexOf(currentEdges[i].A) + vertexIndex;
                int indexB = VertexList.IndexOf(currentEdges[i].B) + vertexIndex;

                Edge edge = new Edge(VertexList[indexA], VertexList[indexB]);
                EdgeList.Add(edge);
            }

            for (int i = 0; i < currentTriangles.Count; i++)
            {
                int indexA = VertexList.IndexOf(currentTriangles[i].A) + vertexIndex;
                int indexB = VertexList.IndexOf(currentTriangles[i].B) + vertexIndex;
                int indexC = VertexList.IndexOf(currentTriangles[i].C) + vertexIndex;

                int edgeIndexAB = EdgeList.IndexOf(currentTriangles[i].AB) + edgeIndex;
                int edgeIndexBC = EdgeList.IndexOf(currentTriangles[i].BC) + edgeIndex;
                int edgeIndexCA = EdgeList.IndexOf(currentTriangles[i].CA) + edgeIndex;

                if (swaps[j])
                {
                    (indexA, indexB) = (indexB, indexA);
                }

                Triangle triangle = new Triangle(VertexList[indexA], VertexList[indexB], VertexList[indexC], EdgeList[edgeIndexAB], EdgeList[edgeIndexBC], EdgeList[edgeIndexCA]);
                
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

    public void AddVertices(List<Vertex> vertices)
    {
        foreach (var vertex in vertices)
        {
            if (!VertexList.Contains(vertex))
            {
                VertexList.Add(vertex);
                Vertices.Add(vertex.Position);
                VertexColors.Add(new Vector3(0f, 0f, 0f));
            }
        }

        UpdateMesh();
    }

    public void AddTriangle(Triangle triangle)
    {
        if (!AddTriangleSimple(triangle))
            return;

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

        Indices.Add((uint)Indices.Count + 0);
        Indices.Add((uint)Indices.Count + 1);
        Indices.Add((uint)Indices.Count + 2);
    }

    public bool AddTriangleSimple(Triangle triangle)
    {
        if (TriangleList.Contains(triangle))
            return false;

        triangle.UpdateNormals();
        TriangleList.Add(triangle);
       
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);
        Normals.Add(triangle.Normal);

        TextureIndices.Add(0);
        TextureIndices.Add(0);
        TextureIndices.Add(0);

        return true;
    }

    public void RemoveTriangle(Triangle triangle)
    {
        
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
        if (vertices.Count < 2)
            return;

        HashSet<Edge> edgesToRemove = new HashSet<Edge>();
        HashSet<Triangle> trianglesToRemove = new HashSet<Triangle>();

        for (int i = 1; i < vertices.Count; i++)
        {
            vertices[0].Position += vertices[i].Position;
            vertices[i].ReplaceWith(vertices[0], edgesToRemove, trianglesToRemove);
            VertexList.Remove(vertices[i]);
        }

        foreach (var edge in edgesToRemove)
        {
            EdgeList.Remove(edge);
        }

        foreach (var triangle in trianglesToRemove)
        {
            TriangleList.Remove(triangle);
        }

        vertices[0].Position /= vertices.Count;

        Init();
        GenerateBuffers();
        RecalculateNormals();
        UpdateMesh();
    }

    public void ChangeVertexTo(Vertex oldVertex, Vertex newVertex)
    {
        foreach (var triangle in oldVertex.ParentTriangles)
        {
            triangle.SetVertexTo(oldVertex, newVertex);
        }
    }


    public void CheckUselessEdges()
    {
        List<Edge> edges = [.. EdgeList];

    }

    public void CheckUselessTriangles()
    {
        List<Triangle> triangles = [.. TriangleList];
        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle triangle = triangles[i];
            if (!VertexList.Contains(triangle.A) || !VertexList.Contains(triangle.B) || !VertexList.Contains(triangle.C))
            {
                TriangleList.Remove(triangle);
                triangle.A.ParentTriangles.Remove(triangle);
                triangle.B.ParentTriangles.Remove(triangle);
                triangle.C.ParentTriangles.Remove(triangle);
                triangle.AB.ParentTriangles.Remove(triangle);
                triangle.BC.ParentTriangles.Remove(triangle);
                triangle.CA.ParentTriangles.Remove(triangle);
            }
        }
    }

    public void UpdateMesh()
    {
        _normalVbo.Update(Normals);
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);

        _vertexVbo.Update(Vertices);
        _vertexColorVbo.Update(VertexColors);

        _edgeVbo.Update(EdgeVertices);
    }

    public void UpdateVertexColors()
    {
        _vertexColorVbo.Update(VertexColors);
    }

    public void SaveModel(string modelName)
    {
        SaveModel(modelName, Game.modelPath);
    }

    public void SaveModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName}.model");
        List<string> lines = new List<string>();

        lines.Add(VertexList.Count.ToString());
        foreach (var vertex in VertexList)
        {
            lines.Add($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z}");
        }
    
        lines.Add(EdgeList.Count.ToString());
        foreach (var edge in EdgeList)
        {
            lines.Add($"e {VertexList.IndexOf(edge.A)} {VertexList.IndexOf(edge.B)}");
        }

        lines.Add(TriangleList.Count.ToString());
        foreach (var triangle in TriangleList)
        {
            lines.Add($"f {VertexList.IndexOf(triangle.A)} {VertexList.IndexOf(triangle.B)} {VertexList.IndexOf(triangle.C)} {EdgeList.IndexOf(triangle.AB)} {EdgeList.IndexOf(triangle.BC)} {EdgeList.IndexOf(triangle.CA)}");
        }

        lines.Add(Uvs.Count.ToString());
        foreach (var uv in Uvs)
        {
            lines.Add($"uv {uv.X} {uv.Y}");
        }
        
        File.WriteAllLines(path, lines);
    }

    public bool LoadModel(string modelName)
    {
        return LoadModel(modelName, Game.modelPath);
    }

    public bool LoadModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName.Trim()}.model");
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("The model does not exist.");
            return false;
        }

        VertexList.Clear();
        TriangleList.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextureIndices.Clear();
        Normals.Clear();
        _transformedVerts.Clear();
        VertexColors.Clear();
        Vertices.Clear();
        EdgeVertices.Clear();
        EdgeList.Clear();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int triangleCount = int.Parse(lines[vertexCount + edgeCount + 2]);
        int uvCount = int.Parse(lines[vertexCount + edgeCount + triangleCount + 3]);

        for (int i = 1; i <= vertexCount; i++)
        {
            string[] values = lines[i].Split(' ');
            VertexList.Add(new Vertex(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]))));
            VertexColors.Add(new Vector3(0f, 0f, 0f));
        }

        for (int i = vertexCount + 2; i <= vertexCount + edgeCount + 1; i++)
        {
            string[] values = lines[i].Split(' ');
            Edge edge = new Edge(VertexList[int.Parse(values[1])], VertexList[int.Parse(values[2])]);
            EdgeList.Add(edge);
        }

        for (int i = vertexCount + edgeCount + 3; i <= vertexCount + edgeCount + triangleCount + 2; i++)
        {
            string[] values = lines[i].Split(' ');
            Triangle triangle = new Triangle(VertexList[int.Parse(values[1])], VertexList[int.Parse(values[2])], VertexList[int.Parse(values[3])], EdgeList[int.Parse(values[4])], EdgeList[int.Parse(values[5])], EdgeList[int.Parse(values[6])]);
            AddTriangleSimple(triangle);
        }

        for (int i = vertexCount + edgeCount + triangleCount + 4; i <= vertexCount + edgeCount + triangleCount + uvCount + 3; i++)
        {
            string[] values = lines[i].Split(' ');
            Uvs.Add(new Vector2(float.Parse(values[1]), float.Parse(values[2])));
        }

        Init();
        GenerateBuffers();

        return true;
    }
    
    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _vertVbo = new VBO(_transformedVerts);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        _normalVbo = new VBO(Normals);

        _vertexVbo = new VBO(Vertices);
        _vertexColorVbo = new VBO(VertexColors);

        _edgeVbo = new VBO(EdgeVertices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);

        _vertexVao.LinkToVAO(0, 3, _vertexVbo);
        _vertexVao.LinkToVAO(1, 3, _vertexColorVbo);

        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
        
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

    public void RenderVertices()
    {
        GL.Enable(EnableCap.PointSprite);
        GL.Enable(EnableCap.ProgramPointSize);

        _vertexVao.Bind();

        GL.PointSize(10.0f);
        GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Count);

        _vertexVao.Unbind();

        GL.Disable(EnableCap.PointSprite);
        GL.Disable(EnableCap.ProgramPointSize);
    }

    public void RenderEdges()
    {
        _edgeVao.Bind();

        GL.LineWidth(2.0f);
        GL.DrawArrays(PrimitiveType.Lines, 0, EdgeVertices.Count);

        _edgeVao.Unbind();
    }

    public void Delete()
    {
        _textureVbo.Delete();
    }
}