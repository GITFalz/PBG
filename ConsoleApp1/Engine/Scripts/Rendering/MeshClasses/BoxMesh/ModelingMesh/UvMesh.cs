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
    private VAO _vertexVao = new VAO();
    private VBO<Vector3> _vertexVbo = new(new List<Vector3>());
    private VBO<Vector3> _vertexColorVbo = new(new List<Vector3>());

    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> VertexColors = new List<Vector3>();

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
        Uvs.Clear();
        Indices.Clear();
        uint index = 0;
        foreach (var triangle in TriangleList)
        {
            Uvs.AddRange(triangle.GetUvPositions());
            Indices.AddRange(index, index + 1, index + 2);
            index += 3;
        }

        Vertices.Clear();
        VertexColors.Clear(); 
        foreach (var uv in UvList)
        {
            Vertices.Add((uv.X, uv.Y, 0));
            VertexColors.Add(uv.Color);
        }

        EdgeVertices.Clear();
        EdgeColors.Clear();
        foreach (var edge in EdgeList)
        {
            EdgeVertices.AddRange((edge.A.X, edge.A.Y, 0), (edge.B.X, edge.B.Y, 0));
            EdgeColors.AddRange(edge.GetColors());
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
        int uvCount = int.Parse(lines[vertexCount + 1]);
        int triangleCount = int.Parse(lines[vertexCount + uvCount + 2]);

        for (int i = vertexCount + 2; i <= vertexCount + uvCount + 1; i++)
        {
            string[] values = lines[i].Split(' ');
            uvs.Add(new Vector2(float.Parse(values[1]), float.Parse(values[2])));
        }

        int index = 0;
        for (int i = vertexCount + uvCount + 3; i <= vertexCount + uvCount + triangleCount + 2; i++)
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
        
    }

    public override void SaveModel(string modelName, string basePath)
    {
        
    }

    public void GenerateBuffers()
    {
        _uvVbo = new(Uvs);

        _vertexVbo = new(Vertices);
        _vertexColorVbo = new(VertexColors);

        _edgeVbo = new(EdgeVertices);
        _edgeColorVbo = new(EdgeColors);
        
        _vao.LinkToVAO(0, 2, _uvVbo);

        _vertexVao.LinkToVAO(0, 2, _vertexVbo);
        _vertexVao.LinkToVAO(1, 3, _vertexColorVbo);

        _edgeVao.LinkToVAO(0, 2, _edgeVbo);
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

    public override void Unload()
    {
        UvList.Clear();
        EdgeList.Clear();
        TriangleList.Clear();

        Uvs.Clear();
        Indices.Clear();

        Vertices.Clear();   
        VertexColors.Clear();

        EdgeVertices.Clear();
        EdgeColors.Clear();
    }
}