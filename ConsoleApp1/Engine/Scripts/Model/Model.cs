using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model
{
    public string Name = "Model";
    public bool IsShown = true;
    public bool IsSelected = false;

    public string CurrentModelName = "";
    

    public ModelMesh Mesh = new();

    private static ShaderProgram _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
    private static Texture _texture = new Texture("dirt_block.png");


    public static ModelCopy randomCopy = new();
    public static ModelCopy Copy = new();


    public List<Vertex> SelectedVertices = new();
    public List<Edge> SelectedEdges = new();
    public List<Triangle> SelectedTriangles = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();
    

    public void Render()
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _shaderProgram.Bind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        Matrix4 Model = Matrix4.Identity;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation;
        int viewLocation;
        int projectionLocation;
        int colorAlphaLocation;

        foreach (var flip in ModelSettings.Mirrors)
        { 
            Model = Matrix4.CreateScale(flip);

            modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
            viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
            projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
            colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
            
            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);
            GL.Uniform1(colorAlphaLocation, ModelSettings.MeshAlpha);

            _texture.Bind();
            
            Mesh.Render();

            _texture.Unbind();
        }

        _shaderProgram.Unbind();

        if (ModelSettings.WireframeVisible)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Always);

            Model = Matrix4.Identity;

            ModelSettings.EdgeShader.Bind();

            modelLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "model");
            viewLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "view");
            projectionLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "projection");

            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);

            Mesh.RenderEdges();

            Shader.Error("Rendering edges error: ");

            ModelSettings.EdgeShader.Unbind();

            ModelSettings.VertexShader.Bind();

            modelLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "model");
            viewLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "view");
            projectionLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "projection");

            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);

            Mesh.RenderVertices();

            Shader.Error("Rendering vertices error: ");

            ModelSettings.VertexShader.Unbind();

            GL.Disable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
        }
    }


    public void UpdateVertexPosition()
    {
        Vertices.Clear();

        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        foreach (var vert in Mesh.VertexList)
        {
            Vector2? screenPos = Mathf.WorldToScreen(vert, projection, view);
            if (screenPos == null)
                continue;
            
            Vertices.Add(vert, screenPos.Value);
        }
    }

    public void GenerateVertexColor()
    {
        foreach (var vert in Mesh.VertexList)
        {
            int vertIndex = Mesh.VertexList.IndexOf(vert);
            vert.Color = SelectedVertices.Contains(vert) ? (0.25f, 0.3f, 1) : (0f, 0f, 0f);

            if (Mesh.Vertices.Count <= vertIndex)
                continue;
            var vertexData = Mesh.Vertices[vertIndex];
            vertexData.Color = new Vector4(vert.Color.X, vert.Color.Y, vert.Color.Z, 1);
            Mesh.Vertices[vertIndex] = vertexData;
        }

        foreach (var edge in Mesh.EdgeList)
        {
            int edgeIndex = Mesh.EdgeList.IndexOf(edge) * 2;
            if (Mesh.EdgeColors.Count > edgeIndex)
                Mesh.EdgeColors[edgeIndex] = edge.A.Color;

            if (Mesh.EdgeColors.Count > edgeIndex + 1)
                Mesh.EdgeColors[edgeIndex + 1] = edge.B.Color;
        }

        Mesh.UpdateVertexColors();
        Mesh.UpdateEdgeColors();
    }


    public bool LoadModel(string fileName)
    {
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return false;
        }

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        return Load(fileName);
    }

    public void SaveModel(string fileName)
    {
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return;
        }

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string path = Path.Combine(Game.modelPath, $"{fileName}.model");
        if (File.Exists(path))
            PopUp.AddConfirmation("Overwrite existing model?", () => Mesh.SaveModel(fileName), null);
        else
            Mesh.SaveModel(fileName);  
    }

    public void SaveAndLoad(string fileName)
    {
        Mesh.SaveModel(CurrentModelName);
        Load(fileName);
    }

    public bool Load(string fileName)
    {
        if (Mesh.LoadModel(fileName))
        {
            CurrentModelName = fileName;
            return true;
        }
        return false;
    }

    public void Unload()
    {
        Mesh.Unload();
    }

    public void Delete()
    {
        Mesh.Delete();
        ModelManager.Models.Remove(Name);
    }

    
    public static List<Edge> GetFullSelectedEdges(List<Vertex> selectedVertices)
    {
        HashSet<Edge> edges = [];
                
        foreach (var vert in selectedVertices)
        {
            foreach (var edge in vert.ParentEdges)
            {
                if (selectedVertices.Contains(edge.Not(vert)))
                    edges.Add(edge);
            }
        }

        return edges.ToList();
    }

    public static List<Triangle> GetFullSelectedTriangles(List<Vertex> selectedVertices)
    {
        HashSet<Triangle> triangles = [];
                
        foreach (var triangle in GetSelectedTriangles(selectedVertices))
        {
            if (IsTriangleFullySelected(selectedVertices, triangle))
                triangles.Add(triangle);
        }
        
        return triangles.ToList();
    }

    public static HashSet<Triangle> GetSelectedTriangles(List<Vertex> selectedVertices)
    {
        HashSet<Triangle> triangles = [];
                
        foreach (var vert in selectedVertices)
        {
            foreach (var triangle in vert.ParentTriangles)
            {
                triangles.Add(triangle);
            }
        }
        
        return triangles;
    }

    public static bool IsTriangleFullySelected(List<Vertex> selectedVertices, Triangle triangle)
    {
        return selectedVertices.Contains(triangle.A) &&
               selectedVertices.Contains(triangle.B) &&
               selectedVertices.Contains(triangle.C);
    }

    public static List<Vertex> GetVertices(List<Triangle> triangles)
    {
        List<Vertex> vertices = [];

        foreach (var triangle in triangles)
        {
            if (!vertices.Contains(triangle.A))
                vertices.Add(triangle.A);

            if (!vertices.Contains(triangle.B))
                vertices.Add(triangle.B);

            if (!vertices.Contains(triangle.C))
                vertices.Add(triangle.C);
        }

        return vertices;
    }

    public static List<Edge> GetEdges(List<Triangle> triangles)
    {
        List<Edge> edges = [];

        foreach (var triangle in triangles)
        {
            if (!edges.Contains(triangle.AB))
                edges.Add(triangle.AB);

            if (!edges.Contains(triangle.BC))
                edges.Add(triangle.BC);

            if (!edges.Contains(triangle.CA))
                edges.Add(triangle.CA);
        }

        return edges;
    }

    public static Vector3 GetSelectedCenter(List<Vertex> selectedVertices)
    {
        Vector3 center = Vector3.Zero;
        if (selectedVertices.Count == 0)
            return center;

        foreach (var vert in selectedVertices)
        {
            center += vert;
        }
        return center / selectedVertices.Count;
    }

    public static Vector3 GetAverageNormal(List<Triangle> triangles)
    {
        Vector3 normal = (0, 1, 0);
        if (triangles.Count == 0)
            return normal;

        foreach (var triangle in triangles)
        {
            normal += triangle.Normal;
        }
        return normal / triangles.Count;
    }

    public static void MoveSelectedVertices(Vector3 move, List<Vertex> selectedVertices)
    {
        foreach (var vert in selectedVertices)
        {
            if (ModelSettings.GridAligned && ModelSettings.Snapping)
                vert.SnapPosition(move, ModelSettings.SnappingFactor);
            else
                vert.MovePosition(move);
        }
    }

    public static void Handle_Flattening(List<Triangle> triangles)
    {
        if (triangles.Count == 0)
            return;

        Triangle first = triangles[0];

        Vector3 rotationAxis = Vector3.Cross(first.Normal, (0, 1, 0));

        if (rotationAxis.Length != 0)
        {
            float angle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(first.Normal, (0, 1, 0)));
            Vector3 center = first.GetCenter();
            Vector3 rotatedNormal = Mathf.RotatePoint(first.Normal, Vector3.Zero, rotationAxis, angle);

            if (Vector3.Dot(rotatedNormal, (0, 1, 0)) < 0)
                angle += 180f;
            
            foreach (var vert in Model.GetVertices(triangles))
                vert.SetPosition(Mathf.RotatePoint(vert, center, rotationAxis, angle));
        }

        first.FlattenRegion(triangles);
    }
}