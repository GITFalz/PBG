using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelMesh
{
    public static ShaderProgram RigShader = new ShaderProgram("Model/ModelRig.vert", "Model/ModelRig.frag");
    public Model Model;

    // Mesh
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new(new List<Vector3>());
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());
    private VBO<Vector2i> _textureVbo = new(new List<Vector2i>());
    private VBO<Vector3> _normalVbo = new(new List<Vector3>());

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<Vector2i> TextureIndices = new List<Vector2i>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector3> _transformedVerts = new List<Vector3>();


    public List<Vertex> VertexList = new List<Vertex>();
    public List<Edge> EdgeList = new List<Edge>();
    public List<Triangle> TriangleList = new List<Triangle>();

    public int indicesCount = 0;


    public int BoneCount = 0;

    
    public ModelMesh(Model model) 
    {
        float size = 0.1f;

        Model = model;
    }

    
    public void Init()
    {
        _transformedVerts.Clear();
        Uvs.Clear();
        TextureIndices.Clear();

        if (TriangleList.Count == 0)
            return;

        foreach (var t in TriangleList)
        {
            _transformedVerts.AddRange(t.GetVerticesPosition());
            Uvs.AddRange(t.GetUvs());
            TextureIndices.AddRange([(0, 1), (0, 1), (0, 1)]);
        }
    }

    public void Bind(Rig rig)
    {
        _transformedVerts.Clear();
        Uvs.Clear();
        TextureIndices.Clear();

        if (TriangleList.Count == 0)
            return;

        for (int i = 0; i < TriangleList.Count; i++)
        {
            var t = TriangleList[i];

            Vertex A = t.A;
            Vertex B = t.B;
            Vertex C = t.C;

            Vector3 APos;
            int boneIndexA;
            if (rig.GetBone(A.BoneName, out var boneA)) {
                A.Bone = boneA;
                APos = (boneA.TransposedInverseGlobalAnimatedMatrix * A.V4).Xyz;
                boneIndexA = boneA.Index;
            } else {
                A.Bone = null;
                APos = A;
                boneIndexA = 0;
            }
            Vector3 BPos;
            int boneIndexB;
            if (rig.GetBone(B.BoneName, out var boneB)) {
                B.Bone = boneB;
                BPos = (boneB.TransposedInverseGlobalAnimatedMatrix * B.V4).Xyz;
                boneIndexB = boneB.Index;
            } else {
                B.Bone = null;
                BPos = B;
                boneIndexB = 0;
            }
            Vector3 CPos;
            int boneIndexC;
            if (rig.GetBone(C.BoneName, out var boneC)) {
                C.Bone = boneC;
                CPos = (boneC.TransposedInverseGlobalAnimatedMatrix  * C.V4).Xyz;
                boneIndexC = boneC.Index;
            } else {
                C.Bone = null;
                CPos = C;
                boneIndexC = 0;
            }

            _transformedVerts.AddRange(APos, BPos, CPos);
            Uvs.AddRange(t.GetUvs());
            TextureIndices.AddRange([(0, boneIndexA), (0, boneIndexB), (0, boneIndexC)]);
        }
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

        TextureIndices.Add((0, 1));
        TextureIndices.Add((0, 1));
        TextureIndices.Add((0, 1));

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

        for (int i = 1; i < vertices.Count; i++)
        {
            vertices[0].Position += vertices[i];
            vertices[i].ReplaceWith(vertices[0]);
            VertexList.Remove(vertices[i]);
        }

        CheckUselessEdges();
        CheckUselessTriangles();
        CheckUselessVertices();

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
            if (!VertexList.Contains(edge.A) || !VertexList.Contains(edge.B) || edge.A == edge.B)
            {
                edge.Delete();
                EdgeList.Remove(edge);
            }
            for (int j = i + 1; j < edges.Count; j++)
            {
                Edge edgeB = edges[j];

                if (edge.HasSameVertex(edgeB))
                {
                    edgeB.ReplaceWith(edge);
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
            if (!VertexList.Contains(triangle.A) || !VertexList.Contains(triangle.B) || !VertexList.Contains(triangle.C))
            {
                Console.WriteLine($"{triangle} is useless because of vertices.");
                triangle.Delete();
                TriangleList.Remove(triangle);
            }
            if (!EdgeList.Contains(triangle.AB) || !EdgeList.Contains(triangle.BC) || !EdgeList.Contains(triangle.CA))
            {
                Console.WriteLine($"{triangle} is useless because of edges.");
                triangle.Delete();
                TriangleList.Remove(triangle);
            }
            if (triangle.HasSameVertices())
            {
                Console.WriteLine($"{triangle} is useless because of same vertices.");
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
    }

    public void UpdateModel()
    {
        _normalVbo.Update(Normals);
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);
    }

    public bool LoadModel(string modelName)
    {
        return LoadModelName(modelName, Game.modelPath);
    }

    public bool LoadModelName(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, modelName + ".model");
        return LoadModelFromPath(path);
    }

    public bool LoadModelFromPath(string path)
    {
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("The model does not exist.");
            return false;
        }

        Unload();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = Int.Parse(lines[0]);

        int edgeIndex = vertexCount + 1;
        int edgeCount = Int.Parse(lines[edgeIndex]);

        int uvIndex = vertexCount + edgeCount + 2;
        int uvCount = Int.Parse(lines[uvIndex]);

        int triangleIndex = vertexCount + edgeCount + uvCount + 3;
        int triangleCount = Int.Parse(lines[triangleIndex]);

        for (int i = 1; i <= vertexCount; i++)
        {
            string[] values = lines[i].Trim().Trim().Split(' ');
            //Console.WriteLine($"Loading vertex {lines[i]} lenght: {values.Length}");
            Vertex vertex = new Vertex(new Vector3(Float.Parse(values[1]), Float.Parse(values[2]), Float.Parse(values[3])));
            vertex.Name = "Vertex " + i;
            if (values.Length > 4)
                vertex.Index = Int.Parse(values[4]);

            if (values.Length > 5)
                vertex.BoneName = values[5].Trim();

            VertexList.Add(vertex);
        }

        for (int i = vertexCount + 2; i <= vertexCount + edgeCount + 1; i++)
        {
            string[] values = lines[i].Trim().Split(' ');
            EdgeList.Add(new Edge(VertexList[Int.Parse(values[1])], VertexList[Int.Parse(values[2])]));
        }

        for (int i = vertexCount + edgeCount + 3; i <= vertexCount + edgeCount + uvCount + 2; i++)
        {
            string[] values = lines[i].Trim().Split(' ');
            Uvs.Add(new Vector2(Float.Parse(values[1]), Float.Parse(values[2])));
        }

        int index = 0;
        for (int i = vertexCount + edgeCount + uvCount + 4; i <= vertexCount + edgeCount + uvCount + triangleCount + 3; i++)
        {
            string[] values = lines[i].Trim().Split(' ');

            Vertex a, b, c;

            try
            {
                a = VertexList[Int.Parse(values[1])];
                b = VertexList[Int.Parse(values[2])];
                c = VertexList[Int.Parse(values[3])];
            }
            catch (Exception)
            {
                PopUp.AddPopUp("An error happened when loading the model: Getting vertices for the faces");
                Unload();
                return false;
            }

            Vector2 uvA = Uvs.ElementAtOrDefault(index + 0);
            Vector2 uvB = Uvs.ElementAtOrDefault(index + 1);
            Vector2 uvC = Uvs.ElementAtOrDefault(index + 2);

            Edge ab, bc, ca;

            try
            {
                ab = EdgeList[Int.Parse(values[4])];
                bc = EdgeList[Int.Parse(values[5])];
                ca = EdgeList[Int.Parse(values[6])];
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

    public void Unload()
    {
        VertexList.Clear();
        TriangleList.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextureIndices.Clear();
        Normals.Clear();
        _transformedVerts.Clear();
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
    }
    
    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _vertVbo.Renew(_transformedVerts);
        _uvVbo.Renew(Uvs);
        _textureVbo.Renew(TextureIndices);
        _normalVbo.Renew(Normals);

        _vao.Bind();
        
        _vao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _vertVbo);
        _vao.LinkToVAO(1, 2, VertexAttribPointerType.Float, 0, 0, _uvVbo);
        _vao.IntLinkToVAO(2, 2, VertexAttribIntegerType.Int, 0, 0, _textureVbo);
        _vao.LinkToVAO(3, 3, VertexAttribPointerType.Float, 0, 0, _normalVbo); 

        _vao.Unbind();  
        
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
}