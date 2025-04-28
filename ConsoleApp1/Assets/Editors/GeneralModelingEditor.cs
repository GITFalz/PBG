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

    public UIController MainUi;
    public UIController ModelingUi;
    public UIController UIScrollViewTest;
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

    public GeneralModelingEditor()
    {
        MainUi = new UIController();
        ModelingUi = new UIController();
        UIScrollViewTest = new UIController();
    }

    public override void Start()
    {
        Console.WriteLine("Animation Editor Start");
        
        Game.camera = new Camera(Game.Width, Game.Height, new Vector3(0, 0, 5));
        ModelSettings.Camera = Game.camera;
        ModelSettings.Model = model;
        
        Transform.Position = new Vector3(0, 0, 0);

        UICollection stateCollection = new("StateCollection", MainUi, AnchorType.ScaleTop, PositionType.Absolute, (0, 0, 0), (Game.Width, 50), (5, 5, 255, 5), 0);

        UIImage statePanel = new("StatePanel", MainUi, AnchorType.ScaleTop, PositionType.Absolute, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (0, 50), (0, 0, 0, 0), 0, 0, (10, 0.05f));


        UIHorizontalCollection stateStacking = new("StateStacking", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        UIButton modelingButton = new("ModelingButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        modelingButton.SetOnClick(() => SwitchScene("Modeling"));

        UIButton riggingButton = new("RiggingButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        riggingButton.SetOnClick(() => SwitchScene("Rigging"));

        UIButton animationButton = new("AnimationButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        animationButton.SetOnClick(() => SwitchScene("Animation"));

        UIButton textureButton = new("TextureButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        textureButton.SetOnClick(() => SwitchScene("Texture"));

        UIButton vertexSelectionButton = new("VertexSelectionButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 97, (0, 0), UIState.Static);
        vertexSelectionButton.SetOnClick(() => ModelingEditor.SwitchSelection(RenderType.Vertex));

        UIButton edgeSelectionButton = new("EdgeSelectionButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 98, (0, 0), UIState.Static);
        edgeSelectionButton.SetOnClick(() => ModelingEditor.SwitchSelection(RenderType.Edge));

        UIButton faceSelectionButton = new("FaceSelectionButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 99, (0, 0), UIState.Static);
        faceSelectionButton.SetOnClick(() => ModelingEditor.SwitchSelection(RenderType.Face));


        UIImage vertexPanel = new("VertexPanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection vertexCollection = new("VertexCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        vertexCollection.AddElement(vertexPanel);
        vertexCollection.AddElement(vertexSelectionButton);

        UIImage edgePanel = new("EdgePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection edgeCollection = new("EdgeCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        edgeCollection.AddElement(edgePanel);
        edgeCollection.AddElement(edgeSelectionButton);

        UIImage facePanel = new("FacePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection faceCollection = new("FaceCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        faceCollection.AddElement(facePanel);
        faceCollection.AddElement(faceSelectionButton);

        UIImage snappingPanel = new("SnappingPanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (100, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIDepthCollection snappingCollection = new("SnappingCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (100, 36), (0, 0, 0, 0), 0);

        SnappingText = new("SnappingText", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 40, 0), (400, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        SnappingText.SetTextType(TextType.Decimal).SetMaxCharCount(5).SetText("0", 0.7f);
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);

        snappingCollection.AddElement(snappingPanel);
        snappingCollection.AddElement(SnappingText);



        UIHorizontalCollection fileStacking = new("FileStacking", MainUi, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        FileName = new("ModelName", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (200, 36), (0, 0, 0, 0), 0, 0, (10, 0.15f));
        FileName.SetText("cube", 0.7f);

        
        UIButton saveModelButton = new("saveModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 95, (0, 0), UIState.Static);
        saveModelButton.SetOnClick(() => SaveModel());

        UIButton loadModelButton = new("loadModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 94, (0, 0), UIState.Static);
        loadModelButton.SetOnClick(() => LoadModel());


        UIImage fileNamePanel = new("FileNamePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (310, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIDepthCollection fileNameCollection = new("FileNameCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (310, 36), (0, 0, 0, 0), 0);
        
        fileNameCollection.AddElement(fileNamePanel);
        fileNameCollection.AddElement(FileName);

        UIImage savePanel = new("SavePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection saveCollection = new("SaveCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        saveCollection.AddElement(savePanel);
        saveCollection.AddElement(saveModelButton);

        UIImage loadPanel = new("LoadPanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection loadCollection = new("LoadCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

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



        UICollection mainPanelCollection = new("MainPanelCollection", ModelingUi, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f));

        BackfaceCullingText = new("CullingText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 35, 10, 10), 0);
        BackfaceCullingText.SetText("cull: " + BackfaceCulling, 0.7f);

        MeshAlphaText = new("AlphaText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (10, 60, 10, 10), 0);
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"), 0.7f);

        UIText WireframeVisibilityText = new("WireframeVisibilityText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 85, 10, 10), 0);
        WireframeVisibilityText.MaxCharCount = 12; 
        WireframeVisibilityText.SetText("frame: " + ModelSettings.WireframeVisible, 0.7f);

        MirrorText = new UIText("MirrorText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 60, 0), (400, 20), (10, 110, 10, 10), 0);
        MirrorText.SetText("mirror: " + (Mirror.X == 1 ? "X" : "-") + (Mirror.Y == 1 ? "Y" : "-") + (Mirror.Z == 1 ? "Z" : "-"), 0.7f);

        AxisText = new UIText("AxisText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 80, 0), (400, 20), (10, 135, 10, 10), 0);
        AxisText.SetText("axis: " + (AxisX == 1 ? "X" : "-") + (AxisY == 1 ? "Y" : "-") + (AxisZ == 1 ? "Z" : "-"), 0.7f);

        UIText GridAlignedText = new("GridAlignedText", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 100, 0), (400, 20), (10, 160, 10, 10), 0);
        GridAlignedText.MaxCharCount = 11;
        GridAlignedText.SetText("grid: " + ModelSettings.GridAligned, 0.7f);

        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomLeft, PositionType.Relative, (0, 0, 0), (400, 20), (10, -10, 10, 10), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.BottomLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetText("Cam Speed: ", 0.7f);
        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.BottomLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (142, 8, 0, 0), 0, 1, (10, 0.05f));
        UIInputField CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.BottomLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 100, 0), (400, 20), (150, 0, 0, 0), 0, 0, (10, 0.05f));
        CameraSpeedField.MaxCharCount = 2;
        CameraSpeedField.SetText("50", 0.7f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; } });

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, CameraSpeedFieldPanel, CameraSpeedField);


        UIButton cullingButton = new("CullingButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (-5, 35, 10, 10), 0, 0, (10, 0.05f), UIState.Static);
        cullingButton.SetOnClick(BackFaceCullingSwitch);

        UIButton alphaButton = new("AlphaUpButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (-5, 60, 10, 10), 0, 0, (10, 0.05f), UIState.Static);
        alphaButton.SetOnHold(AlphaControl);

        UIButton WireframeVisibilitySwitch = new("WireframeVisibilitySwitch", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20),  (-5, 85, 10, 10), 0, 0, (10, 0.05f), UIState.Static);
        WireframeVisibilitySwitch.SetOnClick(() => {
            ModelSettings.WireframeVisible = !ModelSettings.WireframeVisible; 
            WireframeVisibilityText.SetText("frame: " + ModelSettings.WireframeVisible).UpdateCharacters();
        });

        UIButton mirrorButton = new("MirrorButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (-5, 110, 10, 10), 0, 0, (10, 0.05f), UIState.Static);
        mirrorButton.SetOnClick(ApplyMirror);

        UIButton mirrorZButton = new("MirrorZButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-85, 110, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        mirrorZButton.SetOnClick(() => SwitchMirror("Z"));

        UIButton mirrorYButton = new("MirrorYButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-100, 110, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        mirrorYButton.SetOnClick(() => SwitchMirror("Y"));

        UIButton mirrorXButton = new("MirrorXButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-115, 110, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        mirrorXButton.SetOnClick(() => SwitchMirror("X"));

        UIButton axisZButton = new("AxisZButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-113, 135, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        axisZButton.SetOnClick(() => SwitchAxis("Z"));

        UIButton axisYButton = new("AxisYButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-128, 135, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        axisYButton.SetOnClick(() => SwitchAxis("Y"));

        UIButton axisXButton = new("AxisXButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (15, 20), (-143, 135, 10, 10), 0, 0, (10, 0.05f), UIState.InvisibleInteractable);
        axisXButton.SetOnClick(() => SwitchAxis("X"));

        UIButton gridAlignedButton = new("GridAlignedButton", ModelingUi, AnchorType.TopRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (-5, 160, 10, 10), 0, 0, (10, 0.05f), UIState.Static);
        gridAlignedButton.SetOnClick(() => {
            ModelSettings.GridAligned = !ModelSettings.GridAligned; 
            GridAlignedText.SetText("grid: " + ModelSettings.GridAligned).UpdateCharacters();
        });


        mainPanelCollection.AddElements(
            mainPanel, BackfaceCullingText, MeshAlphaText, WireframeVisibilityText, MirrorText, AxisText, GridAlignedText, cameraSpeedStacking,
            cullingButton, alphaButton, WireframeVisibilitySwitch, mirrorButton, mirrorXButton, mirrorYButton, mirrorZButton, axisXButton, axisYButton, axisZButton, gridAlignedButton
        );


        // Add elements to ui
        MainUi.AddElement(stateCollection);
        ModelingUi.AddElement(mainPanelCollection);

        model.Init();


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
        MainUi.Resize();
        ModelingUi.Resize();
        UIScrollViewTest.Resize();
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
        ModelingUi.Update();
        MainUi.Update();
        UIScrollViewTest.Update();
        CurrentEditor.Update(this);
        model.Update();
        base.Update();
    }

    public override void Render()
    {     
        CurrentEditor.Render(this);
        ModelingUi.RenderNoDepthTest();
        MainUi.RenderNoDepthTest();
        UIScrollViewTest.RenderNoDepthTest();
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
        MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2")).UpdateCharacters();
    }

    public void BackFaceCullingSwitch()
    {
        BackfaceCulling = !BackfaceCulling;
        BackfaceCullingText.SetText("cull: " + BackfaceCulling).UpdateCharacters();
    }

    public void SnappingField()
    {
        string text = SnappingText.Text.EndsWith('.') ? SnappingText.Text[..^1] : SnappingText.Text;
        SnappingFactor = Mathf.Clamp(0, 100, Float.Parse(text));
        SnappingText.SetText(SnappingFactor.ToString() + (SnappingText.Text.EndsWith('.') ? "." : "")).UpdateCharacters();
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
        
        MirrorText.SetText("mirror: " + text).UpdateCharacters();
    }

    public void UpdateAxisText()
    {
        string text = "";
        text += AxisX == 1 ? "X" : "-";
        text += AxisY == 1 ? "Y" : "-";
        text += AxisZ == 1 ? "Z" : "-";
        
        AxisText.SetText("axis: " + text).UpdateCharacters();
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