using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditor : Updateable
{
    public static Symmetry symmetry = Symmetry.None;

    private Camera camera;
    
    private ShaderProgram _shaderProgram;
    private AnimationMesh _playerMesh;
    private ModelMesh _modelMesh;
    
    private ModelNode node;
    private ModelNode node2;
    private ModelNode node3;
    private ModelNode node4;
    private ModelNode node5;
    private ModelNode node6;
    private ModelNode node7;
    
    //Text
    public StaticText text;

    public StaticText xText;
    public StaticText yText;
    public StaticText zText;
    
    public StaticText symmetryText;
    public string? symmetryString = null;
    
    
    private List<ModelNode> nodes = new List<ModelNode>();
    
    private ModelNode selectedNode;
    
    private GameObject go;
    
    public static bool clickedRotate = false;
    public static bool clickedMove = false;
    private bool saveRotate = false;
    private bool saveMove = false;
    private bool regenerateVertexUi = true;
    
    private bool freeCamera = false;

    private int _selectedModel = 0;
    
    private List<Undoable> undoables = new List<Undoable>();
    
    private ModelGrid modelGrid = ModelGrid.Instance;
    
    public VoxelMesh ModelingHelperMesh = new VoxelMesh();
    public ShaderProgram ModelingHelperShader = new ShaderProgram("Model/ModelHelper.vert", "Model/ModelHelper.frag");
    
    public UIController Ui = new UIController();
    public UIController MainUi = new UIController();
    
    
    // Ui
    public StaticText MeshAlphaText;
    public StaticButton MeshAlphaButton;
    
    public StaticText BackfaceCullingText;
    public StaticButton BackfaceCullingButton;

    public StaticText SnappingText;
    public StaticButton SnappingButton;
    public StaticButton SnappingButtonUp;
    public StaticButton SnappingButtonDown;
    
    public StaticInputField InputField;
    
    // Ui values
    public float MeshAlpha = 0.1f;
    public bool BackfaceCulling = true;
    public bool Snapping = false;
    public float SnappingFactor = 1;
    private int SnappingFactorIndex = 0;
    private Vector3 SnappingOffset = new Vector3(0, 0, 0);
    
    
    
    
    private float _mouseX = 0;
    
    private bool _command = false;
    
    
    private List<Vertex> _selectedVertices = new List<Vertex>();

    private static readonly Dictionary<int, float> SnappingFactors = new Dictionary<int, float>()
    {
        { 0, 1f },
        { 1, 0.5f },
        { 2, 0.25f },
        { 3, 0.2f },
        { 4, 0.1f }
    };
    
    
    
    
    public override void Awake()
    {
        Console.WriteLine("Animation Editor");
        
        camera.SetCameraMode(CameraMode.Free);
        
        camera.position = new Vector3(0, 0, 5);
        camera.pitch = 0;
        camera.yaw = -90;
        
        camera.UpdateVectors();
        
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);
        
        //_modelMesh.WorldPosition = transform.Position + new Vector3(0, 0, 0);
    }
    
    public override void Start()
    {
        Console.WriteLine("Animation Editor");
        
        camera = new Camera(Game.width, Game.height, new Vector3(0, 0, 5));
        
        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
        
        transform.Position = new Vector3(0, 0, 0);
        
        go = new GameObject();

        _modelMesh = new ModelMesh();

        Vertex vertex1 = new Vertex((0, 0, 0));
        Vertex vertex2 = new Vertex((2, 0, 0));
        Vertex vertex3 = new Vertex((0, 2, 0));
        
        Vertex vertex4 = new Vertex((0, 2, 0));
        Vertex vertex5 = new Vertex((2, 0, 0));
        Vertex vertex6 = new Vertex((2, 2, 0));
        
        vertex2.AddSharedVertex(vertex5);
        vertex3.AddSharedVertex(vertex4);
        
        Triangle triangle = new Triangle(vertex1, vertex2, vertex3);
        Triangle triangle2 = new Triangle(vertex4, vertex5, vertex6);
        
        _modelMesh.AddTriangle(triangle);
        _modelMesh.AddTriangle(triangle2);
        
        _modelMesh.Init();
        _modelMesh.GenerateBuffers();
        _modelMesh.UpdateMesh();
        
        MeshAlphaText = UI.CreateStaticText("test" , "alpha: " + MeshAlpha.ToString("F2"), 0.7f, AnchorType.TopLeft, PositionType.Absolute, null, new Vector4(15, 10, 40, 10));
        BackfaceCullingText = UI.CreateStaticText("test2", "culling: off", 0.7f, AnchorType.TopLeft, PositionType.Absolute, null, new Vector4(15, 10, 65, 10));
        SnappingText = UI.CreateStaticText("test3", "snap: " + Snapping, 0.7f, AnchorType.TopLeft, PositionType.Absolute, null, new Vector4(15, 10, 90, 10));
        
        /*
        MainUi.AddStaticElement(MeshAlphaText);
        MainUi.AddStaticElement(BackfaceCullingText);
        MainUi.AddStaticElement(SnappingText);
        */
        
        gameObject.Scene.UiController = MainUi;
        
        gameObject.Scene.LoadUi();

        StaticText? text = MainUi.GetText("CullingText");
        
        if (text != null) 
            BackfaceCullingText = text;
        text = MainUi.GetText("AlphaText");
        if (text != null) 
            MeshAlphaText = text;
        text = MainUi.GetText("SnappingText");
        if (text != null) 
            SnappingText = text;
        
        MainUi.Generate();
        Ui.Generate();
        
        //gameObject.Scene.SaveUi();
    }
    
    public void AssignInputField(string test)
    {
        StaticInputField? inputField = MainUi.GetInputField(test);
        if (inputField != null)
            UIController.activeInputField = inputField;
    }
    
    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        MeshAlpha += mouseX * GameTime.DeltaTime * 0.2f;
        MeshAlpha = Mathf.Clamp(0, 1, MeshAlpha);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"));
        MeshAlphaText.Generate();
        MainUi.Update();
    }

    public void BackFaceCullingSwitch()
    {
        BackfaceCulling = !BackfaceCulling;
        BackfaceCullingText.SetText("culling: " + BackfaceCulling);
        BackfaceCullingText.Generate();
        MainUi.Update();
    }

    public void SnappingUpButton()
    {
        if (SnappingFactorIndex < 4)
        {
            SnappingFactorIndex++;
            SnappingFactor = SnappingFactors[SnappingFactorIndex];
        }

        UpdateSnappingText();
    }
    
    public void SnappingDownButton()
    {
        if (SnappingFactorIndex > 0)
        {
            SnappingFactorIndex--;
            SnappingFactor = SnappingFactors[SnappingFactorIndex];
        }
        UpdateSnappingText();
    }
    
    public void SnappingSwitchButton()
    {
        Snapping = !Snapping;
        UpdateSnappingText();
    }
    
    public override void Update()
    {
        MainUi.Test();

        if (UIController.activeInputField != null)
        {
            if (Input.IsKeyPressed(Keys.Escape))
                UIController.activeInputField = null;
            
            return;
        }

        if (Input.IsKeyPressed(Keys.Escape))
        {
            freeCamera = !freeCamera;
            
            if (freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                camera.firstMove = true;
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
            }
        }

        if (Input.IsKeyPressed(Keys.Delete))
        {
            _selectedVertices.Clear();
            GenerateVertexColor();
        }
        
        if (freeCamera)
        {
            camera.Update();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            // New Face
            if (Input.IsKeyPressed(Keys.F))
            {
                if (_selectedVertices.Count == 3)
                {
                    Vertex s1 = _selectedVertices[0];
                    Vertex s2 = _selectedVertices[1];
                    Vertex s3 = _selectedVertices[2];
                
                    Vertex v1 = new Vertex(s1);
                    Vertex v2 = new Vertex(s2);
                    Vertex v3 = new Vertex(s3);
                
                    Triangle triangle = new Triangle(v1, v2, v3);
                
                    s1.AddSharedVertexToAll(s1.ToList(), v1);
                    s2.AddSharedVertexToAll(s2.ToList(), v2);
                    s3.AddSharedVertexToAll(s3.ToList(), v3);
                
                    _modelMesh.AddTriangle(triangle);
                
                    _modelMesh.Init();
                    _modelMesh.GenerateBuffers();
                    _modelMesh.UpdateMesh();
                }
            }
            
            // Flipping triangle
            if (Input.IsKeyPressed(Keys.I))
            {
                HashSet<Triangle> triangles = GetSelectedFullTriangles();
                if (triangles.Count > 0)
                {
                    foreach (var triangle in triangles)
                    {
                        Vertex A = triangle.A;
                        Vertex B = triangle.B;
                        if (_modelMesh.SwapVertices(A, B))
                        {
                            triangle.Invert();
                            _modelMesh.UpdateNormals(triangle);
                        }
                    }
                    _modelMesh.Init();
                    _modelMesh.UpdateMesh();
                }
            }
            
            // Deleting triangle
            if (Input.IsKeyPressed(Keys.D))
            {
                HashSet<Triangle> triangles = GetSelectedFullTriangles();
                if (triangles.Count > 0)
                {
                    foreach (var triangle in triangles)
                    {
                        _modelMesh.RemoveTriangle(triangle);
                    }
                    _selectedVertices.Clear();
                
                    _modelMesh.CheckUselessTriangles();
                
                    _modelMesh.Init();
                    _modelMesh.GenerateBuffers();
                    _modelMesh.UpdateMesh();
                
                    Ui.Clear();
                
                    regenerateVertexUi = true;
                }
            }
            
            // Merging
            if (Input.IsKeyPressed(Keys.K) && _selectedVertices.Count >= 2)
            {
                Console.WriteLine("Merging verts");
                
                Vertex s1 = _selectedVertices[0];
                Vector3 position = s1.Position;

                HashSet<Vertex> deletedVertices = new HashSet<Vertex>();
                
                for (int i = 1; i < _selectedVertices.Count; i++)
                {
                    Vertex vert = _selectedVertices[i];
                    s1.AddSharedVertexToAll(s1.ToList(), vert);
                    position += vert.Position;
                    if (
                        vert.GetTwoOtherVertex(out var a, out var b) && (
                        SelectedContainsSharedVertex(a) || 
                        SelectedContainsSharedVertex(b)
                    )) {
                        Console.WriteLine("Remove Triangle");
                        _modelMesh.RemoveTriangle(vert.ParentTriangle);
                        deletedVertices.Add(vert);
                    }
                }
                
                position /= _selectedVertices.Count;
                s1.SetAllPosition(position);

                foreach (var vert in deletedVertices)
                {
                    _selectedVertices.Remove(vert);
                }
                
                for (int i = 1; i < _selectedVertices.Count; i++)
                {
                    _selectedVertices[i].AddSharedVertexToAll(_selectedVertices[i].ToList(), s1);
                    _selectedVertices[i].SetAllPosition(position);
                }
                
                _selectedVertices.Clear();
                
                _modelMesh.CheckUselessTriangles();
                
                _modelMesh.Init();
                _modelMesh.GenerateBuffers();
                _modelMesh.UpdateMesh();
                
                Ui.Clear();
                
                regenerateVertexUi = true;
            }
            
            // Extruding
            if (Input.IsKeyPressed(Keys.E))
            {
                Console.WriteLine("Extruding verts");
            
                Ui.Clear();
            
                if (_selectedVertices.Count < 2)
                    return;
            
                Vertex s1 = _selectedVertices[0];
                Vertex s2 = _selectedVertices[1];

                Vertex v1 = new Vertex(s1);
                Vertex v2 = new Vertex(s1);
                Vertex v3 = new Vertex(s2);
                Vertex v4 = new Vertex(s2);

                Quad quad = new Quad(v1, v2, v3, v4);
            
                Console.WriteLine(quad);
            
                _modelMesh.AddTriangle(quad.A);
                _modelMesh.AddTriangle(quad.B);
            
                _modelMesh.Init();
                _modelMesh.GenerateBuffers();
                _modelMesh.UpdateMesh();
            
                _selectedVertices.Clear();

                s1.AddSharedVertexToAll(s1.ToList(), v1);
                s2.AddSharedVertexToAll(s2.ToList(), v3);
            
                _selectedVertices.Add(v2);
                _selectedVertices.Add(v4);
            }

            // Move start
            if (Input.IsKeyPressed(Keys.G))
            {
                Ui.Clear();
            }
            
            // Moving
            if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G))
            {
                Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
                Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

                if (Input.AreKeysDown(out int index, Keys.X, Keys.C, Keys.V))
                    move *= AxisIgnore[index];
            
                if (Snapping)
                {
                    Vector3 Offset = Vector3.Zero;
                    SnappingOffset += move;
                    if (SnappingOffset.X > SnappingFactor)
                    {
                        Offset.X = SnappingFactor;
                        SnappingOffset.X -= SnappingFactor;
                    }
                    if (SnappingOffset.X < -SnappingFactor)
                    {
                        Offset.X = -SnappingFactor;
                        SnappingOffset.X += SnappingFactor;
                    }
                    if (SnappingOffset.Y > SnappingFactor)
                    {
                        Offset.Y = SnappingFactor;
                        SnappingOffset.Y -= SnappingFactor;
                    }
                    if (SnappingOffset.Y < -SnappingFactor)
                    {
                        Offset.Y = -SnappingFactor;
                        SnappingOffset.Y += SnappingFactor;
                    }
                    if (SnappingOffset.Z > SnappingFactor)
                    {
                        Offset.Z = SnappingFactor;
                        SnappingOffset.Z -= SnappingFactor;
                    }
                    if (SnappingOffset.Z < -SnappingFactor)
                    {
                        Offset.Z = -SnappingFactor;
                        SnappingOffset.Z += SnappingFactor;
                    }
                    move = Offset;
                }
 
                MoveSelectedVertices(move);
            
                _modelMesh.RecalculateNormals();
                _modelMesh.Init();
                _modelMesh.UpdateMesh();
            }
        }

        if (Input.IsKeyReleased(Keys.G) || Input.IsKeyReleased(Keys.E))
        {
            SnappingOffset = Vector3.Zero;
            regenerateVertexUi = true;
        }
        
        //Generate panels on top of eahc vertex
        if (freeCamera && !regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            Ui.Clear();
            regenerateVertexUi = true;
        }
        
        if (!freeCamera)
        {
            if (regenerateVertexUi)
            {
                Console.WriteLine("Regenerate Vertex UI");
                GenerateVertexPanels();
                regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                if (!Input.IsKeyDown(Keys.LeftShift))
                    _selectedVertices.Clear();
                
                Vector2 mousePos = Input.GetMousePosition();
                Vector2? closest = null;
                Vertex? closestVert = null;
            
                System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
                System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
            
                foreach (var vert in _modelMesh.VertexList)
                {
                    Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
                    if (screenPos == null)
                        continue;
                    float distance = Vector2.Distance(mousePos, (Vector2)screenPos);
                    float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
                
                    if (distance < distanceClosest && distance < 10)
                    {
                        closest = screenPos;
                        closestVert = vert;
                    }
                }

                if (closestVert != null && !_selectedVertices.Remove(closestVert))
                    _selectedVertices.Add(closestVert);
                
                GenerateVertexColor();
            }
        }
    }

    public override void Render()
    {
        if (BackfaceCulling)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);
        
        GL.Disable(EnableCap.DepthTest);
        
        _shaderProgram.Bind();
        
        MirrorRender(new Vector3(1, 1, 1));
        //MirrorRender(new Vector3(-1, 1, 1));
        
        //_playerMesh.RenderMesh();
        //_modelMesh.RenderMesh();
        
        _shaderProgram.Unbind();
        
        /*
        GL.Viewport(-70, -30, 320, 220);
        
        ModelingHelperShader.Bind();

        model = Matrix4.CreateTranslation(0, 0, 0);
        view = Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 0, -1f), Vector3.UnitY);
        projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(60),
            160f / 110f, 
            0.1f, 
            1000f
        );
        
        int modelModelLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "model");
        int modelViewLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "view");
        int modelProjectionLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "projection");
        
        GL.UniformMatrix4(modelModelLoc, true, ref model);
        GL.UniformMatrix4(modelViewLoc, true, ref view);
        GL.UniformMatrix4(modelProjectionLoc, true, ref projection);
        
        ModelingHelperMesh.RenderMesh();
        
        ModelingHelperShader.Unbind();
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        
        GL.Viewport(0, 0, Game.width, Game.height);
        */
        
        Ui.Render();
        //MainUi.Render();
        
        base.Render();
    }

    public void MirrorRender(Vector3 flipping)
    {
        Matrix4 model = Matrix4.CreateScale(flipping);
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform1(colorAlphaLocation, MeshAlpha);
        
        _modelMesh.RenderMesh();
    }

    public override void Exit()
    {
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);
        
        base.Exit();
    }
    
    private void GenerateVertexPanels()
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        foreach (var vert in _modelMesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
            
            vert.WentThrough = true;
            
            Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
            if (screenPos == null)
                continue;
                
            Vector2 pos = (Vector2)screenPos;
                
            StaticPanel panel = UI.CreateStaticPanel(pos.ToString(), AnchorType.TopLeft, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
            panel.SetPosition(new Vector3(pos.X, pos.Y, 0));
            panel.TextureIndex = SelectedContainsSharedVertex(vert) ? 2 : 1;
                
            Ui.AddStaticElement(panel);
        }
        
        _modelMesh.ResetVertex();
            
        Ui.Generate();
        Ui.Update();
    }

    public void GenerateVertexColor()
    {
        Console.WriteLine("Generate Vertex Color");
        
        Ui.ClearUiMesh();
        
        int i = 0;
        foreach (var vert in _modelMesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
            
            vert.WentThrough = true;
            
            Ui.SetStaticPanelTexureIndex(i, _selectedVertices.Contains(vert) ? 2 : 1);
            i++;
        }
        
        _modelMesh.ResetVertex();
        
        Ui.Generate();
        Ui.Update();
    }
    
    private bool SelectedContainsSharedVertex(Vertex vertex)
    {
        return vertex.SharedVertices.Any(vert => _selectedVertices.Contains(vert)) || _selectedVertices.Contains(vertex);
    }
    
    private void MoveSelectedVertices(Vector3 move)
    {
        
        foreach (var vert in _selectedVertices)
        {
            vert.MoveVertex(move);
        }
    }

    private void ModelFaceColor(int xy, int xz, int yz, int[] xyz)
    {
        switch (symmetry)
        {
            case Symmetry.XY:
                ModelingHelperMesh.SetFaceColor(0, xy, 3);
                break;
            case Symmetry.XZ:
                ModelingHelperMesh.SetFaceColor(0, xz, 3);
                break;
            case Symmetry.YZ:
                ModelingHelperMesh.SetFaceColor(0, yz, 3);
                break;
            case Symmetry.XYZ:
            {
                foreach (var t in xyz)
                {
                    ModelingHelperMesh.SetFaceColor(0, t, 3);
                }

                break;
            }
        }
    }

    public HashSet<Triangle> GetSelectedFullTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
        HashSet<Triangle> selectedTriangles = new HashSet<Triangle>();
                
        foreach (var vert in _selectedVertices)
        {
            IEnumerable<Vertex> vertToCheck = vert.SharedVertices.Concat([vert]);
            
            foreach (var svert in vertToCheck)
            {
                if (svert.ParentTriangle == null || !triangles.Add(svert.ParentTriangle))
                    continue;

                if (IsTriangleFullySelected(svert.ParentTriangle))
                    selectedTriangles.Add(svert.ParentTriangle);
            }
        }
        
        return selectedTriangles;
    }
    
    public bool IsTriangleFullySelected(Triangle triangle)
    {
        return SelectedContainsSharedVertex(triangle.A) &&
               SelectedContainsSharedVertex(triangle.B) &&
               SelectedContainsSharedVertex(triangle.C);
    }

    private void ModelCornerColor(int index, int color)
    {
        //index is vertex index
        if (index is < 0 or > 8)
            return;
        
        //index is displaced by 1 in mesh
        
        ModelingHelperMesh.SetVoxelColor(index + 1, color);

        int[] indexes = [];
        
        if (symmetry == Symmetry.X)
        {
            indexes = ModelData.SymmetryHelper[index][0];
        }
        else if (symmetry == Symmetry.Y)
        {
            indexes = ModelData.SymmetryHelper[index][1];
        }
        else if (symmetry == Symmetry.Z)
        {
            indexes = ModelData.SymmetryHelper[index][2];
        }
        else if (symmetry == Symmetry.XY)
        {
            indexes = ModelData.SymmetryHelper[index][3];
        }
        else if (symmetry == Symmetry.XZ)
        {
            indexes = ModelData.SymmetryHelper[index][4];
        }
        else if (symmetry == Symmetry.YZ)
        {
            indexes = ModelData.SymmetryHelper[index][5];
        }
        else if (symmetry == Symmetry.XYZ)
        {
            indexes = ModelData.SymmetryHelper[index][6];
        }
        
        for (int i = 0; i < indexes.Length; i++)
        {
            ModelingHelperMesh.SetVoxelColor(indexes[i] + 1, color);
        }
    }

    public void UpdateSnappingText()
    {
        if (Snapping)
            SnappingText.SetText("snap: " + SnappingFactor.ToString("F2"));
        else
            SnappingText.SetText("snap: off");
        
        SnappingText.Generate();
        MainUi.Update();
    }
    
    
    private readonly Dictionary<Keys, float> StepDictionary = new()
    {
        {Keys.D1, 0.1f},
        {Keys.D2, 0.25f},
        {Keys.D3, 0.5f},
        {Keys.D4, 1f},
    };
    
    private readonly List<Vector3> AxisIgnore = new()
    {
        new Vector3(0, 1, 1), // X
        new Vector3(1, 0, 1), // Y
        new Vector3(1, 1, 0), // Z
    };
}

public abstract class Undoable
{
    
}

public class AngleUndo(Vector3 oldAngle) : Undoable
{
    public Vector3 OldAngle = oldAngle;
}

public class ModelUndo(ModelNode oldNode) : Undoable
{
    public ModelNode OldNode = oldNode;
}