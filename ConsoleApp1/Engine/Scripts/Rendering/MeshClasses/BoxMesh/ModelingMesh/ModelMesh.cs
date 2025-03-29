using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelMesh : Meshes
{
    // Mesh
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new(new List<Vector3>());
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());
    private VBO<int> _textureVbo = new(new List<int>());
    private VBO<Vector3> _normalVbo = new(new List<Vector3>());

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector3> _transformedVerts = new List<Vector3>();

    // Vertex
    private VAO _vertexVao = new VAO();
    private VBO<Vector3> _vertexVbo = new(new List<Vector3>());
    private VBO<Vector3> _vertexColorVbo = new(new List<Vector3>());
    private VBO<float> _vertexSizeVbo = new(new List<float>());

    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> VertexColors = new List<Vector3>();
    public List<float> VertexSize = new List<float>();

    // Edge
    private VAO _edgeVao = new VAO();
    private VBO<Vector3> _edgeVbo = new(new List<Vector3>());
    private VBO<Vector3> _edgeColorVbo = new(new List<Vector3>());

    public List<Vector3> EdgeVertices = new List<Vector3>();
    public List<Vector3> EdgeColors = new List<Vector3>();


    public List<Vertex> VertexList = new List<Vertex>();
    public List<Edge> EdgeList = new List<Edge>();
    public List<Triangle> TriangleList = new List<Triangle>();
    
    public void Init()
    {
        Vertices.Clear();
        VertexColors.Clear();
        VertexSize.Clear();
        foreach (var v in VertexList)
        {
            Vertices.Add(v.Position);
            VertexColors.Add(v.Color);
            VertexSize.Add(10f);
        }

        EdgeVertices.Clear();
        EdgeColors.Clear();
        foreach (var e in EdgeList)
        {
            EdgeVertices.AddRange(e.A.Position, e.B.Position);
            EdgeColors.AddRange(e.A.Color, e.B.Color);
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

        if (!VertexList.Contains(triangle.A))
            VertexList.Add(triangle.A);
        if (!VertexList.Contains(triangle.B))
            VertexList.Add(triangle.B);
        if (!VertexList.Contains(triangle.C))
            VertexList.Add(triangle.C);

        if (!EdgeList.Contains(triangle.AB))
            EdgeList.Add(triangle.AB);
        if (!EdgeList.Contains(triangle.BC))
            EdgeList.Add(triangle.BC);
        if (!EdgeList.Contains(triangle.CA))
            EdgeList.Add(triangle.CA);

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

    public void AddCopy(ModelCopy copy)
    {
        foreach (var vertex in copy.newSelectedVertices)
        {
            if (!VertexList.Contains(vertex))
            {
                VertexList.Add(vertex);
                Vertices.Add(vertex.Position);
                VertexColors.Add(new Vector3(0f, 0f, 0f));
            }
        }

        foreach (var edge in copy.newSelectedEdges)
        {
            if (!EdgeList.Contains(edge))
            {
                EdgeList.Add(edge);
                EdgeVertices.AddRange(edge.A.Position, edge.B.Position);
                EdgeColors.AddRange(edge.A.Color, edge.B.Color);
            }
        }

        foreach (var triangle in copy.newSelectedTriangles)
        {
            AddTriangle(triangle);
        }
    }


    public void RemoveVertex(Vertex vertex)
    {
        int index = VertexList.IndexOf(vertex);
        if (index == -1)
            return;

        VertexList.Remove(vertex);
        Vertices.RemoveAt(index);
        VertexColors.RemoveAt(index);

        List<Triangle> triangles = [.. vertex.ParentTriangles];
        List<Edge> edges = [.. vertex.ParentEdges];

        foreach (var triangle in triangles)
        {
            triangle.Delete();
            TriangleList.Remove(triangle);
        }

        foreach (var edge in edges)
        {
            edge.Delete();
            EdgeList.Remove(edge);
        }
    }

    public void RemoveTriangle(Triangle triangle)
    {
        int index = TriangleList.IndexOf(triangle);
        if (index == -1)
            return;

        int tripleIndex = index * 3;

        Normals.RemoveRange(tripleIndex, 3);
        TextureIndices.RemoveRange(tripleIndex, 3);
        Uvs.RemoveRange(tripleIndex, 3);
        Indices.RemoveRange(Indices.Count - 3, 3);

        Vertex A = triangle.A;
        Vertex B = triangle.B;
        Vertex C = triangle.C;

        if (A.ParentTriangles.Count == 1) VertexList.Remove(A); else A.ParentTriangles.Remove(triangle);
        if (B.ParentTriangles.Count == 1) VertexList.Remove(B); else B.ParentTriangles.Remove(triangle);
        if (C.ParentTriangles.Count == 1) VertexList.Remove(C); else C.ParentTriangles.Remove(triangle);

        Edge AB = triangle.AB;
        Edge BC = triangle.BC;
        Edge CA = triangle.CA;

        if (AB.ParentTriangles.Count == 1) EdgeList.Remove(AB.Delete()); else Console.WriteLine("Removed triangle from edge AB: " + AB.ParentTriangles.Remove(triangle));
        if (BC.ParentTriangles.Count == 1) EdgeList.Remove(BC.Delete()); else Console.WriteLine("Removed triangle from edge BC: " + BC.ParentTriangles.Remove(triangle));
        if (CA.ParentTriangles.Count == 1) EdgeList.Remove(CA.Delete()); else Console.WriteLine("Removed triangle from edge CA: " + CA.ParentTriangles.Remove(triangle));

        TriangleList.Remove(triangle.Delete());
    }

    public void RemoveEdge(Edge edge)
    {
        int index = EdgeList.IndexOf(edge);
        if (index == -1)
            return;

        int doubleIndex = index * 2;

        EdgeVertices.RemoveRange(doubleIndex, 2);
        EdgeColors.RemoveRange(doubleIndex, 2);

        Vertex A = edge.A;
        Vertex B = edge.B;

        if (A.ParentEdges.Count == 1) VertexList.Remove(A); else A.ParentEdges.Remove(edge);
        if (B.ParentEdges.Count == 1) VertexList.Remove(B); else B.ParentEdges.Remove(edge);

        EdgeList.Remove(edge.Delete());
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
            edge.Delete();
            EdgeList.Remove(edge);
        }

        foreach (var triangle in trianglesToRemove)
        {
            triangle.Delete();
            TriangleList.Remove(triangle);
        }

        vertices[0].Position /= vertices.Count;

        Init();
        GenerateBuffers();
        RecalculateNormals();
        UpdateMesh();
    }


    public void CheckUselessEdges()
    {
        List<Edge> edges = [.. EdgeList];
        for (int i = 0; i < edges.Count; i++)
        {
            Edge edge = edges[i];
            if (!VertexList.Contains(edge.A) || !VertexList.Contains(edge.B))
            {
                edge.Delete();
                EdgeList.Remove(edge);
            }
        }
    }

    public void CheckUselessTriangles()
    {
        List<Triangle> triangles = [.. TriangleList];
        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle triangle = triangles[i];
            if (!VertexList.Contains(triangle.A) || !VertexList.Contains(triangle.B) || !VertexList.Contains(triangle.C) || !EdgeList.Contains(triangle.AB) || !EdgeList.Contains(triangle.BC) || !EdgeList.Contains(triangle.CA))
            {
                triangle.Delete();
                TriangleList.Remove(triangle);
            }
        }
    }

    public void CombineDuplicateVertices()
    {
        List<Vertex> vertices = [.. VertexList];
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex = vertices[i];
            for (int j = i + 1; j < vertices.Count; j++)
            {
                Vertex other = vertices[j];
                if (vertex.Position == other.Position)
                {
                    MergeVertices([vertex, other]);
                }
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

    public void UpdateVertices()
    {
        _vertVbo.Update(_transformedVerts);
        _vertexVbo.Update(Vertices);
        _edgeVbo.Update(EdgeVertices);
    }

    public void UpdateVertexColors()
    {
        _vertexColorVbo.Update(VertexColors);
    }

    public void UpdateEdgeColors()
    {
        _edgeColorVbo.Update(EdgeColors);
    }

    public override void SaveModel(string modelName)
    {
        SaveModel(modelName, Game.modelPath);
    }

    public override void SaveModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName}.model");
        if (!File.Exists(path)) File.WriteAllText(path, "0\n0\n0\n0\n0");
        List<string> oldLines = [.. File.ReadAllLines(path)];
        List<string> newLines = new List<string>();

        int oldVertexCount = int.Parse(oldLines[0]);
        int oldEdgeCount = int.Parse(oldLines[oldVertexCount + 1]);
        int oldTriangleCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + 2]);
        int oldTextureCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldTriangleCount + 3]);
        int oldNormalCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldTriangleCount + oldTextureCount + 4]);
        int rigStart = oldVertexCount + oldEdgeCount + oldTriangleCount + oldTextureCount + oldNormalCount + 5;

        newLines.Add(VertexList.Count.ToString());
        foreach (var vertex in VertexList)
        {
            newLines.Add($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z} {vertex.Index}");
        }
    
        newLines.Add(EdgeList.Count.ToString());
        foreach (var edge in EdgeList)
        {
            newLines.Add($"e {VertexList.IndexOf(edge.A)} {VertexList.IndexOf(edge.B)}");
        }

        newLines.Add(TriangleList.Count.ToString());
        foreach (var triangle in TriangleList)
        {
            newLines.Add($"f {VertexList.IndexOf(triangle.A)} {VertexList.IndexOf(triangle.B)} {VertexList.IndexOf(triangle.C)} {EdgeList.IndexOf(triangle.AB)} {EdgeList.IndexOf(triangle.BC)} {EdgeList.IndexOf(triangle.CA)}");
        }

        newLines.Add(Uvs.Count.ToString());
        foreach (var uv in Uvs)
        {
            newLines.Add($"uv {uv.X} {uv.Y}");
        }

        newLines.Add(Normals.Count.ToString());
        foreach (var normal in Normals)
        {
            newLines.Add($"n {normal.X} {normal.Y} {normal.Z}");
        }

        for (int i = rigStart; i < oldLines.Count; i++)
        {
            newLines.Add(oldLines[i]);
        }
        
        File.WriteAllLines(path, newLines);
    }

    public override bool LoadModel(string modelName)
    {
        return LoadModel(modelName, Game.modelPath);
    }

    public override bool LoadModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName.Trim()}.model");
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("The model does not exist.");
            return false;
        }

        Unload();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int triangleCount = int.Parse(lines[vertexCount + edgeCount + 2]);
        int uvCount = int.Parse(lines[vertexCount + edgeCount + triangleCount + 3]);

        for (int i = 1; i <= vertexCount; i++)
        {
            string[] values = lines[i].Split(' ');
            Vertex vertex = new Vertex(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
            vertex.Index = int.Parse(values[4]);
            VertexList.Add(vertex);
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

    public override void Unload()
    {
        VertexList.Clear();
        TriangleList.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextureIndices.Clear();
        Normals.Clear();
        _transformedVerts.Clear();
        VertexColors.Clear();
        VertexSize.Clear();
        Vertices.Clear();
        EdgeVertices.Clear();
        EdgeList.Clear();
    }
    
    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _vertVbo = new(_transformedVerts);
        _uvVbo = new(Uvs);
        _textureVbo = new(TextureIndices);
        _normalVbo = new(Normals);

        _vertexVbo = new(Vertices);
        _vertexColorVbo = new(VertexColors);
        _vertexSizeVbo = new(VertexSize);

        _edgeVbo = new(EdgeVertices);
        _edgeColorVbo = new(EdgeColors);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);

        _vertexVao.LinkToVAO(0, 3, _vertexVbo);
        _vertexVao.LinkToVAO(1, 3, _vertexColorVbo);
        _vertexVao.LinkToVAO(2, 1, _vertexSizeVbo);

        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
        _edgeVao.LinkToVAO(1, 3, _edgeColorVbo);
        
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
}