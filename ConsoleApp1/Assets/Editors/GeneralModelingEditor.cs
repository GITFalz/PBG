using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class GeneralModelingEditor : ScriptingNode
{
    public BaseEditor CurrentEditor;
    public ModelingEditor modelingEditor;
    public RiggingEditor riggingEditor;
    public AnimationEditor animationEditor;
    public TextureEditor textureEditor;

    public UIController MainUi;
    public UIController UIScrollViewTest;
    public OldModel model = new OldModel();

    public string currentModelName = "cube";
    public List<string> MeshSaveNames = new List<string>();

    // Main UI
    public static UIInputField FileName;
    public static UIInputField SnappingText;


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
        get => ModelSettings.Mirror;
        set => ModelSettings.Mirror = value;
    }

    public int MirrorX
    {
        get => ModelSettings.Mirror.X;
        set => ModelSettings.Mirror.X = value;
    }

    public int MirrorY
    {
        get => ModelSettings.Mirror.Y;
        set => ModelSettings.Mirror.Y = value;
    }

    public int MirrorZ
    {
        get => ModelSettings.Mirror.Z;
        set => ModelSettings.Mirror.Z = value;
    }

    public int AxisX
    {
        get => ModelSettings.Axis.X;
        set => ModelSettings.Axis.X = value;
    }

    public int AxisY
    {
        get => ModelSettings.Axis.Y;
        set => ModelSettings.Axis.Y = value;
    }

    public int AxisZ
    {
        get => ModelSettings.Axis.Z;
        set => ModelSettings.Axis.Z = value;
    }
    

    public Action LoadAction = () => {};
    public Action SaveAction = () => {};

    
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
        UIScrollViewTest = new UIController();

        modelingEditor = new ModelingEditor(this); 
        riggingEditor = new RiggingEditor(this); 
        animationEditor = new AnimationEditor(this);
        textureEditor = new TextureEditor(this);
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

        UITextButton modelingButton = new("ModelingButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        modelingButton.SetOnClick(() => SwitchScene("Modeling"));
        modelingButton.SetTextCharCount("Modeling", 1.2f);
        modelingButton.Collection.SetScale((modelingButton.Text.newScale.X + 16, 36f));

        UITextButton riggingButton = new("RiggingButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        riggingButton.SetOnClick(() => SwitchScene("Rigging"));
        riggingButton.SetTextCharCount("Rigging", 1.2f);
        riggingButton.Collection.SetScale((riggingButton.Text.newScale.X + 16, 36f));

        UITextButton animationButton = new("AnimationButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        animationButton.SetOnClick(() => SwitchScene("Animation"));
        animationButton.SetTextCharCount("Animation", 1.2f);
        animationButton.Collection.SetScale((animationButton.Text.newScale.X + 16, 36f));

        UITextButton textureButton = new("TextureButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (75, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        textureButton.SetOnClick(() => SwitchScene("Texture"));
        textureButton.SetTextCharCount("Texture", 1.2f);
        textureButton.Collection.SetScale((textureButton.Text.newScale.X + 16, 36f));

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
        SnappingText.SetTextType(TextType.Decimal).SetMaxCharCount(5).SetText("0", 1.2f);
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);

        snappingCollection.AddElement(snappingPanel);
        snappingCollection.AddElement(SnappingText);



        UIHorizontalCollection fileStacking = new("FileStacking", MainUi, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        FileName = new("ModelName", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (200, 36), (0, 0, 0, 0), 0, 0, (10, 0.15f));
        FileName.SetMaxCharCount(24).SetText("cube", 1.2f);

        
        UIButton saveModelButton = new("saveModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 95, (0, 0), UIState.Static);
        saveModelButton.SetOnClick(Save);

        UIButton loadModelButton = new("loadModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 94, (0, 0), UIState.Static);
        loadModelButton.SetOnClick(Load);


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

        stateStacking.AddElement(modelingButton.Collection);
        stateStacking.AddElement(riggingButton.Collection);
        stateStacking.AddElement(animationButton.Collection);
        stateStacking.AddElement(textureButton.Collection);
        stateStacking.AddElement(vertexCollection);
        stateStacking.AddElement(edgeCollection);
        stateStacking.AddElement(faceCollection);
        stateStacking.AddElement(snappingCollection);

        fileStacking.AddElement(fileNameCollection);
        fileStacking.AddElement(saveCollection);
        fileStacking.AddElement(loadCollection);

        stateCollection.AddElement(stateStacking);
        stateCollection.AddElement(fileStacking);

        MainUi.AddElement(stateCollection);

        model.Init();


        Camera camera = Game.camera;

        camera.SetCameraMode(CameraMode.Free);
        
        camera.Position = new Vector3(0, 0, 5);
        camera.Pitch = 0;
        camera.Yaw = -90;
        
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
        MainUi.Update();
        UIScrollViewTest.Update();

        CurrentEditor.Update(this);
        model.Update();
        base.Update();
    }

    public override void Render()
    {     
        CurrentEditor.Render(this);

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

    public void Load()
    {
        LoadAction?.Invoke();
    }

    public void Save()
    {
        SaveAction?.Invoke();
    }

    public void LoadModel()
    {
        string fileName = FileName.Text.Trim();
        ModelManager.LoadModel(fileName);
    }

    public void SaveModel()
    {
        string fileName = FileName.Text.Trim();
        ModelManager.SaveModel(fileName);
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

        ModelManager.Render();
    }

    #region Saved ui functions (Do not delete)

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
}