using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class GeneralModelingEditor : ScriptingNode
{
    public BaseEditor CurrentEditor;
    public ModelingEditor modelingEditor = new ModelingEditor();
    public RiggingEditor riggingEditor = new RiggingEditor();
    public AnimationEditor animationEditor = new AnimationEditor();
    public TextureEditor textureEditor = new TextureEditor();

    public UIController MainUi = new UIController();
    public UIController ModelingUi = new UIController();
    public UIController UIScrollViewTest = new UIController();
    public OldModel model = new OldModel();

    public string currentModelName = "cube";
    public List<string> MeshSaveNames = new List<string>();

    // Ui Elements
    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIInputField SnappingText;
    public static UIText MirrorText;
    public static UIText AxisText;
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

    public int AxisX
    {
        get => ModelSettings.axis.X;
        set => ModelSettings.axis.X = value;
    }

    public int AxisY
    {
        get => ModelSettings.axis.Y;
        set => ModelSettings.axis.Y = value;
    }

    public int AxisZ
    {
        get => ModelSettings.axis.Z;
        set => ModelSettings.axis.Z = value;
    }

    
    public bool freeCamera = false;
    public int _selectedModel = 0;
    public bool regenerateVertexUi = false;
    public Bone? _selectedBone = null;

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
        
        Game.camera = new Camera(Game.Width, Game.Height, new Vector3(0, 0, 5));
        ModelSettings.Camera = Game.camera;
        ModelSettings.Model = model;
        
        Transform.Position = new Vector3(0, 0, 0);

        // Top panel
        UIMesh uiMesh = MainUi.uIMesh;
        TextMesh textMesh = MainUi.textMesh;


        UICollection stateCollection = new("StateCollection", AnchorType.ScaleTop, PositionType.Absolute, (0, 0, 0), (Game.Width, 50), (5, 5, 255, 5), 0);

        UIImage statePanel = new("StatePanel", AnchorType.ScaleTop, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (0, 0, 0), (0, 50), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);


        UIHorizontalCollection stateStacking = new("StateStacking", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        UIButton modelingButton = new("ModelingButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh, UIState.Static);
        modelingButton.OnClick = new SerializableEvent(() => SwitchScene("Modeling"));

        UIButton riggingButton = new("RiggingButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh, UIState.Static);
        riggingButton.OnClick = new SerializableEvent(() => SwitchScene("Rigging"));

        UIButton animationButton = new("AnimationButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh, UIState.Static);
        animationButton.OnClick = new SerializableEvent(() => SwitchScene("Animation"));

        UIButton textureButton = new("TextureButton", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh, UIState.Static);
        textureButton.OnClick = new SerializableEvent(() => SwitchScene("Texture"));

        UIButton vertexSelectionButton = new("VertexSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 97, (0, 0), uiMesh, UIState.Static);
        vertexSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Vertex));

        UIButton edgeSelectionButton = new("EdgeSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 98, (0, 0), uiMesh, UIState.Static);
        edgeSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Edge));

        UIButton faceSelectionButton = new("FaceSelectionButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 99, (0, 0), uiMesh, UIState.Static);
        faceSelectionButton.OnClick = new SerializableEvent(() => ModelingEditor.SwitchSelection(RenderType.Face));


        UIImage vertexPanel = new("VertexPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);
        UIDepthCollection vertexCollection = new("VertexCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        vertexCollection.AddElement(vertexPanel);
        vertexCollection.AddElement(vertexSelectionButton);

        UIImage edgePanel = new("EdgePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);
        UIDepthCollection edgeCollection = new("EdgeCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        edgeCollection.AddElement(edgePanel);
        edgeCollection.AddElement(edgeSelectionButton);

        UIImage facePanel = new("FacePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);
        UIDepthCollection faceCollection = new("FaceCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        faceCollection.AddElement(facePanel);
        faceCollection.AddElement(faceSelectionButton);

        UIImage snappingPanel = new("SnappingPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (100, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f), uiMesh);
        UIDepthCollection snappingCollection = new("SnappingCollection", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (100, 36), (0, 0, 0, 0), 0);

        SnappingText = new("SnappingText", AnchorType.MiddleCenter, PositionType.Relative, (0, 40, 0), (400, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), textMesh);
        SnappingText.SetTextType(TextType.Decimal).SetMaxCharCount(5).SetText("0", 0.7f);
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);

        snappingCollection.AddElement(snappingPanel);
        snappingCollection.AddElement(SnappingText);



        UIHorizontalCollection fileStacking = new("FileStacking", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        FileName = new("ModelName", AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (200, 36), (0, 0, 0, 0), 0, 0, (10, 0.15f), textMesh);
        FileName.SetText("cube", 0.7f);

        
        UIButton saveModelButton = new("saveModelButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 95, (0, 0), uiMesh, UIState.Static);
        saveModelButton.OnClick = new SerializableEvent(() => SaveModel());

        UIButton loadModelButton = new("loadModelButton", AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 94, (0, 0), uiMesh, UIState.Static);
        loadModelButton.OnClick = new SerializableEvent(() => LoadModel());


        UIImage fileNamePanel = new("FileNamePanel", AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (310, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f), uiMesh);
        UIDepthCollection fileNameCollection = new("FileNameCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (310, 36), (0, 0, 0, 0), 0);
        
        fileNameCollection.AddElement(fileNamePanel);
        fileNameCollection.AddElement(FileName);

        UIImage savePanel = new("SavePanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);
        UIDepthCollection saveCollection = new("SaveCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        saveCollection.AddElement(savePanel);
        saveCollection.AddElement(saveModelButton);

        UIImage loadPanel = new("LoadPanel", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);
        UIDepthCollection loadCollection = new("LoadCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        loadCollection.AddElement(loadPanel);
        loadCollection.AddElement(loadModelButton);


        stateCollection.AddElement(statePanel);

        stateStacking.AddElement(modelingButton);
        stateStacking.AddElement(riggingButton);
        stateStacking.AddElement(animationButton);
        stateStacking.AddElement(textureButton);
        stateStacking.AddElement(vertexCollection);
        stateStacking.AddElement(edgeCollection);
        stateStacking.AddElement(faceCollection);
        stateStacking.AddElement(snappingCollection);

        fileStacking.AddElement(fileNameCollection);
        fileStacking.AddElement(saveCollection);
        fileStacking.AddElement(loadCollection);

        stateCollection.AddElement(stateStacking);
        stateCollection.AddElement(fileStacking);



        // Modeling panel
        UIMesh modelingUiMesh = ModelingUi.uIMesh;
        TextMesh modelingTextMesh = ModelingUi.textMesh;


        UICollection mainPanelCollection = new("MainPanelCollection", AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f), modelingUiMesh);

        BackfaceCullingText = new("CullingText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, 35, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        BackfaceCullingText.SetText("cull: " + BackfaceCulling, 0.7f);

        MeshAlphaText = new("AlphaText", AnchorType.TopLeft, PositionType.Relative, (0, 20, 0), (400, 20), (10, 60, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"), 0.7f);

        UIText WireframeVisibilityText = new("WireframeVisibilityText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, 85, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        WireframeVisibilityText.MaxCharCount = 12; 
        WireframeVisibilityText.SetText("frame: " + ModelSettings.WireframeVisible, 0.7f);

        MirrorText = new UIText("MirrorText", AnchorType.TopLeft, PositionType.Relative, (0, 60, 0), (400, 20), (10, 110, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        MirrorText.SetText("mirror: " + (Mirror.X == 1 ? "X" : "-") + (Mirror.Y == 1 ? "Y" : "-") + (Mirror.Z == 1 ? "Z" : "-"), 0.7f);

        AxisText = new UIText("AxisText", AnchorType.TopLeft, PositionType.Relative, (0, 80, 0), (400, 20), (10, 135, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        AxisText.SetText("axis: " + (AxisX == 1 ? "X" : "-") + (AxisY == 1 ? "Y" : "-") + (AxisZ == 1 ? "Z" : "-"), 0.7f);

        UIText GridAlignedText = new("GridAlignedText", AnchorType.TopLeft, PositionType.Relative, (0, 100, 0), (400, 20), (10, 160, 10, 10), 0, 0, (10, 0.05f), modelingTextMesh);
        GridAlignedText.MaxCharCount = 11;
        GridAlignedText.SetText("grid: " + ModelSettings.GridAligned, 0.7f);

        UICollection cameraSpeedStacking = new("CameraSpeedStacking", AnchorType.BottomLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, -10, 10, 10), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", AnchorType.BottomLeft, PositionType.Relative, (0, 0, 0), (400, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), modelingTextMesh);
        CameraSpeedTextLabel.SetText("Cam Speed: ", 0.7f);
        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", AnchorType.BottomLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (45, 30), (142, 8, 0, 0), 0, 1, (10, 0.05f), modelingUiMesh);
        UIInputField CameraSpeedField = new("CameraSpeedText", AnchorType.BottomLeft, PositionType.Relative, (0, 100, 0), (400, 20), (150, 0, 0, 0), 0, 0, (10, 0.05f), modelingTextMesh);
        CameraSpeedField.MaxCharCount = 2;
        CameraSpeedField.SetText("50", 0.7f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; } });

        cameraSpeedStacking.AddElement(CameraSpeedTextLabel, CameraSpeedFieldPanel, CameraSpeedField);


        UIButton cullingButton = new("CullingButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 35, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.Static);
        cullingButton.OnClick = new SerializableEvent(BackFaceCullingSwitch);

        UIButton alphaButton = new("AlphaUpButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 60, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.Static);
        alphaButton.OnHold = new SerializableEvent(AlphaControl);

        UIButton WireframeVisibilitySwitch = new("WireframeVisibilitySwitch", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20),  (-5, 85, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.Static);
        WireframeVisibilitySwitch.OnClick = new SerializableEvent(() => {
            ModelSettings.WireframeVisible = !ModelSettings.WireframeVisible; 
            WireframeVisibilityText.SetText("frame: " + ModelSettings.WireframeVisible).GenerateChars().UpdateText();
        });

        UIButton mirrorButton = new("MirrorButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 110, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.Static);
        mirrorButton.OnClick = new SerializableEvent(ApplyMirror);

        UIButton mirrorZButton = new("MirrorZButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-85, 110, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        mirrorZButton.OnClick = new SerializableEvent(() => SwitchMirror("Z"));

        UIButton mirrorYButton = new("MirrorYButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-100, 110, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        mirrorYButton.OnClick = new SerializableEvent(() => SwitchMirror("Y"));

        UIButton mirrorXButton = new("MirrorXButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-115, 110, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        mirrorXButton.OnClick = new SerializableEvent(() => SwitchMirror("X"));

        UIButton axisZButton = new("AxisZButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-113, 135, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        axisZButton.OnClick = new SerializableEvent(() => SwitchAxis("Z"));

        UIButton axisYButton = new("AxisYButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-128, 135, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        axisYButton.OnClick = new SerializableEvent(() => SwitchAxis("Y"));

        UIButton axisXButton = new("AxisXButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (15, 20), (-143, 135, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.InvisibleInteractable);
        axisXButton.OnClick = new SerializableEvent(() => SwitchAxis("X"));

        UIButton gridAlignedButton = new("GridAlignedButton", AnchorType.TopRight, PositionType.Relative, (1, 1, 1), (0, 0, 0), (40, 20), (-5, 160, 10, 10), 0, 0, (10, 0.05f), modelingUiMesh, UIState.Static);
        gridAlignedButton.OnClick = new SerializableEvent(() => {
            ModelSettings.GridAligned = !ModelSettings.GridAligned; 
            GridAlignedText.SetText("grid: " + ModelSettings.GridAligned).GenerateChars().UpdateText();
        });


        mainPanelCollection.AddElement(
            mainPanel, BackfaceCullingText, MeshAlphaText, WireframeVisibilityText, MirrorText, AxisText, GridAlignedText, cameraSpeedStacking,
            cullingButton, alphaButton, WireframeVisibilitySwitch, mirrorButton, mirrorXButton, mirrorYButton, mirrorZButton, axisXButton, axisYButton, axisZButton, gridAlignedButton
        );


        // Scroll view test
        UIMesh scrollViewMaskMesh = UIScrollViewTest.maskMesh;
        UIMesh scrollViewUiMesh = UIScrollViewTest.uIMesh;
        UIMesh scrollViewMaskedUiMesh = UIScrollViewTest.maskeduIMesh;
        TextMesh scrollViewTextMesh = UIScrollViewTest.textMesh;
        TextMesh scrollViewMaskedTextMesh = UIScrollViewTest.maskedTextMesh;

        // Add elements to ui
        MainUi.AddElement(stateCollection);
        ModelingUi.AddElement(mainPanelCollection);

        model.Init();
        
        MainUi.GenerateBuffers();
        ModelingUi.GenerateBuffers();
        UIScrollViewTest.GenerateBuffers();



        Camera camera = Game.camera;

        camera.SetCameraMode(CameraMode.Free);
        
        camera.position = new Vector3(0, 0, 5);
        camera.pitch = 0;
        camera.yaw = -90;
        
        camera.UpdateVectors();
        
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);

        CurrentEditor = modelingEditor;
        modelingEditor.Start(this);
        riggingEditor.Start(this);
        animationEditor.Start(this);

        //MainUi.ToLines();
    }

    public override void Resize()
    {
        MainUi.OnResize();
        ModelingUi.OnResize();
        UIScrollViewTest.OnResize();
        modelingEditor.Resize(this);
        riggingEditor.Resize(this);
        animationEditor.Resize(this);
        textureEditor.Resize(this);
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
        UIScrollViewTest.Test();
        CurrentEditor.Update(this);
        model.Update();
        base.Update();
    }

    public override void Render()
    {
        Shader.Error("general render: ");

        CurrentEditor.Render(this);
        ModelingUi.Render();
        MainUi.Render();
        UIScrollViewTest.Render();
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
        if (!CurrentEditor.Started)
            CurrentEditor.Start(this);
        CurrentEditor.Awake(this);
    }

    public void LoadModel()
    {
        string fileName = FileName.Text.Trim();
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return;
        }

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

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

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string path = Path.Combine(Game.modelPath, $"{fileName}.model");
        if (File.Exists(path))
            PopUp.AddConfirmation("Overwrite existing model?", () => model.CurrentMesh.SaveModel(fileName), null);
        else
            model.CurrentMesh.SaveModel(fileName);  
    }

    public void SaveAndLoad(string fileName)
    {
        model.CurrentMesh.SaveModel(currentModelName);
        Load(fileName);
    }

    public void Load(string fileName)
    {
        if (model.CurrentMesh.LoadModel(fileName))
        {
            currentModelName = fileName;
            MeshSaveNames.Clear();
        }
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
            case "Texture":
                DoSwitchScene(textureEditor);
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

    public void SwitchAxis(string axis)
    {
        switch (axis)
        {
            case "X":
                AxisX = AxisX == 0 ? 1 : 0;
                break;
            case "Y":
                AxisY = AxisY == 0 ? 1 : 0;
                break;
            case "Z":
                AxisZ = AxisZ == 0 ? 1 : 0;
                break;
        }
        
        UpdateAxisText();
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
        string text = SnappingText.Text.EndsWith('.') ? SnappingText.Text[..^1] : SnappingText.Text;
        SnappingFactor = Mathf.Clamp(0, 100, Float.Parse(text));
        SnappingText.SetText(SnappingFactor.ToString() + (SnappingText.Text.EndsWith('.') ? "." : "")).GenerateChars().UpdateText();
        Snapping = SnappingFactor != 0.0f;
    }
    
    public void SetBonePivot(string axis)
    {
        if (_selectedBone == null)
            return;
        
        switch (axis)
        {
            case "X":
                _selectedBone.Pivot.Position.X = float.Parse(BonePivotX.Text);
                break;
            case "Y":
                _selectedBone.Pivot.Position.Y = float.Parse(BonePivotY.Text);
                break;
            case "Z":
                _selectedBone.Pivot.Position.Z = float.Parse(BonePivotZ.Text);
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
                _selectedBone.End.Position.X = float.Parse(BoneEndX.Text);
                break;
            case "Y":
                _selectedBone.End.Position.Y = float.Parse(BoneEndY.Text);
                break;
            case "Z":
                _selectedBone.End.Position.Z = float.Parse(BoneEndZ.Text);
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

    public void UpdateAxisText()
    {
        string text = "";
        text += AxisX == 1 ? "X" : "-";
        text += AxisY == 1 ? "Y" : "-";
        text += AxisZ == 1 ? "Z" : "-";
        
        AxisText.SetText("axis: " + text).GenerateChars().UpdateText();
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
            Vector2? screenPosA = Mathf.WorldToScreen(bone.Pivot.Position, projection, view);
            Vector2? screenPosB = Mathf.WorldToScreen(bone.End.Position, projection, view);
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
    public bool Started = false;

    public abstract void Start(GeneralModelingEditor editor);
    public abstract void Resize(GeneralModelingEditor editor);
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