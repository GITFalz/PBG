using ConsoleApp1.Engine.Scripts.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model
{
    public string Name = "Model";
    public bool IsShown = true;
    public bool IsSelected = false;

    public string CurrentModelName = "";
    public string TextureName = "empty.png";
    public TextureLocation TextureLocation = TextureLocation.NormalTexture;
    

    public ModelMesh Mesh;


    private static ShaderProgram _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
    private static int _modelLocation = _shaderProgram.GetLocation("model");
    private static int _viewLocation = _shaderProgram.GetLocation("view");
    private static int _projectionLocation = _shaderProgram.GetLocation("projection");
    private static int _colorAlphaLocation = _shaderProgram.GetLocation("colorAlpha");
    private static ShaderProgram _animationShader = new ShaderProgram("Model/ModelAnimation.vert", "Model/Model.frag");
    private static int _animationModelLocation = _animationShader.GetLocation("model");
    private static int _animationViewLocation = _animationShader.GetLocation("view");
    private static int _animationProjectionLocation = _animationShader.GetLocation("projection");
    private static int _animationColorAlphaLocation = _animationShader.GetLocation("colorAlpha");
    private static ShaderProgram _activeShader = _shaderProgram;
    private static int _activeModelLocation = _modelLocation;
    private static int _activeViewLocation = _viewLocation; 
    private static int _activeProjectionLocation = _projectionLocation;
    private static int _activeColorAlphaLocation = _colorAlphaLocation;
    public Texture Texture = new Texture("empty.png", TextureLocation.NormalTexture);


    public static ModelCopy randomCopy = new();
    public static ModelCopy Copy = new();


    public List<Vertex> SelectedVertices = new();
    public List<Edge> SelectedEdges = new();
    public List<Triangle> SelectedTriangles = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();


    public SSBO<Matrix4> BoneMatrices = new SSBO<Matrix4>();
    public List<Matrix4> BoneMatricesList = [];

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            ModelMatrix = Matrix4.CreateTranslation(_position);
        }
    }
    private Vector3 _position = Vector3.Zero;
    public Matrix4 ModelMatrix = Matrix4.Identity;


    public Rig? Rig;
    public string? RigName => Rig?.Name;
    public Animation? Animation;

    public bool RenderBones = false;
    public bool Animate = false;


    public Model()
    {
        Mesh = new ModelMesh(this);
        Create();
    }

    public void Create()
    {
        if (!RigManager.TryGet("Base", out Rig? rig))
        {
            if (RigManager.Load("Base") && RigManager.TryGet("Base", out rig, false))
            {
                Rig = rig;
            }
            else
            {
                RootBone root = new RootBone("RootBone");
                Rig r = new Rig("Base");
                r.RootBone = root;

                if (!RigManager.Add(r, false))
                    return;

                if (!RigManager.TryGet("Base", out Rig? rigBase, false))
                {
                    PopUp.AddPopUp("Base rig cannot be loaded.");
                    return;
                }

                Rig = rigBase;
                RigManager.Save("Base", false);
            }
        }
        else
        {
            Rig = rig;
        }

        if (Rig == null)
        {
            PopUp.AddPopUp("Rig cannot be loaded.");
            return;
        }

        Rig.Create();
        Rig.Initialize();
        Rig.RootBone.UpdateGlobalTransformation();

        BoneMatricesList.Clear();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList.Add(bone.FinalMatrix);
        }
        BoneMatrices.Renew(BoneMatricesList);

        Mesh.InitRig();
    }

    public void BindRig()
    {
        if (Rig == null)
            return;

        Mesh.Bind(Rig);
        Mesh.UpdateModel();
    }

    public void InitRig()
    {
        if (Rig == null)
            return;

        Rig.Create();
        Rig.Initialize();

        BoneMatricesList.Clear();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList.Add(bone.FinalMatrix);
        }
        BoneMatrices.Renew(BoneMatricesList);

        Mesh.InitRig();
    }

    public void UpdateMatrices()
    {
        if (Rig == null)
            return;

        BoneMatricesList.Clear();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList.Add(bone.FinalMatrix);
        }
        BoneMatrices.Update(BoneMatricesList, 0);
    }

    public void UpdateRig()
    {
        UpdateMatrices();
        Mesh.UpdateRig();
    }

    public void SetModeling()
    {
        _activeShader = _shaderProgram;
        _activeModelLocation = _modelLocation;
        _activeViewLocation = _viewLocation;
        _activeProjectionLocation = _projectionLocation;
        _activeColorAlphaLocation = _colorAlphaLocation;
        Mesh.Init();
        Mesh.UpdateModel(); 
    }

    public void SetAnimation()
    {
        _activeShader = _animationShader;
        _activeModelLocation = _animationModelLocation;
        _activeViewLocation = _animationViewLocation;
        _activeProjectionLocation = _animationProjectionLocation;
        _activeColorAlphaLocation = _animationColorAlphaLocation;
        if (Rig != null)
        {
            Mesh.Bind(Rig);
            Mesh.UpdateModel();
        }
    }

    public void SetStaticRig()
    {
        if (RigName == null)
        {
            Animate = false;
            if (Rig != null)
            {
                BoneMatrices.Renew(Rig.GetGlobalAnimatedMatrices());
                Mesh.InitRig();
            }
            Rig = null;
            return;
        }

        if (RigManager.Rigs.TryGetValue(RigName, out Rig? value))
        {
            Animate = false;
            Rig = value.Copy();
            BoneMatrices.Renew(Rig.GetGlobalAnimatedMatrices());
            Mesh.InitRig();
        }
        else
        {
            PopUp.AddPopUp($"Rig {RigName} not found.");
        }
    }

    public void SetAnimationRig()
    {
        if (RigName == null)
        {
            Animate = false;
            if (Rig != null)
            {
                SetStaticRig();
            }
            Rig = null;
            return;
        }
        
        if (RigManager.Rigs.TryGetValue(RigName, out Rig? value))
        {
            Animate = true;
            Rig = value.Copy();
            Mesh.InitRig();
        }
        else
        {
            PopUp.AddPopUp($"Rig {RigName} not found.");
        }
    }

    public void SetAnimationFrame(int index)
    {
        if (Animation == null || Rig == null)
            return;

        foreach (var bone in Rig.BonesList)
        {
            var frame = Animation.GetSpecificFrame(bone.Name, index);
            if (frame == null)
                continue;

            bone.Position = frame.Position;
            bone.Rotation = frame.Rotation;
            bone.Scale = frame.Scale;
            bone.LocalAnimatedMatrix = frame.GetLocalTransform();
        }

        Rig.RootBone.UpdateGlobalTransformation();

        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList[bone.Index] = bone.GlobalAnimatedMatrix;
        }

        Mesh.UpdateRig();

        BoneMatrices.Update(BoneMatricesList, 0);
    }

    public void Renew(string fileName, TextureLocation textureLocation)
    {
        TextureName = fileName;
        TextureLocation = textureLocation;
        Texture.Renew(fileName, textureLocation);
    }

    public void Reload()
    {
        Texture.Renew(TextureName, TextureLocation);
    }

    public void Update()
    {
        if (Rig == null || Animation == null)
            return;

        if (Animate)
        {
            foreach (var bone in Rig.BonesList)
            {
                var frame = Animation.GetFrame(bone.Name);
                if (frame == null)
                    continue;

                bone.Position = frame.Position;
                bone.Rotation = frame.Rotation;
                bone.Scale = frame.Scale;
                bone.LocalAnimatedMatrix = frame.GetLocalTransform(); ;
            }

            Rig.RootBone.UpdateGlobalTransformation();

            foreach (var bone in Rig.BonesList)
            {
                BoneMatricesList[bone.Index] = bone.GlobalAnimatedMatrix;
            }

            Mesh.UpdateRig();

            BoneMatrices.Update(BoneMatricesList, 0);
        }
    }

    public void Render()
    {
        RenderMirror();
        RenderWireframe();

        if (Rig != null && RenderBones)
        {
            Mesh.RenderBones();
        }
        
        GL.CullFace(TriangleFace.Back);
    }

    public void RenderMirror()
    {
        var camera = Game.camera;

        _activeShader.Bind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        Matrix4 Model = ModelMatrix;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        for (int i = 0; i < ModelSettings.Mirrors.Length; i++)
        {
            Vector3 flip = ModelSettings.Mirrors[i];
            GL.CullFace(ModelSettings.BackfaceCulling ? (i % 2 == 0 ? TriangleFace.Back : TriangleFace.Front) : TriangleFace.FrontAndBack);

            Model = Matrix4.CreateScale(flip) * ModelMatrix;

            GL.UniformMatrix4(_activeModelLocation, false, ref Model);
            GL.UniformMatrix4(_activeViewLocation, false, ref view);
            GL.UniformMatrix4(_activeProjectionLocation, false, ref projection);
            GL.Uniform1(_activeColorAlphaLocation, ModelSettings.MeshAlpha);

            Texture.Bind();

            if (Rig != null)
            {
                BoneMatrices.Bind(0);
                Mesh.Render();
                BoneMatrices.Unbind();
            }
            else
            {
                Mesh.Render();
            }

            Texture.Unbind();
        }

        _activeShader.Unbind();
    }

    public void RenderWireframe()
    {
        if (ModelSettings.WireframeVisible && IsSelected)
        {
            var camera = Game.camera;

            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Always);

            Matrix4 model = ModelMatrix;
            Matrix4 view = camera.ViewMatrix;
            Matrix4 projection = camera.ProjectionMatrix;

            ModelSettings.EdgeShader.Bind();

            GL.UniformMatrix4(ModelSettings.edgeModelLocation, false, ref model);
            GL.UniformMatrix4(ModelSettings.edgeViewLocation, false, ref view);
            GL.UniformMatrix4(ModelSettings.edgeProjectionLocation, false, ref projection);

            BoneMatrices.Bind(0);

            Mesh.RenderEdges();

            BoneMatrices.Unbind();

            Shader.Error("Rendering edges error: ");

            ModelSettings.EdgeShader.Unbind();

            ModelSettings.VertexShader.Bind();

            GL.UniformMatrix4(ModelSettings.edgeModelLocation, false, ref model);
            GL.UniformMatrix4(ModelSettings.edgeViewLocation, false, ref view);
            GL.UniformMatrix4(ModelSettings.edgeProjectionLocation, false, ref projection);

            Mesh.RenderVertices();

            Shader.Error("Rendering vertices error: ");

            ModelSettings.VertexShader.Unbind();

            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(true);
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
            Vector3 vertPosition = (ModelMatrix.Transposed() * new Vector4(vert, 1.0f)).Xyz;
            Vector2? screenPos = Mathf.WorldToScreen(vertPosition, projection, view);
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

    public bool LoadModelFromPath(string path)
    {
        return LoadFromPath(path);
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

    public bool LoadFromPath(string path)
    {
        if (Mesh.LoadModelFromPath(path))
        {
            CurrentModelName = Path.GetFileNameWithoutExtension(path);
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
        Clear();
        Mesh.Delete();
        ModelManager.DeleteModel(this);
    }

    public void Clear()
    {
        SelectedVertices = [];
        SelectedEdges = [];
        SelectedTriangles = [];
        Vertices = [];
    }


    public void GetConnectedVertices()
    {
        if (SelectedVertices.Count == 0)
            return;

        HashSet<Vertex> connectedVertices = [];
        HashSet<Triangle> connectedTriangles = [];

        for (int i = 0; i < SelectedVertices.Count; i++)
        {
            SelectedVertices[i].GetConnectedVertices(connectedVertices, connectedTriangles);
        }

        SelectedVertices.Clear();
        SelectedVertices.AddRange(connectedVertices);
        GenerateVertexColor();
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