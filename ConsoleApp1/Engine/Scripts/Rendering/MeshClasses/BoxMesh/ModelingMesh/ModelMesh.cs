using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ModelMesh : Meshes
{
    public static ShaderProgram RigShader = new ShaderProgram("Model/ModelRig.vert", "Model/ModelRig.frag");
    public Model Model;

    // Mesh
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO();
    private VBO<Vector3> _vertVbo = new(new List<Vector3>());
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());
    private VBO<Vector2i> _textureVbo = new(new List<Vector2i>());
    private VBO<Vector3> _normalVbo = new(new List<Vector3>());

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<Vector2i> TextureIndices = new List<Vector2i>();
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


    // Bone
    public VAO _boneVao = new VAO();
    public IBO BoneIBO;

    public VBO<Vector3> BoneVertexVBO;
    public VBO<Vector3> BoneNormalVBO;
    public VBO<Matrix4> BoneDataVBO = new();
    public VBO<int> BoneColorVBO = new();
    public int indicesCount = 0;


    public int BoneCount = 0;

    
    public ModelMesh(Model model) 
    {
        float size = 0.1f;

        Model = model;

        Vector3 firstSize = new Vector3(0.3f, 0.3f, 0.3f) * size;
        Vector3 firstOffset = new Vector3(0, 0, 0) * size;

        Vector3 secondScale = new Vector3(0.2f, 0.2f, 0.2f) * size;    
        Vector3 secondOffset = new Vector3(0, 2, 0) * size;

        Vector3 thirdScale = new Vector3(2, 2, 2) * size;   
        Vector3 thirdOffset = new Vector3(0, 1.0625f, 0) * size;

        float t = (float)(1.0f + Math.Sqrt(5f)) / 2.0f;

        List<Vector3> vertices = [
            // Sphere 1     
            new Vector3(-1, t, 0),
            new Vector3( 1, t, 0),
            new Vector3(-1,-t, 0),
            new Vector3( 1,-t, 0), 

            new Vector3( 0,-1, t),
            new Vector3( 0, 1, t),
            new Vector3( 0,-1,-t),
            new Vector3( 0, 1,-t),

            new Vector3( t, 0,-1),
            new Vector3( t, 0, 1),
            new Vector3(-t, 0,-1),
            new Vector3(-t, 0, 1),
        ];

        uint[] indices = [
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1,
        ];

        List<Vector3> newVertices = [];
        List<Vector3> normals = [];
        List<uint> newIndices = [];

        Vector3 scale = firstSize;
        Vector3 offset = firstOffset;

        for (int j = 0; j < 2; j++) 
        {
            List<Vector3> sphere = [];

            for (int i = 0; i < 20; i++)
            {
                int a = (int)indices[i * 3 + 0];
                int b = (int)indices[i * 3 + 1];
                int c = (int)indices[i * 3 + 2];

                Vector3 A = vertices[a];
                Vector3 B = vertices[b];
                Vector3 C = vertices[c];

                Vector3 AB = (A + B) / 2;
                Vector3 AC = (A + C) / 2;
                Vector3 BC = (B + C) / 2;

                if (!sphere.Contains(A))
                    sphere.Add(A);
                
                if (!sphere.Contains(B))
                    sphere.Add(B);

                if (!sphere.Contains(C))
                    sphere.Add(C);

                if (!sphere.Contains(AB))
                    sphere.Add(AB);

                if (!sphere.Contains(AC))
                    sphere.Add(AC);

                if (!sphere.Contains(BC))
                    sphere.Add(BC);

                a = sphere.IndexOf(A);
                b = sphere.IndexOf(B);
                c = sphere.IndexOf(C);

                int ab = sphere.IndexOf(AB);
                int ac = sphere.IndexOf(AC);
                int bc = sphere.IndexOf(BC);

                newIndices.Add((uint)(a + newVertices.Count));
                newIndices.Add((uint)(ab + newVertices.Count));
                newIndices.Add((uint)(ac + newVertices.Count));

                newIndices.Add((uint)(b + newVertices.Count));
                newIndices.Add((uint)(bc + newVertices.Count));
                newIndices.Add((uint)(ab + newVertices.Count));

                newIndices.Add((uint)(c + newVertices.Count));
                newIndices.Add((uint)(ac + newVertices.Count));
                newIndices.Add((uint)(bc + newVertices.Count));

                newIndices.Add((uint)(ab + newVertices.Count));
                newIndices.Add((uint)(bc + newVertices.Count));
                newIndices.Add((uint)(ac + newVertices.Count));
            }

            for (int i = 0; i < sphere.Count; i++)
            {  
                Vector3 vertex = sphere[i];
                vertex.Normalize();
                normals.Add(vertex);
                sphere[i] = vertex * scale + offset;
            }

            newVertices.AddRange(sphere);

            scale = secondScale;
            offset = secondOffset;
        }

        uint count = (uint)newVertices.Count;

        newVertices.AddRange(
            new Vector3(     0,     1,     0) * thirdScale,
            new Vector3( 0.1f, 0.25f,     0) * thirdScale,
            new Vector3(     0, 0.25f, 0.1f) * thirdScale,
            new Vector3(-0.1f, 0.25f,     0) * thirdScale,
            new Vector3(     0, 0.25f,-0.1f) * thirdScale,
            new Vector3(     0,     0,     0) * thirdScale
        );

        normals.AddRange(
            new Vector3( 0, 1, 0),
            new Vector3( 1, 0, 0),
            new Vector3( 0, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3( 0, 0,-1),
            new Vector3( 0, 0, 0)
        );

        newIndices.AddRange(
            [0+count, 2+count, 1+count,
            0+count, 3+count, 2+count,
            0+count, 4+count, 3+count,
            0+count, 1+count, 4+count,
            1+count, 2+count, 5+count,
            2+count, 3+count, 5+count,
            3+count, 4+count, 5+count,
            4+count, 1+count, 5+count]
        );

        indicesCount = newIndices.Count;
        
        
        BoneIBO = new(newIndices);
        BoneVertexVBO = new(newVertices); 
        BoneNormalVBO = new(normals);

        _boneVao.Bind();

        _boneVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0, BoneVertexVBO);
        _boneVao.LinkToVAO(1, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0, BoneNormalVBO);

        _boneVao.Unbind();
    }

    
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

    public void InitRig()
    {
        Rig? rig = Model.Rig;
        if (rig == null)
            return;

        List<Matrix4> boneMatrices = [];
        List<int> boneColors = [];

        foreach (var bone in rig.BonesList)
        {
            boneMatrices.Add(bone.FinalMatrix);
            boneColors.Add((int)bone.Selection);
        }

        BoneCount = boneMatrices.Count;

        BoneDataVBO.Renew(boneMatrices);
        BoneColorVBO.Renew(boneColors);

        _boneVao.Bind();

        BoneDataVBO.Bind();

        int vec4Size = sizeof(float) * 4;
        for (int i = 0; i < 4; i++)
        {
            _boneVao.InstanceLink(2 + i, 4, VertexAttribPointerType.Float, 64, i * vec4Size, 1);
        }

        BoneDataVBO.Unbind();

        BoneColorVBO.Bind();

        _boneVao.InstanceIntLink(6, 1, VertexAttribIntegerType.Int, 4, 0, 1);

        BoneColorVBO.Unbind();

        _boneVao.Unbind();
    }

    public void UpdateRigVertexPosition()
    {
        for (int i = 0; i < TriangleList.Count; i++)
        {
            var t = TriangleList[i];
            
            Vertex A = t.A;
            Vertex B = t.B;
            Vertex C = t.C;

            if (A.Bone != null)
            {
                Vector3 APos = (A.Bone.TransposedInverseGlobalAnimatedMatrix * A.V4).Xyz;
                _transformedVerts[i * 3 + 0] = APos;
                TextureIndices[i * 3 + 0] = (0, A.Bone.Index);
            }

            if (B.Bone != null)
            {
                Vector3 BPos = (B.Bone.TransposedInverseGlobalAnimatedMatrix * B.V4).Xyz;
                _transformedVerts[i * 3 + 1] = BPos;
                TextureIndices[i * 3 + 1] = (0, B.Bone.Index);
            }

            if (C.Bone != null)
            {
                Vector3 CPos = (C.Bone.TransposedInverseGlobalAnimatedMatrix * C.V4).Xyz;
                _transformedVerts[i * 3 + 2] = CPos;
                TextureIndices[i * 3 + 2] = (0, C.Bone.Index);
            }
        }

        UpdateModel();
    }

    public void UpdateRig()
    {
        Rig? rig = Model.Rig;
        if (rig == null)
            return;

        List<Matrix4> boneMatrices = [];
        List<int> boneColors = [];

        foreach (var bone in rig.BonesList)
        {
            boneMatrices.Add(bone.FinalMatrix);
            boneColors.Add((int)bone.Selection);
        }

        BoneDataVBO.Update(boneMatrices);
        BoneColorVBO.Update(boneColors);
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

        TextureIndices.Add((0, 1));
        TextureIndices.Add((0, 1));
        TextureIndices.Add((0, 1));

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
                    vertices.RemoveAt(j);
                    j--;
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

    public void UpdateModel()
    {
        _normalVbo.Update(Normals);
        _vertVbo.Update(_transformedVerts);
        _textureVbo.Update(TextureIndices);
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

        int oldVertexCount = Int.Parse(oldLines[0]);
        int oldEdgeCount = Int.Parse(oldLines[oldVertexCount + 1]);
        int oldUvCount = Int.Parse(oldLines[oldVertexCount + oldEdgeCount + 2]);
        int oldTriangleCount = Int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldUvCount + 3]);
        int oldNormalCount = Int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldUvCount + oldTriangleCount + 4]);
        int rigStart = oldVertexCount + oldEdgeCount + oldUvCount + oldTriangleCount + oldNormalCount + 5;

        newLines.Add(VertexList.Count.ToString());
        foreach (var vertex in VertexList)
        {
            newLines.Add($"v {Float.Str(vertex.X)} {Float.Str(vertex.Y)} {Float.Str(vertex.Z)} {vertex.Index} {vertex.BoneName}");
        }

        newLines.Add(EdgeList.Count.ToString());
        foreach (var edge in EdgeList)
        {
            newLines.Add($"e {VertexList.IndexOf(edge.A)} {VertexList.IndexOf(edge.B)}");
        }

        newLines.Add((TriangleList.Count * 3).ToString());
        foreach (var triangle in TriangleList)
        {
            newLines.Add($"uv {Float.Str(triangle.UvA.X)} {Float.Str(triangle.UvA.Y)}");
            newLines.Add($"uv {Float.Str(triangle.UvB.X)} {Float.Str(triangle.UvB.Y)}");
            newLines.Add($"uv {Float.Str(triangle.UvC.X)} {Float.Str(triangle.UvC.Y)}");
        }

        newLines.Add(TriangleList.Count.ToString());
        foreach (var triangle in TriangleList)
        {
            newLines.Add($"f {VertexList.IndexOf(triangle.A)} {VertexList.IndexOf(triangle.B)} {VertexList.IndexOf(triangle.C)} {EdgeList.IndexOf(triangle.AB)} {EdgeList.IndexOf(triangle.BC)} {EdgeList.IndexOf(triangle.CA)}");
        }

        newLines.Add(Normals.Count.ToString());
        foreach (var normal in Normals)
        {
            newLines.Add($"n {Float.Str(normal.X)} {Float.Str(normal.Y)} {Float.Str(normal.Z)}");
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

    public bool LoadModelFromPath(string path)
    {
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("The model file does not exist.");
            return false;
        }

        return LoadModelFromPathInternal(path);
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

        return LoadModelFromPathInternal(path);
    }

    private bool LoadModelFromPathInternal(string path)
    {
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

            Uv uvA = Uvs.ElementAtOrDefault(index + 0);
            Uv uvB = Uvs.ElementAtOrDefault(index + 1);
            Uv uvC = Uvs.ElementAtOrDefault(index + 2);

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
        _boneVao.DeleteBuffer();
        BoneVertexVBO.DeleteBuffer();
        BoneNormalVBO.DeleteBuffer();
        BoneDataVBO.DeleteBuffer();
        BoneIBO.DeleteBuffer();
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
        _vao.IntLinkToVAO(2, 2, VertexAttribIntegerType.Int, 0, 0, _textureVbo);
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

    public void RenderBones(Matrix4 model)
    {
        if (BoneCount == 0)
            return;

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.DepthMask(true);
            
        RigShader.Bind();

        Matrix4 view = Game.camera.GetViewMatrix();
        Matrix4 projection = Game.camera.GetProjectionMatrix();

        int modelLocation = RigShader.GetLocation("model");
        int viewLocation = RigShader.GetLocation("view");
        int projectionLocation = RigShader.GetLocation("projection");   

        GL.UniformMatrix4(modelLocation, false, ref model);
        GL.UniformMatrix4(viewLocation, false, ref view);
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        _boneVao.Bind();
        BoneIBO.Bind();

        GL.DrawElementsInstanced(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero, BoneCount);

        Shader.Error("Bone render error: ");

        BoneIBO.Unbind();
        _boneVao.Unbind();

        RigShader.Unbind();
    }
}