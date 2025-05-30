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
    public UIController UIHierarchyController;

    public UIScrollView HierarchyScrollView;

    public string currentModelName = "cube";
    public List<string> MeshSaveNames = new List<string>();

    // Main UI
    public UIInputField FileName;
    public UIInputField SnappingText;


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

    public Action FileManagerLoadAction = () => {};

    
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

    public FileManager FileManager;


    private bool _started = false;
    private bool _modelSelected = false;

    public GeneralModelingEditor(FileManager fileManager)
    {
        FileManager = fileManager;
        MainUi = new UIController();
        UIScrollViewTest = new UIController();

        modelingEditor = new ModelingEditor(this);
        riggingEditor = new RiggingEditor(this);
        animationEditor = new AnimationEditor(this);
        textureEditor = new TextureEditor(this);

        CurrentEditor = modelingEditor;

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

        vertexCollection.AddElements(vertexPanel, vertexSelectionButton);

        UIImage edgePanel = new("EdgePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection edgeCollection = new("EdgeCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        edgeCollection.AddElements(edgePanel, edgeSelectionButton);

        UIImage facePanel = new("FacePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection faceCollection = new("FaceCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (36, 36), (0, 0, 0, 0), 0);

        faceCollection.AddElements(facePanel, faceSelectionButton);

        UIImage snappingPanel = new("SnappingPanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (100, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIDepthCollection snappingCollection = new("SnappingCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (100, 36), (0, 0, 0, 0), 0);

        SnappingText = new("SnappingText", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 40, 0), (400, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        SnappingText.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText("0", 1f);
        SnappingText.OnTextChange = new SerializableEvent(SnappingField);

        snappingCollection.AddElements(snappingPanel, SnappingText);



        UIHorizontalCollection fileStacking = new("FileStacking", MainUi, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), (7, 7, 7, 7), 5, 0);

        FileName = new("ModelName", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (200, 36), (0, 0, 0, 0), 0, 0, (10, 0.15f));
        FileName.SetMaxCharCount(24).SetText("cube", 1.2f);


        UIButton saveModelButton = new("saveModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 95, (0, 0), UIState.Static);
        saveModelButton.SetOnClick(Save);

        UIButton loadModelButton = new("loadModelButton", MainUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 94, (0, 0), UIState.Static);
        loadModelButton.SetOnClick(Load);


        UIImage fileNamePanel = new("FileNamePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (310, 36), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIDepthCollection fileNameCollection = new("FileNameCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (310, 36), (0, 0, 0, 0), 0);

        fileNameCollection.AddElements(fileNamePanel, FileName);

        UIImage savePanel = new("SavePanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection saveCollection = new("SaveCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        saveCollection.AddElements(savePanel, saveModelButton);

        UIImage loadPanel = new("LoadPanel", MainUi, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIDepthCollection loadCollection = new("LoadCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);

        loadCollection.AddElements(loadPanel, loadModelButton);

        UICollection importCollection = new("ImportCollection", MainUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (36, 36), (0, 0, 0, 0), 0);
        UIButton importButton = new("ImportModelButton", MainUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (36, 36), (0, 0, 0, 0), 0, 90, (-1, -1), UIState.Static);
        importButton.SetOnClick(() =>
        {
            FileManager.Toggle();
        });

        importCollection.AddElements(importButton);

        stateCollection.AddElement(statePanel);

        stateStacking.AddElements(modelingButton, riggingButton, animationButton, textureButton, vertexCollection, edgeCollection, faceCollection, snappingCollection);

        fileStacking.AddElements(fileNameCollection, saveCollection, loadCollection, importCollection);

        stateCollection.AddElements(stateStacking, fileStacking);

        MainUi.AddElement(stateCollection);




        // Hierarchy panel collection
        UIHierarchyController = new UIController("HierarchyController");

        UICollection mainHierarchyCollection = new("MainHierarchyCollection", UIHierarchyController, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage hierarchyPanel = new("HierarchyPanel", UIHierarchyController, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height - 50), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        hierarchyPanel.SetTopPc(50);

        HierarchyScrollView = new("HierarchyScrollView", UIHierarchyController, AnchorType.ScaleRight, PositionType.Relative, CollectionType.Vertical, (238, Game.Height - 50), (-7, 0, 0, 0));
        HierarchyScrollView.SetBorder((5, 0, 5, 5));
        HierarchyScrollView.SetSpacing(0);
        HierarchyScrollView.SetBottomPx(5);
        HierarchyScrollView.SetTopPc(50);
        HierarchyScrollView.AddTopPx(5);

        mainHierarchyCollection.AddElements(hierarchyPanel, HierarchyScrollView);

        UIHierarchyController.AddElement(mainHierarchyCollection);
    }

    public void Start()
    {
        Console.WriteLine("Animation Editor Start");

        Game.camera = new Camera(Game.Width, Game.Height, new Vector3(0, 0, 5));
        ModelSettings.Camera = Game.camera;

        Transform.Position = new Vector3(0, 0, 0);


        Camera camera = Game.camera;

        camera.SetCameraMode(CameraMode.Free);

        camera.Position = new Vector3(0, 0, 5);
        camera.Pitch = 0;
        camera.Yaw = -90;

        camera.UpdateVectors();

        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);

        CurrentEditor = modelingEditor;
        modelingEditor.Start();
        riggingEditor.Start();
        animationEditor.Start();

        //MainUi.ToLines();
        foreach (var (name, model) in ModelManager.Models)
        {
            GenerateModelButton(model);
        }

        LoadAction = () =>
        {
            LoadModel();
            GenerateModelButton(ModelManager.SelectedModel);
        };
        SaveAction = SaveModel;
        FileManagerLoadAction = () =>
        {
            HierarchyScrollView.DeleteSubElements();
            foreach (var (name, model) in ModelManager.Models)
            {
                GenerateModelButton(model);
            }
        };

        _started = true;
    }

    public void Resize()
    {
        MainUi.Resize();
        UIScrollViewTest.Resize();
        UIHierarchyController.Resize(); 

        modelingEditor.Resize();
        riggingEditor.Resize();
        animationEditor.Resize();
        textureEditor.Resize();
    }

    public void Awake()
    {
        Game.BackgroundColor = (0.3f, 0.3f, 0.3f);
        CurrentEditor.Awake();
    }

    public void Update()
    {
        MainUi.Update();
        UIScrollViewTest.Update();
        UIHierarchyController.Update();

        if (FileManager.IsVisible && Input.IsKeyPressed(Keys.Enter))
        {
            for (int i = 0; i < FileManager.SelectedPaths.Count; i++)
            {
                string filePath = FileManager.SelectedPaths[i];
                ModelManager.LoadModelFromPath(filePath);
            }
            FileManager.SelectedPaths = [];
            FileManager.ToggleOff();
            FileManagerLoadAction?.Invoke();
        }

        CurrentEditor.Update();
    }

    public void Render()
    {     
        CurrentEditor.Render();

        MainUi.RenderNoDepthTest();
        UIHierarchyController.RenderNoDepthTest();
        UIScrollViewTest.RenderNoDepthTest();
    }

    public void Exit()
    {
        CurrentEditor.Exit();
        HierarchyScrollView.DeleteSubElements();    
    }
    
    public void GenerateModelButton(Model? model)
    {
        if (model == null)
            return;
        
        Console.WriteLine($"Generating button for model: {model.Name}");    
        UICollection modelCollection = new UICollection($"ModelCollection_{model.Name}", UIHierarchyController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), 0);

        UIButton modelButton = new UIButton($"Model_{model.Name}", UIHierarchyController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        UIText modelText = new UIText($"ModelText_{model.Name}", UIHierarchyController, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (300, 30), (10, 0, 0, 0), 0);
        modelText.SetTextCharCount(model.Name, 1f);
        modelButton.SetOnClick(() =>
        {
            _modelSelected = true;
            if (!Input.IsKeyDown(Keys.LeftShift) && ModelManager.SelectedModels.Count > 0 && (ModelManager.SelectedModels.Count > 1 || !ModelManager.SelectedModels.ContainsKey(model.Name)))
            {
                foreach (var (name, selected) in ModelManager.SelectedModels)
                {
                    ModelManager.UnSelect(selected.Model);
                    selected.Button.Color = (0.5f, 0.5f, 0.5f, 1f);
                    selected.Button.UpdateColor();
                }
                ModelManager.SelectedModels = [];
            }

            if (ModelManager.SelectedModels.Remove(model.Name))
            {
                ModelManager.UnSelect(model);
                modelButton.Color = (0.5f, 0.5f, 0.5f, 1f);
                modelButton.UpdateColor();
            }
            else
            {
                ModelManager.SelectedModels.Add(model.Name, new(modelButton, model));
                ModelManager.Select(model);
                modelButton.Color = (0.529f, 0.808f, 0.980f, 1.0f);
                modelButton.UpdateColor();
            }
        });

        modelCollection.AddElements(modelText, modelButton);

        HierarchyScrollView.AddElement(modelCollection);

        if (_started)
            UIHierarchyController.AddElement(modelCollection);
    }

    public void DoSwitchScene(BaseEditor editor)
    {
        CurrentEditor.Exit();
        CurrentEditor = editor;
        if (!CurrentEditor.Started)
            CurrentEditor.Start();
        CurrentEditor.Awake();
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
        string text = SnappingText.Text;
        SnappingFactor = Mathf.Clamp(0, 100, Float.Parse(text, 0.0f)); 
        Snapping = SnappingFactor != 0.0f;
    }
    #endregion
}