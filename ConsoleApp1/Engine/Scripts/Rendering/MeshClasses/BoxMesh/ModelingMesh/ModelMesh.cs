using System.Runtime.InteropServices;
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
    public struct VertexData
    {
        public Vector3 Position;
        public Vector4 Color;
        public float Size;
    }
    private VAO _vertexVao = new VAO();
    private VBO<VertexData> _vertexVbo = new(new List<VertexData>());
    public List<VertexData> Vertices = new List<VertexData>();

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
        foreach (var v in VertexList)
        {
            Vertices.Add(new VertexData{
                Position = v.Position,
                Color = new Vector4(v.Color.X, v.Color.Y, v.Color.Z, 1f),
                Size = 10f
            });
        }

        EdgeVertices.Clear();
        EdgeColors.Clear();
        foreach (var e in EdgeList)
        {
            EdgeVertices.AddRange(e.A, e.B);
            EdgeColors.AddRange(e.A.Color, e.B.Color);
        }

        _transformedVerts.Clear();
        Uvs.Clear();

        if (TriangleList.Count == 0)
            return;

        foreach (var t in TriangleList)
        {
            _transformedVerts.AddRange(t.GetVerticesPosition());
            Uvs.AddRange(t.GetUvs());
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
                Vector3 position = currentVertices[i] * flip[j];
                Vertex vertex = new Vertex(position);

                if (VertexSharePosition(vertex, out var v)) vertex = v;

                VertexList.Add(vertex);
                Vertices.Add(new VertexData
                {
                    Position = vertex.Position,
                    Color = new Vector4(vertex.Color.X, vertex.Color.Y, vertex.Color.Z, 1f),
                    Size = 10f
                });
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
            if (v == position)
            {
                vertex = v;
                return true;
            }
        }
        return false;
    }

    public void AddVertex(Vertex vertex, bool updateMesh = true)
    {
        if (!VertexList.Contains(vertex))
        {
            VertexList.Add(vertex);
            Vertices.Add(new VertexData
            {
                Position = vertex.Position,
                Color = new Vector4(vertex.Color.X, vertex.Color.Y, vertex.Color.Z, 1f),
                Size = 10f
            });
        }

        if (updateMesh)
            UpdateMesh();
    }

    public void AddVertices(List<Vertex> vertices)
    {
        foreach (var vertex in vertices)
        {
            if (!VertexList.Contains(vertex))
            {
                AddVertex(vertex, false);
            }
        }

        UpdateMesh();
    }

    public Edge AddOrReplace(Edge edge)
    {
        foreach (var e in EdgeList)
        {
            if (e.HasSameVertex(edge))
                return e;
        }

        EdgeList.Add(edge);
        EdgeVertices.AddRange(edge.A, edge.B);
        EdgeColors.AddRange(edge.A.Color, edge.B.Color);

        return edge;
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

        Indices.Add((uint)Indices.Count + 0);
        Indices.Add((uint)Indices.Count + 1);
        Indices.Add((uint)Indices.Count + 2);
    }

    public bool AddTriangleSimple(Triangle triangle)
    {
        if (TriangleList.Contains(triangle))
            return false;

        triangle.UpdateNormal();
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
                Vertices.Add(new VertexData
                {
                    Position = vertex.Position,
                    Color = new Vector4(vertex.Color.X, vertex.Color.Y, vertex.Color.Z, 1f),
                    Size = 10f
                });
            }
        }

        foreach (var edge in copy.newSelectedEdges)
        {
            if (!EdgeList.Contains(edge))
            {
                EdgeList.Add(edge);
                EdgeVertices.AddRange(edge.A, edge.B);
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

        if (AB.ParentTriangles.Count == 1) EdgeList.Remove(AB.Delete()); else AB.ParentTriangles.Remove(triangle);
        if (BC.ParentTriangles.Count == 1) EdgeList.Remove(BC.Delete()); else BC.ParentTriangles.Remove(triangle);
        if (CA.ParentTriangles.Count == 1) EdgeList.Remove(CA.Delete()); else CA.ParentTriangles.Remove(triangle);

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

    public void RecalculateNormals()
    {
        Normals.Clear();
        foreach (var triangle in TriangleList)
        {
            triangle.UpdateNormal();
            Normals.AddRange(triangle.Normal, triangle.Normal, triangle.Normal);
        }

        _normalVbo.Update(Normals);
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

        HashSet<Edge> edgesToRemove = [];
        HashSet<Triangle> trianglesToRemove = [];

        for (int i = 1; i < vertices.Count; i++)
        {
            vertices[0].Position += vertices[i];
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

        // Check for duplicate edges
        for (int i = 0; i < edges.Count - 1; i++)
        {
            Edge edgeA = edges[i];
            for (int j = i + 1; j < edges.Count; j++)
            {
                Edge edgeB = edges[j];

                if (edgeA.HasSameVertex(edgeB))
                {
                    edgeB.Delete();
                    EdgeList.Remove(edgeB);
                }
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

    public void CheckUselessVertices()
    {
        List<Vertex> vertices = [.. VertexList];
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex = vertices[i];
            if (vertex.ParentEdges.Count == 0 && vertex.ParentTriangles.Count == 0)
            {
                VertexList.Remove(vertex);
            }
        }
    }

    public void CombineDuplicateVertices()
    {
        List<Vertex> vertices = [.. VertexList];
        for (int i = 0; i < vertices.Count - 1; i++)
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
        _vertexVbo.Update(Vertices);
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
        CheckUselessVertices();
        CheckUselessEdges();
        CheckUselessTriangles();

        string path = Path.Combine(basePath, $"{modelName}.model");
        if (!File.Exists(path)) File.WriteAllText(path, "0\n0\n0\n0\n0");
        List<string> oldLines = [.. File.ReadAllLines(path)];
        List<string> newLines = new List<string>();

        int oldVertexCount = int.Parse(oldLines[0]);
        int oldEdgeCount = int.Parse(oldLines[oldVertexCount + 1]);
        int oldUvCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + 2]);
        int oldTriangleCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldUvCount + 3]);
        int oldNormalCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldUvCount + oldTriangleCount + 4]);
        int rigStart = oldVertexCount + oldEdgeCount + oldUvCount + oldTriangleCount + oldNormalCount + 5;

        newLines.Add(VertexList.Count.ToString());
        foreach (var vertex in VertexList)
        {
            newLines.Add($"v {vertex.X} {vertex.Y} {vertex.Z} {vertex.Index}");
        }

        newLines.Add(EdgeList.Count.ToString());
        foreach (var edge in EdgeList)
        {
            newLines.Add($"e {VertexList.IndexOf(edge.A)} {VertexList.IndexOf(edge.B)}");
        }

        newLines.Add((TriangleList.Count * 3).ToString());
        foreach (var triangle in TriangleList)
        {
            newLines.Add($"uv {triangle.UvA.X} {triangle.UvA.Y}");
            newLines.Add($"uv {triangle.UvB.X} {triangle.UvB.Y}");
            newLines.Add($"uv {triangle.UvC.X} {triangle.UvC.Y}");
        }

        newLines.Add(TriangleList.Count.ToString());
        foreach (var triangle in TriangleList)
        {
            newLines.Add($"f {VertexList.IndexOf(triangle.A)} {VertexList.IndexOf(triangle.B)} {VertexList.IndexOf(triangle.C)} {EdgeList.IndexOf(triangle.AB)} {EdgeList.IndexOf(triangle.BC)} {EdgeList.IndexOf(triangle.CA)}");
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
            Console.WriteLine($"Model {modelName} does not exist.");
            PopUp.AddPopUp("The model does not exist.");
            return false;
        }

        Unload();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int uvCount = int.Parse(lines[vertexCount + edgeCount + 2]);
        int triangleCount = int.Parse(lines[vertexCount + edgeCount + uvCount + 3]);

        for (int i = 1; i <= vertexCount; i++)
        {
            string[] values = lines[i].Split(' ');
            Vertex vertex = new Vertex(new Vector3(Float.Parse(values[1]), Float.Parse(values[2]), Float.Parse(values[3])));
            vertex.Name = "Vertex " + i;
            vertex.Index = int.Parse(values[4]);
            VertexList.Add(vertex);
        }

        for (int i = vertexCount + 2; i <= vertexCount + edgeCount + 1; i++)
        {
            string[] values = lines[i].Split(' ');
            EdgeList.Add(new Edge(VertexList[int.Parse(values[1])], VertexList[int.Parse(values[2])]));
        }

        for (int i = vertexCount + edgeCount + 3; i <= vertexCount + edgeCount + uvCount + 2; i++)
        {
            string[] values = lines[i].Split(' ');
            Uvs.Add(new Vector2(Float.Parse(values[1]), Float.Parse(values[2])));
        }

        int index = 0;
        for (int i = vertexCount + edgeCount + uvCount + 4; i <= vertexCount + edgeCount + uvCount + triangleCount + 3; i++)
        {
            string[] values = lines[i].Split(' ');
            
            Vertex a, b, c;
        
            try
            {
                a = VertexList[int.Parse(values[1])];
                b = VertexList[int.Parse(values[2])];
                c = VertexList[int.Parse(values[3])];
            }
            catch (Exception)
            {
                PopUp.AddPopUp("An error happened when loading the model: Getting vertices for the faces");
                Unload();
                return false;
            }
            
            Uv uvA = Uvs.ElementAtOrDefault(index + 0);
            Uv uvB = Uvs.ElementAtOrDefault(index + 1);
            Uv uvC = Uvs.ElementAtOrDefault(index + 2);

            Edge ab, bc, ca;

            try
            {
                ab = EdgeList[int.Parse(values[4])];
                bc = EdgeList[int.Parse(values[5])];
                ca = EdgeList[int.Parse(values[6])];
            }
            catch (Exception)
            {
                PopUp.AddPopUp("An error happened when loading the model: Getting edges for the faces");
                Unload();
                return false;
            }

            Triangle triangle = new Triangle(a, b, c, uvA, uvB, uvC, ab, bc, ca);
            AddTriangleSimple(triangle);
            index += 3;
        }

        CheckUselessVertices();
        CheckUselessEdges();
        CheckUselessTriangles();

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
        Vertices.Clear();
        EdgeVertices.Clear();
        EdgeList.Clear();
    }

    public void Delete()
    {
        Unload();

        _vao.DeleteBuffer();
        _ibo.DeleteBuffer();
        _vertVbo.DeleteBuffer();
        _uvVbo.DeleteBuffer();
        _textureVbo.DeleteBuffer();
        _normalVbo.DeleteBuffer();
        _vertexVbo.DeleteBuffer();
        _edgeVbo.DeleteBuffer();
        _edgeColorVbo.DeleteBuffer();
        _vertexVao.DeleteBuffer();
        _edgeVao.DeleteBuffer();
    }
    
    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _vertVbo.Renew(_transformedVerts);
        _uvVbo.Renew(Uvs);
        _textureVbo.Renew(TextureIndices);
        _normalVbo.Renew(Normals);

        _vertexVbo.Renew(Vertices);

        _edgeVbo.Renew(EdgeVertices);
        _edgeColorVbo.Renew(EdgeColors);

        _vao.Bind();
        
        _vao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _vertVbo);
        _vao.LinkToVAO(1, 2, VertexAttribPointerType.Float, 0, 0, _uvVbo);
        _vao.IntLinkToVAO(2, 1, VertexAttribIntegerType.Int, 0, 0, _textureVbo);
        _vao.LinkToVAO(3, 3, VertexAttribPointerType.Float, 0, 0, _normalVbo); 

        _vao.Unbind();  

        _vertexVao.Bind();  

        _vertexVbo.Bind();

        int stride = Marshal.SizeOf(typeof(VertexData));
        _vertexVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
        _vertexVao.Link(1, 3, VertexAttribPointerType.Float, stride, 3 * sizeof(float));
        _vertexVao.Link(2, 1, VertexAttribPointerType.Float, stride, 7 * sizeof(float));

        _vertexVbo.Unbind();

        _vertexVao.Unbind();

        _edgeVao.Bind();

        _edgeVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _edgeVbo);
        _edgeVao.LinkToVAO(1, 3, VertexAttribPointerType.Float, 0, 0, _edgeColorVbo);

        _edgeVao.Unbind();
        
        _ibo.Renew(Indices);
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
        GL.Enable(EnableCap.ProgramPointSize);

        _vertexVao.Bind();

        GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Count);

        _vertexVao.Unbind();

        GL.Disable(EnableCap.ProgramPointSize);
    }

    public void RenderEdges()
    {
        _edgeVao.Bind();

        GL.DrawArrays(PrimitiveType.Lines, 0, EdgeVertices.Count);

        _edgeVao.Unbind();
    }
}