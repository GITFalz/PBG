using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class GeneralModelingEditor : Updateable
{
    public BaseEditor CurrentEditor;
    public ModelingEditor modelingEditor = new ModelingEditor();
    public RiggingEditor riggingEditor = new RiggingEditor();
    public AnimationEditor animationEditor = new AnimationEditor();


    public Camera camera;

    public StaticText BackfaceCullingText;

    public StaticText MeshAlphaText;

    public StaticText SnappingText;

    public StaticText MirrorText;
    public StaticInputField InputField;


    public OldUIController MainUi = new OldUIController();
    public OldUIController Ui = new OldUIController();


    public Model model = new Model();

    
    public bool freeCamera = false;

    public int _selectedModel = 0;

    public bool regenerateVertexUi = false;
    
    public OldUIController BoneUi = new OldUIController();
    
    public Bone? _selectedBone = null;
    
    public StaticInputField BonePivotX;
    public StaticInputField BonePivotY;
    public StaticInputField BonePivotZ;
    
    public StaticInputField BoneEndX;
    public StaticInputField BoneEndY;
    public StaticInputField BoneEndZ;
    
    
    public List<Vertex> _selectedVertices = new List<Vertex>();

    public ShaderProgram _shaderProgram;

    public static readonly Dictionary<int, float> SnappingFactors = new Dictionary<int, float>()
    {
        { 0, 1f },
        { 1, 0.5f },
        { 2, 0.25f },
        { 3, 0.2f },
        { 4, 0.1f }
    };

    public override void Start()
    {
        Console.WriteLine("Animation Editor Start");

        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
        
        camera = new Camera(Game.width, Game.height, new Vector3(0, 0, 5));
        ModelSettings.Camera = camera;
        
        transform.Position = new Vector3(0, 0, 0);
        
        gameObject.Scene.UiController = MainUi;
        gameObject.Scene.LoadUi();

        var t = MainUi.GetElement<StaticText>("CullingText");

        BackfaceCullingText = t ?? throw new NotFoundException("CullingText");
        ModelSettings.BackfaceCulling = bool.Parse(BackfaceCullingText.Text.Split(' ')[1].Trim());
        
        t = MainUi.GetElement<StaticText>("AlphaText");
        
        MeshAlphaText = t ?? throw new NotFoundException("AlphaText");
        ModelSettings.MeshAlpha = float.Parse(MeshAlphaText.Text.Split(' ')[1].Trim());
        
        t = MainUi.GetElement<StaticText>("SnappingText");
        
        SnappingText = t ?? throw new NotFoundException("SnappingText");
        ModelSettings.Snapping = SnappingText.Text.Split(' ')[1].Trim() != "off";
        
        t = MainUi.GetElement<StaticText>("MirrorText");
        
        MirrorText = t ?? throw new NotFoundException("MirrorText");
        string mirrorValues = MirrorText.Text.Split(' ')[1].Trim();
        
        ModelSettings.mirror = new Vector3i(
            mirrorValues[0] == 'X' ? 1 : 0,
            mirrorValues[1] == 'Y' ? 1 : 0,
            mirrorValues[2] == 'Z' ? 1 : 0
        );

        StaticInputField? inputField = MainUi.GetElement<StaticInputField>("FileName");
        InputField = inputField ?? throw new NotFoundException("inputField");

        Bone rootBone = new Bone("bone1");
        rootBone.Pivot = (1, 0, -1);
        rootBone.End = (1, 2, -1);
        model.Init(rootBone, "cube");
        
        MainUi.Generate();
        Ui.Generate();

        camera.SetCameraMode(CameraMode.Free);
        
        camera.position = new Vector3(0, 0, 5);
        camera.pitch = 0;
        camera.yaw = -90;
        
        camera.UpdateVectors();
        
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);

        CurrentEditor = modelingEditor;
        CurrentEditor.Start(this);
    }

    public override void Awake()
    {
        CurrentEditor.Awake(this);
        base.Awake();
    }

    public override void Update()
    {
        MainUi.Test();
        CurrentEditor.Update(this);
        base.Update();
    }

    public override void Render()
    {
        CurrentEditor.Render(this);
        base.Render();
    }

    public override void Exit()
    {
        CurrentEditor.Exit(this);
        base.Exit();
    }

    public void DoSwitchScene(BaseEditor editor)
    {
        CurrentEditor.Exit(this);
        CurrentEditor = editor;
        CurrentEditor.Start(this);
    }

    public void SwitchScene(string editor)
    {
        switch (editor)
        {
            case "Modeling":
                DoSwitchScene(modelingEditor);
                break;
            case "Rigging":
                DoSwitchScene(riggingEditor);
                break;
            case "Animation":
                DoSwitchScene(animationEditor);
                break;
        }
    }

    public void RenderAnimation()
    {
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        model.Render();

        Ui.Render();
    }

    public void RenderRigging()
    {
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        ModelSettings.MeshAlpha = 1f;
        model.Render();

        Ui.Render();
    }

    public void RenderModel()
    {
        if (ModelSettings.BackfaceCulling)
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }

        model.Render();
        
        Ui.Render();
    }

    #region Saved ui functions (Do not delete)

    public void SwitchMirror(string axis)
    {
        switch (axis)
        {
            case "X":
                ModelSettings.mirror.X = ModelSettings.mirror.X == 0 ? 1 : 0;
                break;
            case "Y":
                ModelSettings.mirror.Y = ModelSettings.mirror.Y == 0 ? 1 : 0;
                break;
            case "Z":
                ModelSettings.mirror.Z = ModelSettings.mirror.Z == 0 ? 1 : 0;
                break;
        }
        
        UpdateMirrorText();
    }
    
    public void ApplyMirror()
    {
        model.Mesh.ApplyMirror();
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
        
        regenerateVertexUi = true;
        
        ModelSettings.mirror = new Vector3i(0, 0, 0);
        UpdateMirrorText();
    }
    
    public void SaveModel()
    {
        string modelName = InputField.Text;
        if (modelName == "cube")
            return;

        model.Mesh.SaveModel(modelName);
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
    }
    
    public void LoadModel()
    {
        string modelName = InputField.Text;
        model.Mesh.LoadModel(modelName);
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
    }
    
    public void AssignInputField(string test)
    {
        StaticInputField? inputField = MainUi.GetInputField(test);
        if (inputField != null)
            OldUIController.activeInputField = inputField;
    }
    
    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        ModelSettings.MeshAlpha += mouseX * GameTime.DeltaTime * 0.2f;
        ModelSettings.MeshAlpha = Mathf.Clamp(0, 1, ModelSettings.MeshAlpha);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2"));
        MeshAlphaText.Generate();
        MainUi.Update();
    }

    public void BackFaceCullingSwitch()
    {
        ModelSettings.BackfaceCulling = !ModelSettings.BackfaceCulling;
        BackfaceCullingText.SetText("culling: " + ModelSettings.BackfaceCulling);
        BackfaceCullingText.Generate();
        MainUi.Update();
    }

    public void SnappingUpButton()
    {
        if (ModelSettings.SnappingFactorIndex < 4)
        {
            ModelSettings.SnappingFactorIndex++;
            ModelSettings.SnappingFactor = SnappingFactors[ModelSettings.SnappingFactorIndex];
        }

        UpdateSnappingText();
    }
    
    public void SnappingDownButton()
    {
        if (ModelSettings.SnappingFactorIndex > 0)
        {
            ModelSettings.SnappingFactorIndex--;
            ModelSettings.SnappingFactor = SnappingFactors[ModelSettings.SnappingFactorIndex];
        }
        UpdateSnappingText();
    }
    
    public void SnappingSwitchButton()
    {
        ModelSettings.Snapping = !ModelSettings.Snapping;
        UpdateSnappingText();
    }
    
    public void SetBonePivot(string axis)
    {
        if (_selectedBone == null)
            return;
        
        switch (axis)
        {
            case "X":
                _selectedBone.Pivot.X = float.Parse(BonePivotX.Text);
                break;
            case "Y":
                _selectedBone.Pivot.Y = float.Parse(BonePivotY.Text);
                break;
            case "Z":
                _selectedBone.Pivot.Z = float.Parse(BonePivotZ.Text);
                break;
        }
    }
    
    public void SetBoneEnd(string axis)
    {
        if (_selectedBone == null)
            return;
        
        switch (axis)
        {
            case "X":
                _selectedBone.End.X = float.Parse(BoneEndX.Text);
                break;
            case "Y":
                _selectedBone.End.Y = float.Parse(BoneEndY.Text);
                break;
            case "Z":
                _selectedBone.End.Z = float.Parse(BoneEndZ.Text);
                break;
        }
    }

    #endregion
    
    public void UpdateMirrorText()
    {
        string text = "";
        text += ModelSettings.mirror.X == 1 ? "X" : "-";
        text += ModelSettings.mirror.Y == 1 ? "Y" : "-";
        text += ModelSettings.mirror.Z == 1 ? "Z" : "-";
        
        MirrorText.SetText("mirror: " + text);
        MirrorText.Generate();
        MainUi.Update();
    }

    public List<Link<Vector2>> GetLinkPositions(List<Link<Vector3>> worldLinks)
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        List<Link<Vector2>> screenLinks = new List<Link<Vector2>>();
        
        foreach (var link in worldLinks)
        {
            Vector2? screenPosA = Mathf.WorldToScreen(link.A, projection, view);
            Vector2? screenPosB = Mathf.WorldToScreen(link.B, projection, view);
            if (screenPosA == null || screenPosB == null) continue;
            screenLinks.Add(new Link<Vector2>(screenPosA.Value, screenPosB.Value)); 
        }
        return screenLinks;
    }

    public List<Link<Vector2>> GetLinkPositions(List<Bone> worldLinks)
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        List<Link<Vector2>> screenLinks = new List<Link<Vector2>>();
        
        foreach (var bone in worldLinks)
        {
            Vector2? screenPosA = Mathf.WorldToScreen(bone.Pivot, projection, view);
            Vector2? screenPosB = Mathf.WorldToScreen(bone.End, projection, view);
            if (screenPosA == null || screenPosB == null) continue;
            screenLinks.Add(new Link<Vector2>(screenPosA.Value, screenPosB.Value)); 
        }
        return screenLinks;
    }

    public StaticPanel GeneratePanel(Vector2 position)
    {
        StaticPanel panel = UI.CreateStaticPanel(position.ToString(), AnchorType.TopLeft, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
        panel.SetPosition(new Vector3(position.X, position.Y, 0));
        panel.TextureIndex = 1;
        return panel;
    }
    
    public StaticPanel GeneratePanelLink(Vector2 pos1, Vector2 pos2, int textureIndex)
    {
        float distance = Vector2.Distance(pos1, pos2);
        StaticPanel panel = UI.CreateStaticPanel(pos1.ToString(), AnchorType.TopLeft, PositionType.Free, new Vector3(20, distance, 0), new Vector4(0, 0, 0, 0), null);
        panel.SetPosition(new Vector3(pos1.X, pos1.Y, 0));
        panel.SetRotation(new Vector3(pos1.X, pos1.Y, 0), -Mathf.GetAngleBetweenPoints(pos1, pos2));
        panel.SetOriginType(OriginType.Pivot);
        panel.TextureIndex = textureIndex;
        return panel;
    }

    public void Handle_MovingSelectedVertices()
    {
        Vector3 move = GetSnappingMovement();
        MoveSelectedVertices(move);
        
        model.Mesh.RecalculateNormals();
        model.Mesh.InitModel();
        model.Mesh.UpdateMesh();
    }

    public Vector3 GetSnappingMovement()
    {
        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        if (Input.AreKeysDown(out int index, Keys.X, Keys.C, Keys.V))
            move *= AxisIgnore[index];
        
        if (ModelSettings.Snapping)
        {
            Vector3 Offset = Vector3.Zero;

            Vector3 SnappingOffset = ModelSettings.SnappingOffset;
            float SnappingFactor = ModelSettings.SnappingFactor;

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

            ModelSettings.SnappingOffset = SnappingOffset;
            ModelSettings.SnappingFactor = SnappingFactor;
        
            move = Offset;
        }

        return move;
    }

    public void Handle_FaceExtrusion()
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
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
        
        _selectedVertices = selectedVerts;
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
        
        model.Mesh.AddTriangle(quad.A);
        model.Mesh.AddTriangle(quad.B);

        s1.AddSharedVertexToAll(s1.ToList(), v1);
        s2.AddSharedVertexToAll(s2.ToList(), v3);
    }

    public void Handle_VertexMerging()
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
                model.Mesh.RemoveTriangle(vert.ParentTriangle);
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
                
        model.Mesh.CheckUselessTriangles();
                
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
                
        Ui.Clear();
                
        regenerateVertexUi = true;
    }

    public void Handle_TriangleDeletion()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                model.Mesh.RemoveTriangle(triangle);
            }
            _selectedVertices.Clear();
                
            model.Mesh.CheckUselessTriangles();
                
            model.Mesh.InitModel();
            model.Mesh.GenerateBuffers();
            model.Mesh.UpdateMesh();
                
            Ui.Clear();
                
            regenerateVertexUi = true;
        }
    }

    public void Handle_FlipTriangleNormal()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Vertex A = triangle.A;
                Vertex B = triangle.B;
                if (model.Mesh.SwapVertices(A, B))
                {
                    triangle.Invert();
                    model.Mesh.UpdateNormals(triangle);
                }
            }
            model.Mesh.InitModel();
            model.Mesh.UpdateMesh();
        }
    }

    public void Handle_GenerateNewFace()
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
                
            model.Mesh.AddTriangle(triangle);
                
            model.Mesh.InitModel();
            model.Mesh.GenerateBuffers();
            model.Mesh.UpdateMesh();
        }
    }

    public void Handle_SelectAllVertices()
    {
        Console.WriteLine("Select all");
        _selectedVertices.Clear();
                
        foreach (var vert in model.Mesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
                    
            vert.WentThrough = true;
                    
            _selectedVertices.Add(vert);
        }
                
        model.Mesh.ResetVertex();
        GenerateVertexColor();
    }

    public void GenerateVertexPanels()
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        foreach (var vert in model.Mesh.VertexList)
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
        
        model.Mesh.ResetVertex();
            
        Ui.Generate();
        Ui.Update();
    }

    public void GenerateVertexColor()
    {
        Console.WriteLine("Generate Vertex Color");
        
        Ui.ClearUiMesh();
        
        int i = 0;
        foreach (var vert in model.Mesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
            
            vert.WentThrough = true;
            
            Ui.SetStaticPanelTexureIndex(i, _selectedVertices.Contains(vert) ? 2 : 1);
            i++;
        }
        
        model.Mesh.ResetVertex();
        
        Ui.Generate();
        Ui.Update();
    }
    
    public bool SelectedContainsSharedVertex(Vertex vertex)
    {
        return vertex.SharedVertices.Any(vert => _selectedVertices.Contains(vert)) || _selectedVertices.Contains(vertex);
    }
    
    public void RemoveSelectedVertex(Vertex vertex)
    {
        // Iterate through all shared vertices and remove them
        foreach (var vert in vertex.SharedVertices)
        {
            if (_selectedVertices.Remove(vert))
                return;
        }
        _selectedVertices.Remove(vertex);
    }
    
    public void MoveSelectedVertices(Vector3 move)
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
        if (ModelSettings.Snapping)
            SnappingText.SetText("snap: " + ModelSettings.SnappingFactor.ToString("F2"));
        else
            SnappingText.SetText("snap: off");
        
        SnappingText.Generate();
        MainUi.Update();
    }
    
    public readonly List<Vector3> AxisIgnore = new()
    {
        new Vector3(0, 1, 1), // X
        new Vector3(1, 0, 1), // Y
        new Vector3(1, 1, 0), // Z
    };
}

public abstract class BaseEditor 
{ 
    public abstract void Start(GeneralModelingEditor editor);
    public abstract void Awake(GeneralModelingEditor editor);
    public abstract void Update(GeneralModelingEditor editor);
    public abstract void Render(GeneralModelingEditor editor);
    public abstract void Exit(GeneralModelingEditor editor);
}

public class Link<T>(T a, T b) where T : struct
{
    public T A = a;
    public T B = b;
}