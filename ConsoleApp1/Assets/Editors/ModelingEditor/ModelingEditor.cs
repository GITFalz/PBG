using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditor : Updateable
{
    private Camera camera;
    
    private ShaderProgram _shaderProgram;
    private AnimationMesh _playerMesh;
    private ModelMesh _modelMesh;
    
    private bool saveRotate = false;
    private bool saveMove = false;
    private bool regenerateVertexUi = true;
    
    private bool freeCamera = false;

    private int _selectedModel = 0;
    
    public UIController Ui = new UIController();
    public UIController MainUi = new UIController();
    
    
    // Ui
    public StaticText MeshAlphaText;
    public StaticText BackfaceCullingText;
    public StaticText SnappingText;

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
        Console.WriteLine("Animation Editor Start");
        
        camera = new Camera(Game.width, Game.height, new Vector3(0, 0, 5));
        
        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
        
        transform.Position = new Vector3(0, 0, 0);

        _modelMesh = new ModelMesh();
        
        gameObject.Scene.UiController = MainUi;
        
        gameObject.Scene.LoadUi();

        var t = MainUi.GetElement<StaticText>("CullingText");

        BackfaceCullingText = t ?? throw new NotFoundException("CullingText");
        BackfaceCulling = bool.Parse(BackfaceCullingText.Text.Split(' ')[1].Trim());
        
        t = MainUi.GetElement<StaticText>("AlphaText");
        
        MeshAlphaText = t ?? throw new NotFoundException("AlphaText");
        MeshAlpha = float.Parse(MeshAlphaText.Text.Split(' ')[1].Trim());
        
        t = MainUi.GetElement<StaticText>("SnappingText");
        
        SnappingText = t ?? throw new NotFoundException("SnappingText");
        Snapping = SnappingText.Text.Split(' ')[1].Trim() != "off";

        StaticInputField? inputField = MainUi.GetElement<StaticInputField>("FileName");
        InputField = inputField ?? throw new NotFoundException("inputField");

        LoadModel();
        
        MainUi.Generate();
        Ui.Generate();
    }
    
    #region Saved ui functions (Do not delete)

    public void SaveModel()
    {
        string modelName = InputField.Text;
        _modelMesh.SaveModel(modelName);
        
        _modelMesh.Init();
        _modelMesh.GenerateBuffers();
        _modelMesh.UpdateMesh();
    }
    
    public void LoadModel()
    {
        string modelName = InputField.Text;
        _modelMesh.LoadModel(modelName);
        
        _modelMesh.Init();
        _modelMesh.GenerateBuffers();
        _modelMesh.UpdateMesh();
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
    
    #endregion
    
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
            // Select all
            if (Input.IsKeyPressed(Keys.A)) Handle_SelectAllVertices();
            
            // New Face
            if (Input.IsKeyPressed(Keys.F)) Handle_GenerateNewFace();
            
            // Flipping triangle
            if (Input.IsKeyPressed(Keys.I)) Handle_FlipTriangleNormal();
            
            // Deleting triangle
            if (Input.IsKeyPressed(Keys.D)) Handle_TriangleDeletion();
            
            // Merging
            if (Input.IsKeyPressed(Keys.K) && _selectedVertices.Count >= 2) Handle_VertexMerging();
        }
        
        // Extrude
        if (Input.IsKeyPressed(Keys.E)) Handle_FaceExtrusion();

        // Move start
        if (Input.IsKeyPressed(Keys.G)) Ui.Clear();
        
        // Moving
        if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G)) Handle_MovingSelectedVertices();

        if (Input.IsKeyReleased(Keys.E))
        {
            _modelMesh.CheckUselessTriangles();
            _modelMesh.CombineDuplicateVertices();
            _modelMesh.RecalculateNormals();
            _modelMesh.Init();
            _modelMesh.UpdateMesh();
            
            SnappingOffset = Vector3.Zero;
            regenerateVertexUi = true;
        }

        if (Input.IsKeyReleased(Keys.G))
        {
            SnappingOffset = Vector3.Zero;
            regenerateVertexUi = true;
        }
        
        //Generate panels on top of each vertex
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

    private void Handle_MovingSelectedVertices()
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

    private void Handle_FaceExtrusion()
    {
        Console.WriteLine("Extruding verts");
        
        if (_selectedVertices.Count < 2)
            return;
        
        Ui.Clear();
        
        List<Vertex> selectedVerts = new List<Vertex>();
        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        foreach (var triangle in triangles)
        {
            _selectedVertices.Remove(triangle.A);
            _selectedVertices.Remove(triangle.B);
            _selectedVertices.Remove(triangle.C);

            ExtrudeVerts(triangle.A, triangle.B, out var _, out var v21, out _, out var v41);
            ExtrudeVerts(triangle.B, triangle.C, out _, out var v22, out _, out var v42);
            ExtrudeVerts(triangle.C, triangle.A, out _, out var v23, out _, out var v43);
            
            selectedVerts.Add(v21);
            selectedVerts.Add(v22);
            selectedVerts.Add(v23);
            
            v21.AddSharedVertexToAll(v21.ToList(), v43);
            v22.AddSharedVertexToAll(v22.ToList(), v41);
            v23.AddSharedVertexToAll(v23.ToList(), v42);
        }
        
        _modelMesh.Init();
        _modelMesh.GenerateBuffers();
        _modelMesh.UpdateMesh();
        
        _selectedVertices = selectedVerts;
        
        /*
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
        */
    }

    public void ExtrudeVerts(Vertex A, Vertex B, out Vertex v1, out Vertex v2, out Vertex v3, out Vertex v4)
    {
        Vertex s1 = A;
        Vertex s2 = B;

        v1 = new Vertex(s1);
        v2 = new Vertex(s1);
        v3 = new Vertex(s2);
        v4 = new Vertex(s2);

        Quad quad = new Quad(v1, v2, v3, v4);
        
        _modelMesh.AddTriangle(quad.A);
        _modelMesh.AddTriangle(quad.B);

        s1.AddSharedVertexToAll(s1.ToList(), v1);
        s2.AddSharedVertexToAll(s2.ToList(), v3);
    }

    private void Handle_VertexMerging()
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

    private void Handle_TriangleDeletion()
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

    private void Handle_FlipTriangleNormal()
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

    private void Handle_GenerateNewFace()
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

    private void Handle_SelectAllVertices()
    {
        Console.WriteLine("Select all");
        _selectedVertices.Clear();
                
        foreach (var vert in _modelMesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
                    
            vert.WentThrough = true;
                    
            _selectedVertices.Add(vert);
        }
                
        _modelMesh.ResetVertex();
        GenerateVertexColor();
    }
    
    private void UpdateSelection()
    {
        Console.WriteLine("Select all");
        List<Vertex> newSelected = new List<Vertex>();
                
        foreach (var vert in _selectedVertices)
        {
            if (vert.WentThroughOne())
                continue;
                    
            vert.WentThrough = true;
                    
            newSelected.Add(vert);
        }
        
        _selectedVertices = newSelected;
                
        _modelMesh.ResetVertex();
        GenerateVertexColor();
    }

    public override void Render()
    {
        if (BackfaceCulling)
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
        }
        
        _shaderProgram.Bind();
        
        MirrorRender(new Vector3(1, 1, 1));
        
        _shaderProgram.Unbind();
        
        Ui.Render();
        
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
        
        gameObject.Scene.SaveUi();
        
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
    
    private void RemoveSelectedVertex(Vertex vertex)
    {
        // Iterate through all shared vertices and remove them
        foreach (var vert in vertex.SharedVertices)
        {
            if (_selectedVertices.Remove(vert))
                return;
        }
        _selectedVertices.Remove(vertex);
    }
    
    private void MoveSelectedVertices(Vector3 move)
    {
        
        foreach (var vert in _selectedVertices)
        {
            vert.MoveVertex(move);
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

                if (VertexTriangleIsSelected(svert, out var triangle) && triangle != null)
                    selectedTriangles.Add(triangle);
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
    
    public bool VertexTriangleIsSelected(Vertex vertex, out Triangle? triangle)
    {
        foreach (var vert in vertex.SharedVertices)
        {
            if (vert.ParentTriangle != null && IsTriangleFullySelected(vert.ParentTriangle))
            {
                triangle = vert.ParentTriangle;
                return true;
            }
        }
        
        if (vertex.ParentTriangle != null && IsTriangleFullySelected(vertex.ParentTriangle))
        {
            triangle = vertex.ParentTriangle;
            return true;
        }

        triangle = null;
        return false;
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