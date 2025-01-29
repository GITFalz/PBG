using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class GeneralModelingEditor : Component
{
    public BaseEditor CurrentEditor;
    public ModelingEditor modelingEditor = new ModelingEditor();
    public RiggingEditor riggingEditor = new RiggingEditor();
    public AnimationEditor animationEditor = new AnimationEditor();

    public UIController MainUi = new UIController();
    public UIController Ui = new UIController();
    public UIController ModelingUi = new UIController();
    public Model model = new Model();

    // Ui Elements
    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIInputField SnappingText;
    public static UIText MirrorText;
    public static UIInputField InputField;
    public static UIText BonePivotX;
    public static UIText BonePivotY;
    public static UIText BonePivotZ;
    public static UIText BoneEndX;
    public static UIText BoneEndY;
    public static UIText BoneEndZ;

    public float MeshAlpha 
    {
        get => ModelSettings.MeshAlpha;
        set => ModelSettings.MeshAlpha = value;
    }

    public bool BackfaceCulling
    {
        get => ModelSettings.BackfaceCulling;
        set => ModelSettings.BackfaceCulling = value;
    }

    public bool Snapping
    {
        get => ModelSettings.Snapping;
        set => ModelSettings.Snapping = value;
    }

    public float SnappingFactor
    {
        get => ModelSettings.SnappingFactor;
        set => ModelSettings.SnappingFactor = value;
    }

    public int SnappingFactorIndex
    {
        get => ModelSettings.SnappingFactorIndex;
        set => ModelSettings.SnappingFactorIndex = value;
    }

    public Vector3 SnappingOffset
    {
        get => ModelSettings.SnappingOffset;
        set => ModelSettings.SnappingOffset = value;
    }

    public Vector3i Mirror
    {
        get => ModelSettings.mirror;
        set => ModelSettings.mirror = value;
    }

    public int MirrorX
    {
        get => ModelSettings.mirror.X;
        set => ModelSettings.mirror.X = value;
    }

    public int MirrorY
    {
        get => ModelSettings.mirror.Y;
        set => ModelSettings.mirror.Y = value;
    }

    public int MirrorZ
    {
        get => ModelSettings.mirror.Z;
        set => ModelSettings.mirror.Z = value;
    }

    
    public bool freeCamera = false;
    public int _selectedModel = 0;
    public bool regenerateVertexUi = false;
    public Bone? _selectedBone = null;
    
    
    public List<Vertex> _selectedVertices = new List<Vertex>();
    public List<Triangle> _selectedTriangles = new List<Triangle>();
    public Dictionary<Vertex, VertexPanel> _vertexPanels = new Dictionary<Vertex, VertexPanel>();

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
        
        Game.camera = new Camera(Game.width, Game.height, new Vector3(0, 0, 5));
        ModelSettings.Camera = Game.camera;
        ModelSettings.Model = model;
        
        transform.Position = new Vector3(0, 0, 0);

        gameObject.Scene.UIControllers.Add(MainUi);
        gameObject.Scene.UIControllers.Add(Ui);
        gameObject.Scene.UIControllers.Add(ModelingUi);

        UIPanel statePanel = new("StatePanel", AnchorType.ScaleTop, PositionType.Absolute, (0, 0, 0), (Game.width, 40), (5, 5, 5, 5), 0, 0, (10, 0.15f), null);

        UIButton modelingButton = new("ModelingButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (75, 30), (5, 5, 5, 5), 0, 0, (10, 0.15f), null, UIState.Static);
        modelingButton.OnClick = new SerializableEvent(() => SwitchScene("Modeling"));

        UIButton riggingButton = new("RiggingButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (75, 30), (85, 5, 5, 5), 0, 0, (10, 0.15f), null, UIState.Static);
        riggingButton.OnClick = new SerializableEvent(() => SwitchScene("Rigging"));

        UIButton animationButton = new("AnimationButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (75, 30), (165, 5, 5, 5), 0, 0, (10, 0.15f), null, UIState.Static);
        animationButton.OnClick = new SerializableEvent(() => SwitchScene("Animation"));

        UIButton vertexSelectionButton = new("VertexSelectionButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (40, 40), (245, 0, 0, 0), 0, 97, (0, 0), null, UIState.Static);
        vertexSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(SelectionType.Vertex));

        UIButton edgeSelectionButton = new("EdgeSelectionButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (40, 40), (285, 0, 0, 0), 0, 98, (0, 0), null, UIState.Static);
        edgeSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(SelectionType.Edge));

        UIButton faceSelectionButton = new("FaceSelectionButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (40, 40), (325, 0, 0, 0), 0, 99, (0, 0), null, UIState.Static);
        faceSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(SelectionType.Face));

        statePanel.AddChild(modelingButton);
        statePanel.AddChild(riggingButton);
        statePanel.AddChild(animationButton);
        statePanel.AddChild(vertexSelectionButton);
        statePanel.AddChild(edgeSelectionButton);
        statePanel.AddChild(faceSelectionButton);


        UIPanel mainPanel = new("MainPanel", AnchorType.ScaleLeft, PositionType.Absolute, (0, 0, 0), (245, Game.height), (5, 50, 5, 5), 0, 0, (10, 0.15f), null);

        InputField = new("FileName", AnchorType.TopLeft, PositionType.Relative, (0, 80, 0), (400, 20), (10, 10, 10, 10), 0, 0, (10, 0.15f), null);
        InputField.SetText("cube", 0.7f);

        BackfaceCullingText = new("CullingText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, 35, 10, 10), 0, 0, (10, 0.15f), null);
        BackfaceCullingText.SetText("cull: " + BackfaceCulling, 0.7f);

        MeshAlphaText = new("AlphaText", AnchorType.TopLeft, PositionType.Relative, (0, 20, 0), (400, 20), (10, 60, 10, 10), 0, 0, (10, 0.15f), null);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"), 0.7f);

        SnappingText = new("SnappingText", AnchorType.TopLeft, PositionType.Relative, (0, 40, 0), (400, 20), (370, 13, 10, 10), 0, 0, (10, 0.15f), null);
        SnappingText.TextType = TextType.Decimal;
        SnappingText.MaxCharCount = 5;
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);
        SnappingText.SetText("0", 0.7f);

        MirrorText = new UIInputField("MirrorText", AnchorType.TopLeft, PositionType.Relative, (0, 60, 0), (400, 20), (10, 110, 10, 10), 0, 0, (10, 0.15f), null);
        MirrorText.SetText("mirror: " + (Mirror.X == 1 ? "X" : "-") + (Mirror.Y == 1 ? "Y" : "-") + (Mirror.Z == 1 ? "Z" : "-"), 0.7f);

        mainPanel.AddChild(InputField);
        mainPanel.AddChild(BackfaceCullingText);
        mainPanel.AddChild(MeshAlphaText);
        statePanel.AddChild(SnappingText);
        mainPanel.AddChild(MirrorText);


        UIButton cullingButton = new("CullingButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (40, 20), (-5, 35, 10, 10), 0, 0, (10, 0.15f), null, UIState.Static);
        cullingButton.OnClick = new SerializableEvent(BackFaceCullingSwitch);

        UIButton alphaButton = new("AlphaUpButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (40, 20), (-5, 60, 10, 10), 0, 0, (10, 0.15f), null, UIState.Static);
        alphaButton.OnHold = new SerializableEvent(AlphaControl);

        UIButton mirrorButton = new("MirrorButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (40, 20), (-5, 110, 10, 10), 0, 0, (10, 0.15f), null, UIState.Static);
        mirrorButton.OnClick = new SerializableEvent(ApplyMirror);

        UIButton mirrorZButton = new("MirrorZButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (15, 20), (-80, 110, 10, 10), 0, 0, (10, 0.15f), null, UIState.InvisibleInteractable);
        mirrorZButton.OnClick = new SerializableEvent(() => SwitchMirror("Z"));

        UIButton mirrorYButton = new("MirrorYButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (15, 20), (-95, 110, 10, 10), 0, 0, (10, 0.15f), null, UIState.InvisibleInteractable);
        mirrorYButton.OnClick = new SerializableEvent(() => SwitchMirror("Y"));

        UIButton mirrorXButton = new("MirrorXButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (15, 20), (-110, 110, 10, 10), 0, 0, (10, 0.15f), null, UIState.InvisibleInteractable);
        mirrorXButton.OnClick = new SerializableEvent(() => SwitchMirror("X"));


        mainPanel.AddChild(cullingButton);
        mainPanel.AddChild(alphaButton);

        mainPanel.AddChild(mirrorButton);
        mainPanel.AddChild(mirrorXButton);
        mainPanel.AddChild(mirrorYButton);
        mainPanel.AddChild(mirrorZButton);

        MainUi.AddElement(statePanel);
        ModelingUi.AddElement(mainPanel);


        Bone rootBone = new Bone("bone1");
        rootBone.Pivot = (1, 0, -1);
        rootBone.End = (1, 2, -1);
        model.Init(rootBone, "cube");
        
        MainUi.GenerateBuffers();
        ModelingUi.GenerateBuffers();
        Ui.GenerateBuffers();

        Camera camera = Game.camera;

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

    public override void OnResize()
    {
        MainUi.OnResize();
    }

    public override void Awake()
    {
        Game.BackgroundColor = (0.3f, 0.3f, 0.3f);
        CurrentEditor.Awake(this);
        base.Awake();
    }

    public override void Update()
    {
        ModelingUi.Test();
        CurrentEditor.Update(this);
        base.Update();
    }

    public override void Render()
    {
        CurrentEditor.Render(this);
        ModelingUi.Render();
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
    }

    public void RenderRigging()
    {
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        MeshAlpha = 1f;
        model.Render();
    }

    public void RenderModel()
    {
        if (BackfaceCulling)
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }

        model.Render();
    }

    #region Saved ui functions (Do not delete)

    public void SwitchMirror(string axis)
    {
        switch (axis)
        {
            case "X":
                MirrorX = MirrorX == 0 ? 1 : 0;
                break;
            case "Y":
                MirrorY = MirrorY == 0 ? 1 : 0;
                break;
            case "Z":
                MirrorZ = MirrorZ == 0 ? 1 : 0;
                break;
        }
        
        UpdateMirrorText();
    }
    
    public void ApplyMirror()
    {
        model.modelMesh.ApplyMirror();
        
        model.modelMesh.Init();
        model.modelMesh.GenerateBuffers();
        model.modelMesh.UpdateMesh();
        
        regenerateVertexUi = true;
        
        Mirror = new Vector3i(0, 0, 0);
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
    
    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        MeshAlpha += mouseX * GameTime.DeltaTime;
        MeshAlpha = Mathf.Clamp(0, 1, MeshAlpha);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2")).GenerateChars().UpdateText();
    }

    public void BackFaceCullingSwitch()
    {
        BackfaceCulling = !BackfaceCulling;
        BackfaceCullingText.SetText("cull: " + BackfaceCulling).GenerateChars().UpdateText();
    }

    public void SnappingField()
    {
        string text = SnappingText.Text.EndsWith('.') ? SnappingText.Text + "0" : SnappingText.Text;
        SnappingFactor = Mathf.Clamp(0, 100, Float.Parse(text));
        SnappingText.SetText(SnappingFactor.ToString() + (SnappingText.Text.EndsWith('.') ? "." : "")).GenerateChars().UpdateText();
        Snapping = SnappingFactor > 0;
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
        text += Mirror.X == 1 ? "X" : "-";
        text += Mirror.Y == 1 ? "Y" : "-";
        text += Mirror.Z == 1 ? "Z" : "-";
        
        MirrorText.SetText("mirror: " + text).GenerateChars().UpdateText();
    }

    public List<Link<Vector2>> GetLinkPositions(List<Link<Vector3>> worldLinks)
    {
        Camera camera = Game.camera;

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
        Camera camera = Game.camera;

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
        
        model.modelMesh.RecalculateNormals();
        model.modelMesh.Init();
        model.modelMesh.UpdateMesh();
    }

    public Vector3 GetSnappingMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        if (Input.AreKeysDown(out int index, Keys.X, Keys.C, Keys.V))
            move *= AxisIgnore[index];
        
        if (Snapping)
        {
            Vector3 Offset = Vector3.Zero;

            Vector3 snappingOffset = SnappingOffset;
            float snappingFactor = SnappingFactor;

            snappingOffset += move;
            if (snappingOffset.X > SnappingFactor)
            {
                Offset.X = SnappingFactor;
                snappingOffset.X -= SnappingFactor;
            }
            if (snappingOffset.X < -SnappingFactor)
            {
                Offset.X = -SnappingFactor;
                snappingOffset.X += SnappingFactor;
            }
            if (snappingOffset.Y > SnappingFactor)
            {
                Offset.Y = SnappingFactor;
                snappingOffset.Y -= SnappingFactor;
            }
            if (snappingOffset.Y < -SnappingFactor)
            {
                Offset.Y = -SnappingFactor;
                snappingOffset.Y += SnappingFactor;
            }
            if (snappingOffset.Z > SnappingFactor)
            {
                Offset.Z = SnappingFactor;
                snappingOffset.Z -= SnappingFactor;
            }
            if (snappingOffset.Z < -SnappingFactor)
            {
                Offset.Z = -SnappingFactor;
                snappingOffset.Z += SnappingFactor;
            }

            SnappingOffset = snappingOffset;
            SnappingFactor = snappingFactor;
        
            move = Offset;
        }

        return move;
    }

    public void Handle_FaceExtrusion()
    {
        Console.WriteLine("Extruding verts");
        
        if (_selectedVertices.Count < 2)
            return;
        
        //Ui.show();
        
        /*
        List<OldVertex> selectedVerts = new List<OldVertex>();
        HashSet<OldTriangle> triangles = GetSelectedFullTriangles();
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
        */
    }

    public void ExtrudeVerts(OldVertex A, OldVertex B, out OldVertex v1, out OldVertex v2, out OldVertex v3, out OldVertex v4)
    {
        OldVertex s1 = A;
        OldVertex s2 = B;

        v1 = new OldVertex(s1);
        v2 = new OldVertex(s1);
        v3 = new OldVertex(s2);
        v4 = new OldVertex(s2);

        Quad quad = new Quad(v1, v2, v3, v4);
        
        model.Mesh.AddTriangle(quad.A);
        model.Mesh.AddTriangle(quad.B);

        s1.AddSharedVertexToAll(s1.ToList(), v1);
        s2.AddSharedVertexToAll(s2.ToList(), v3);
    }

    public void Handle_VertexMerging()
    {
        Console.WriteLine("Merging verts");
                
        if (_selectedVertices.Count < 2)
            return;

        ModelMesh modelMesh = model.modelMesh;

        modelMesh.MergeVertices(_selectedVertices);
                
        _selectedVertices.Clear();

        modelMesh.Init();
        modelMesh.GenerateBuffers();
        modelMesh.UpdateMesh();
                
        Ui.Clear();
                
        regenerateVertexUi = true;
    }

    public void Handle_TriangleDeletion()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        ModelMesh modelMesh = model.modelMesh;

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                modelMesh.RemoveTriangle(triangle);
            }
            _selectedVertices.Clear();
                
            modelMesh.Init();
            modelMesh.GenerateBuffers();
            modelMesh.UpdateMesh();
                
            Ui.Clear();
                
            regenerateVertexUi = true;
        }
    }

    public void Handle_FlipTriangleNormal()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        ModelMesh modelMesh = model.modelMesh;

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Vertex A = triangle.A;
                Vertex B = triangle.B;
                if (modelMesh.SwapVertices(A, B))
                {
                    triangle.Invert();
                    modelMesh.UpdateNormals(triangle);
                }
            }

            modelMesh.Init();
            modelMesh.UpdateMesh();
        }
    }

    public void Handle_GenerateNewFace()
    {
        
    }

    public void Handle_SelectAllVertices()
    {
        Console.WriteLine("Select all");
        _selectedVertices.Clear();
                
        foreach (var vert in model.modelMesh.VertexList)
        {
            _selectedVertices.Add(vert);
        }
                
        GenerateVertexColor();
    }

    public void GenerateVertexPanels()
    {
        Ui.Clear();
        _vertexPanels.Clear();

        Camera camera = Game.camera;

        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        foreach (var vert in model.modelMesh.VertexList)
        {
            Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
            if (screenPos == null)
                continue;
                
            Vector2 pos = (Vector2)screenPos - (10, 10);
                
            UIPanel panel = new(pos.ToString(), AnchorType.TopLeft, PositionType.Free, (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, SelectedContainsVertex(vert) ? 11 : 10, (10, 0.15f), null);
            panel.SetPosition(new Vector3(pos.X, pos.Y, 0));
            Ui.AddElement(panel);

            _vertexPanels.Add(vert, new VertexPanel(panel, pos));
        }
            
        Ui.GenerateBuffers();
        Ui.Update();
    }

    public void GenerateVertexColor()
    {
        Console.WriteLine("Generate Vertex Color: " + _vertexPanels.Count + " " + _selectedVertices.Count + " " + Ui.Elements.Count);
        
        foreach (var (vert, panel) in _vertexPanels)
        {
            bool selected = SelectedContainsVertex(vert);
            panel.Panel.TextureIndex = selected ? 11 : 10;
            panel.Panel.UpdateTexture();
        }

        Ui.UpdateTextures();
    }
    
    public bool SelectedContainsVertex(Vertex vertex)
    {
        return _selectedVertices.Contains(vertex);
    }
    
    public void RemoveSelectedVertex(Vertex vertex)
    {
        _selectedVertices.Remove(vertex);
    }
    
    public void MoveSelectedVertices(Vector3 move)
    {
        foreach (var vert in _selectedVertices)
        {
            if (Snapping)
                vert.SnapPosition(move, SnappingFactor);
            else
                vert.MovePosition(move);
        }
    }

    public HashSet<Triangle> GetSelectedFullTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
                
        foreach (var triangle in GetSelectedTriangles())
        {
            if (IsTriangleFullySelected(triangle))
                triangles.Add(triangle);
        }
        
        return triangles;
    }

    public HashSet<Triangle> GetSelectedTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
                
        foreach (var vert in _selectedVertices)
        {
            foreach (var triangle in vert.ParentTriangles)
            {
                triangles.Add(triangle);
            }
        }
        
        return triangles;
    }
    
    public bool IsTriangleFullySelected(Triangle triangle)
    {
        return SelectedContainsVertex(triangle.A) &&
               SelectedContainsVertex(triangle.B) &&
               SelectedContainsVertex(triangle.C);
    }

    /*
    public void UpdateSnappingText()
    {
        if (Snapping)
            SnappingText.SetText("snap: " + SnappingFactor.ToString("F2"));
        else
            SnappingText.SetText("snap: off");
        
        SnappingText.Generate();
        MainUi.Update();
    }
    */
    
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

public class VertexPanel
{
    public UIPanel Panel;
    public Vector2 ScreenPosition;

    public VertexPanel(UIPanel panel, Vector2 screenPosition)
    {
        Panel = panel;
        ScreenPosition = screenPosition;
    }
}