using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class GeneralModelingEditor : ScriptingNode
{
    public BaseEditor CurrentEditor;
    public ModelingEditor modelingEditor = new ModelingEditor();
    public RiggingEditor riggingEditor = new RiggingEditor();
    public AnimationEditor animationEditor = new AnimationEditor();

    public UIController MainUi = new UIController();
    public UIController ModelingUi = new UIController();
    public Model model = new Model();

    public string currentModelName = "cube";

    // Ui Elements
    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIInputField SnappingText;
    public static UIText MirrorText;
    public static UIText BonePivotX;
    public static UIText BonePivotY;
    public static UIText BonePivotZ;
    public static UIText BoneEndX;
    public static UIText BoneEndY;
    public static UIText BoneEndZ;

    // Main UI
    public static UIInputField FileName;

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
        
        Game.camera = new Camera(Game.Width, Game.Height, new Vector3(0, 0, 5));
        ModelSettings.Camera = Game.camera;
        ModelSettings.Model = model;
        
        Transform.Position = new Vector3(0, 0, 0);

        UICollection stateCollection = new("StateCollection", AnchorType.ScaleTop, PositionType.Absolute, (0, 0, 0), (Game.Width, 50), (5, 5, 5, 5), 0);

        UIImage statePanel = new("StatePanel", AnchorType.ScaleTop, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (0, 0, 0), (0, 50), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);


        UICollectionHorizontalStacking stateStacking = new("StateStacking", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        UIButton modelingButton = new("ModelingButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null, UIState.Static);
        modelingButton.OnClick = new SerializableEvent(() => SwitchScene("Modeling"));

        UIButton riggingButton = new("RiggingButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null, UIState.Static);
        riggingButton.OnClick = new SerializableEvent(() => SwitchScene("Rigging"));

        UIButton animationButton = new("AnimationButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null, UIState.Static);
        animationButton.OnClick = new SerializableEvent(() => SwitchScene("Animation"));

        UIButton vertexSelectionButton = new("VertexSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 97, (0, 0), null, UIState.Static);
        vertexSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Vertex));

        UIButton edgeSelectionButton = new("EdgeSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 98, (0, 0), null, UIState.Static);
        edgeSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Edge));

        UIButton faceSelectionButton = new("FaceSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 99, (0, 0), null, UIState.Static);
        faceSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Face));

        SnappingText = new("SnappingText", AnchorType.MiddleCenter, PositionType.Relative, (0, 40, 0), (400, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        SnappingText.TextType = TextType.Decimal;
        SnappingText.MaxCharCount = 5;
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);
        SnappingText.SetText("0", 0.7f);


        UIImage vertexPanel = new("VertexPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        UICollectionDepthStacking vertexCollection = new("VertexCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        vertexCollection.AddElement(vertexPanel);
        vertexCollection.AddElement(vertexSelectionButton);

        UIImage edgePanel = new("EdgePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        UICollectionDepthStacking edgeCollection = new("EdgeCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        edgeCollection.AddElement(edgePanel);
        edgeCollection.AddElement(edgeSelectionButton);

        UIImage facePanel = new("FacePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        UICollectionDepthStacking faceCollection = new("FaceCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        faceCollection.AddElement(facePanel);
        faceCollection.AddElement(faceSelectionButton);

        UIImage snappingPanel = new("SnappingPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (100, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f), null);
        UICollectionDepthStacking snappingCollection = new("SnappingCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (100, 36), (0, 0, 0, 0), 0);

        snappingCollection.AddElement(snappingPanel);
        snappingCollection.AddElement(SnappingText);


        UICollectionHorizontalStacking fileStacking = new("FileStacking", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        FileName = new("ModelName", AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (200, 36), (0, 0, 0, 0), 0, 0, (10, 0.15f), null);
        FileName.SetText("cube", 0.7f);

        
        UIButton saveModelButton = new("saveModelButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 95, (0, 0), null, UIState.Static);
        saveModelButton.OnClick = new SerializableEvent(() => SaveModel());

        UIButton loadModelButton = new("loadModelButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 94, (0, 0), null, UIState.Static);
        loadModelButton.OnClick = new SerializableEvent(() => LoadModel());


        UIImage fileNamePanel = new("FileNamePanel", AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (310, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f), null);
        UICollectionDepthStacking fileNameCollection = new("FileNameCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (310, 36), (0, 0, 0, 0), 0);
        
        fileNameCollection.AddElement(fileNamePanel);
        fileNameCollection.AddElement(FileName);

        UIImage savePanel = new("SavePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        UICollectionDepthStacking saveCollection = new("SaveCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        saveCollection.AddElement(savePanel);
        saveCollection.AddElement(saveModelButton);

        UIImage loadPanel = new("LoadPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);
        UICollectionDepthStacking loadCollection = new("LoadCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        loadCollection.AddElement(loadPanel);
        loadCollection.AddElement(loadModelButton);


        stateCollection.AddElement(statePanel);

        stateStacking.AddElement(modelingButton);
        stateStacking.AddElement(riggingButton);
        stateStacking.AddElement(animationButton);
        stateStacking.AddElement(vertexCollection);
        stateStacking.AddElement(edgeCollection);
        stateStacking.AddElement(faceCollection);
        stateStacking.AddElement(snappingCollection);

        fileStacking.AddElement(fileNameCollection);
        fileStacking.AddElement(saveCollection);
        fileStacking.AddElement(loadCollection);

        stateCollection.AddElement(stateStacking);
        stateCollection.AddElement(fileStacking);



        UICollection mainPanelCollection = new("MainPanelCollection", AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 60, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f), null);

        BackfaceCullingText = new("CullingText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, 35, 10, 10), 0, 0, (10, 0.05f), null);
        BackfaceCullingText.SetText("cull: " + BackfaceCulling, 0.7f);

        MeshAlphaText = new("AlphaText", AnchorType.TopLeft, PositionType.Relative, (0, 20, 0), (400, 20), (10, 60, 10, 10), 0, 0, (10, 0.05f), null);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"), 0.7f);

        MirrorText = new UIText("MirrorText", AnchorType.TopLeft, PositionType.Relative, (0, 60, 0), (400, 20), (10, 110, 10, 10), 0, 0, (10, 0.05f), null);
        MirrorText.SetText("mirror: " + (Mirror.X == 1 ? "X" : "-") + (Mirror.Y == 1 ? "Y" : "-") + (Mirror.Z == 1 ? "Z" : "-"), 0.7f);

        mainPanelCollection.AddElement(mainPanel);
        mainPanelCollection.AddElement(BackfaceCullingText);
        mainPanelCollection.AddElement(MeshAlphaText);
        mainPanelCollection.AddElement(MirrorText);


        UIButton cullingButton = new("CullingButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 35, 10, 10), 0, 0, (10, 0.05f), null, UIState.Static);
        cullingButton.OnClick = new SerializableEvent(BackFaceCullingSwitch);

        UIButton alphaButton = new("AlphaUpButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 60, 10, 10), 0, 0, (10, 0.05f), null, UIState.Static);
        alphaButton.OnHold = new SerializableEvent(AlphaControl);

        UIButton mirrorButton = new("MirrorButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 110, 10, 10), 0, 0, (10, 0.05f), null, UIState.Static);
        mirrorButton.OnClick = new SerializableEvent(ApplyMirror);

        UIButton mirrorZButton = new("MirrorZButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-85, 110, 10, 10), 0, 0, (10, 0.05f), null, UIState.InvisibleInteractable);
        mirrorZButton.OnClick = new SerializableEvent(() => SwitchMirror("Z"));

        UIButton mirrorYButton = new("MirrorYButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-100, 110, 10, 10), 0, 0, (10, 0.05f), null, UIState.InvisibleInteractable);
        mirrorYButton.OnClick = new SerializableEvent(() => SwitchMirror("Y"));

        UIButton mirrorXButton = new("MirrorXButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-115, 110, 10, 10), 0, 0, (10, 0.05f), null, UIState.InvisibleInteractable);
        mirrorXButton.OnClick = new SerializableEvent(() => SwitchMirror("X"));


        mainPanelCollection.AddElement(cullingButton);
        mainPanelCollection.AddElement(alphaButton);
        mainPanelCollection.AddElement(mirrorButton);
        mainPanelCollection.AddElement(mirrorXButton);
        mainPanelCollection.AddElement(mirrorYButton);
        mainPanelCollection.AddElement(mirrorZButton);
 
        MainUi.AddElement(stateCollection);
        ModelingUi.AddElement(mainPanelCollection);


        Bone rootBone = new Bone("bone1");
        rootBone.Pivot = (1, 0, -1);
        rootBone.End = (1, 2, -1);
        model.Init(rootBone, FileName.Text);
        
        MainUi.GenerateBuffers();
        ModelingUi.GenerateBuffers();

        Console.WriteLine("Vertices: " + ModelingUi.uIMesh.Vertices.Count);

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

        //MainUi.ToLines();
    }

    public override void Resize()
    {
        Console.WriteLine("Animation Editor Resize");
        MainUi.OnResize();
        ModelingUi.OnResize();
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
        MainUi.Test();
        CurrentEditor.Update(this);
        model.Update();
        base.Update();
    }

    public override void Render()
    {
        CurrentEditor.Render(this);
        ModelingUi.Render();
        MainUi.Render();
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

    public void LoadModel()
    {
        string fileName = FileName.Text.Trim();
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return;
        }

        if (File.Exists(Path.Combine(Game.modelPath, $"{fileName}.model")) && currentModelName != fileName && currentModelName != "")
            PopUp.AddConfirmation("Save current model?", () => SaveAndLoad(fileName), () => Load(fileName));
        else
            Load(fileName);
    }

    public void SaveModel()
    {
        string fileName = FileName.Text.Trim();
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return;
        }

        if (File.Exists(Path.Combine(Game.modelPath, $"{fileName}.model")))
            PopUp.AddConfirmation("Overwrite existing model?", () => model.modelMesh.SaveModel(fileName), null);
        else
            model.modelMesh.SaveModel(fileName);
    }

    public void SaveAndLoad(string fileName)
    {
        model.modelMesh.SaveModel(currentModelName);
        Load(fileName);
    }

    public void Load(string fileName)
    {
        if (model.modelMesh.LoadModel(fileName))
            currentModelName = fileName;
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
        model.modelMesh.CombineDuplicateVertices();
        
        model.modelMesh.Init();
        model.modelMesh.GenerateBuffers();
        model.modelMesh.UpdateMesh();
        
        regenerateVertexUi = true;
        
        Mirror = new Vector3i(0, 0, 0);
        UpdateMirrorText();
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
    
    public UIPanel GeneratePanelLink(Vector2 pos1, Vector2 pos2, int textureIndex)
    {
        float distance = Vector2.Distance(pos1, pos2);
        UIImage panel = new($"Link{pos1}{pos2}", AnchorType.TopLeft, PositionType.Free, (1, 1, 1), (0, 0, 0), (20, distance), (0, 0, 0, 0), 0, textureIndex, (10, 0.15f), null);
        panel.SetPosition(new Vector3(pos1.X, pos1.Y, 0));
        return panel;
    }
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