using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UvMesh : Meshes
{
    // Mesh
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());

    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();

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


    public List<Uv> UvList = new List<Uv>();
    public List<UvEdge> EdgeList = new List<UvEdge>();
    public List<UvTriangle> TriangleList = new List<UvTriangle>();


    public void Init()
    {
        InitUvs();
        GenerateIndices();

        Vertices.Clear();
        foreach (var uv in UvList)
        {
            Vertices.Add(new VertexData{
                Position = (uv.X, uv.Y, 0),
                Color = new Vector4(uv.Color.X, uv.Color.Y, uv.Color.Z, 1f),
                Size = 10f
            });
        }

        EdgeVertices.Clear();
        EdgeColors.Clear();
        foreach (var edge in EdgeList)
        {
            EdgeVertices.AddRange((edge.A.X, edge.A.Y, 0), (edge.B.X, edge.B.Y, 0));
            EdgeColors.AddRange(edge.GetColors());
        }
    }

    public void InitUvs()
    {
        Uvs.Clear();
        foreach (var triangle in TriangleList)
        {
            Uvs.AddRange(triangle.GetUvPositions());
        }
    }

    public UvEdge AddOrReplace(UvEdge edge)
    {
        foreach (var e in EdgeList)
        {
            if (e == edge)
                return e;
        }
        EdgeList.Add(edge);
        return edge;
    }

    public Uv AddOrReplace(Uv uv)
    {
        foreach (var u in UvList)
        {
            if (u == uv)
                return u;
        }
        UvList.Add(uv);
        return uv;
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

        List<Vector2> uvs = new List<Vector2>();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int uvCount = int.Parse(lines[edgeCount + vertexCount + 2]);
        int triangleCount = int.Parse(lines[edgeCount + vertexCount + uvCount + 3]);

        for (int i = edgeCount + vertexCount + 3; i <= edgeCount + vertexCount + uvCount + 2; i++)
        {
            string[] values = lines[i].Split(' ');
            Console.WriteLine(string.Join(", ", values));
            uvs.Add(new Vector2(Float.Parse(values[1]), Float.Parse(values[2])));
        }

        int index = 0;
        for (int i = edgeCount + vertexCount + uvCount + 4; i <= edgeCount + vertexCount + uvCount + triangleCount + 3; i++)
        {
            Uv a = AddOrReplace(uvs.ElementAtOrDefault(index + 0));
            Uv b = AddOrReplace(uvs.ElementAtOrDefault(index + 1));
            Uv c = AddOrReplace(uvs.ElementAtOrDefault(index + 2));

            UvEdge ab = AddOrReplace(new UvEdge(a, b));
            UvEdge bc = AddOrReplace(new UvEdge(b, c));
            UvEdge ca = AddOrReplace(new UvEdge(c, a));

            UvTriangle triangle = new UvTriangle(a, b, c, ab, bc, ca);
            TriangleList.Add(triangle);
            
            index += 3;
        }

        Init();
        GenerateBuffers();

        return true;
    }

    public override void SaveModel(string modelName)
    {
        SaveModel(modelName, Game.modelPath);
    }

    public override void SaveModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName}.model");
        if (!File.Exists(path)) 
            return;

        List<string> lines = [.. File.ReadAllLines(path)];

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int uvCount = int.Parse(lines[edgeCount + vertexCount + 2]);

        for (int i = 0; i < UvList.Count; i++)
        {
            if (i >= uvCount)
                break;

            var uv = UvList[i];
            lines[i + edgeCount + vertexCount + 3] = $"uv {Float.Str(uv.X)} {Float.Str(uv.Y)}";
        }

        File.WriteAllLines(path, lines);
    }

    public void UpdateVertexColors()
    {
        _vertexVbo.Update(Vertices);
    }

    public void UpdateEdgeColors()
    {
        _edgeColorVbo.Update(EdgeColors);
    }

    public void UpdateEdges()
    {
        _edgeVbo.Update(EdgeVertices);
    }

    public void UpdatePosition()
    {
        _uvVbo.Update(Uvs);
    }

    public void UpdateMesh()
    {
        UpdateVertexColors();
        UpdateEdgeColors();
        UpdateEdges();
        UpdatePosition();
    }



    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < TriangleList.Count * 3; i++)
        {
            Indices.Add(i);
        }
    }

    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _uvVbo.Renew(Uvs);

        _vertexVbo.Renew(Vertices);

        _edgeVbo.Renew(EdgeVertices);
        _edgeColorVbo.Renew(EdgeColors);

        _vao.Bind();
        
        _vao.LinkToVAO(0, 2, VertexAttribPointerType.Float, 0, 0, _uvVbo);

        _vao.Unbind();  

        _vertexVao.Bind();  

        _vertexVbo.Bind();

        int stride = Marshal.SizeOf(typeof(VertexData));
        _vertexVao.Link(0, 3, VertexAttribPointerType.Float, stride, 0);
        _vertexVao.Link(1, 4, VertexAttribPointerType.Float, stride, 3 * sizeof(float));
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

    public override void Unload()
    {
        UvList.Clear();
        EdgeList.Clear();
        TriangleList.Clear();

        Uvs.Clear();
        Indices.Clear();

        Vertices.Clear();   

        EdgeVertices.Clear();
        EdgeColors.Clear();
    }
}