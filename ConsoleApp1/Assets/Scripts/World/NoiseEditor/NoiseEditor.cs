using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class NoiseEditor : ScriptingNode
{
    public static int NodePanelWidth = Game.Width - 300;
    public static int NodePanelHeight = Game.Height;

    public static Vector2i NodeWindowPosition { get; private set; } = new Vector2i(0, 0);
    public static Vector2i InternalNodeWindowPosition { get; private set; } = new Vector2i(0, 200);
    public static float NoiseSize = 0.01f;
    public static Vector2 Offset = new Vector2(0, 0);

    public static Vector2 DisplayPosition = new Vector2(100, 100);
    public static Vector2 DisplayWindowSize = new Vector2(100, 100);
    
    public static Matrix4 DisplayProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -4, 0);
    public static Matrix4 NodePanelProjectionMatrix = Matrix4.CreateOrthographicOffCenter(7, NodePanelWidth - 7, NodePanelHeight - 7, 7, -2, 2);
    public static Matrix4 SelectionProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -3, 0);

    public static ShaderProgram VoronoiShader = new ShaderProgram("Painting/Rectangle.vert", "Noise/Voronoi.frag");
    public static VAO VoronoiVAO = new VAO();

    public static bool Selected = false;
    public static List<ConnectorNode> SelectedNodes = [];

    public UIController DisplayController;
    public UIController MainWindowController;
    public static UIController SidePanelController;
    public static UIInputField NodeNameField;
    public UIController NodeController;
    public UIController SelectionController;

    private UICollection _mainWindowCollection;
    private UIImage _mainWindowBackground;

    private UIRectangleDisplayPrefab _displayPrefab;


    public static UIListPrefab FileList;


    public UICollection SelectionCollection;
    public UICollection EmbeddedCollection;

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    private Vector2 NodePosition = new Vector2(0, 0);

    private bool _isScalingNoise = false;

    private ColorPicker _colorPicker = new ColorPicker(300, 200, (200, 200));

    private float VoronoiSize = 10f;

    private List<CurveWindow> _curveWindows = new List<CurveWindow>();

    public NoiseEditor()
    {
        InternalNodeWindowPosition = new Vector2i(0, Game.Height - NodePanelHeight);

        DisplayController = new();
        MainWindowController = new();
        SidePanelController = new();
        NodeController = new();
        SelectionController = new();

        NoiseNodeManager.NodeController = NodeController;

        // *--- DisplayController ---*

        _displayPrefab = new(DisplayWindowSize, (DisplayPosition.X, DisplayPosition.Y, 0, 0), DisplayController)
        {
            Depth = 10f
        };
        _displayPrefab.Collection.CanTest = true;
        _displayPrefab.Background.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        _displayPrefab.Background.SetOnHold(MoveNoise);
        _displayPrefab.Background.SetOnHover(() => {
            float scrollDelta = Input.GetMouseScrollDelta().Y;
            if (scrollDelta != 0)
            {
                NoiseSize -= scrollDelta * GameTime.DeltaTime * 100 * NoiseSize;
                _isScalingNoise = true;
            }
        });
        _displayPrefab.Background.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        // *--- MainWindowController ---*

        _mainWindowCollection = new("MainWindowCollection", MainWindowController, AnchorType.TopLeft, PositionType.Absolute, (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 5f)
        {
            Depth = 0f
        };
        _mainWindowBackground = new("MainWindowBackground", MainWindowController, AnchorType.MiddleCenter, PositionType.Relative, (0.4f, 0.4f, 0.4f, 1f), (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 0, 11, (10, 0.05f));

        _mainWindowBackground.SetOnHover(ScaleSelectionWindow);
        _mainWindowBackground.SetOnHold(MoveNodeWindow);

        _mainWindowCollection.AddElements(_mainWindowBackground);

        MainWindowController.AddElements(_mainWindowCollection);

        // *--- SidePanelController ---*

        UICollection sidePanelCollection = new("SidePanelCollection", SidePanelController, AnchorType.ScaleRight, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (300, Game.Height), (0, 0, 0, 0), 0f);
        
        UIImage sidePanelBackground = new("SidePanelBackground", SidePanelController, AnchorType.ScaleFull, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0, 10, (10, 0.05f));
        UIVerticalCollection sidePanelVerticalCollection = new("SidePanelVerticalCollection", SidePanelController, AnchorType.ScaleRight, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (10, 10, 10, 10), 5f, 0);

        UIVerticalCollection filePanelCollection = new("FilePanelCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UICollection nodeNameCollection = new("NodeNameCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), 0f);
        UIImage nodeNameBackground = new("NodeNameBackground", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 11, (10f, 0.05f));
        NodeNameField = new("NodeNameField", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (280, 30), (7, 7, 0, 0), 0f, 10, (10f, 0.05f));
        NodeNameField.SetMaxCharCount(22).SetText("noise", 1.2f).SetTextType(TextType.Alphanumeric);
        NodeNameField.SetOnTextChange(() => {
            NoiseNodeManager.FileName = NodeNameField.Text.Trim();
            NoiseNodeManager.updateFileNameAction = () => NodeNameField.SetText(NoiseNodeManager.FileName, 1.2f).UpdateCharacters();
        });
        nodeNameBackground.SetScale((NodeNameField.Scale.X + 14, nodeNameBackground.Scale.Y));
        nodeNameCollection.AddElements(nodeNameBackground, NodeNameField);

        UIHorizontalCollection saveLoadCollection = new("SaveLoadCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 40), (0, 0, 0, 0), (0, 0, 0, 0), 10f, 0);

        UITextButton saveButton = new("SaveButton", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (135, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        saveButton.SetOnClick(() => {
            if (File.Exists(Path.Combine(Game.worldNoiseNodeNodeEditorPath, NoiseNodeManager.FileName + ".cWorldNode")))
            {
                PopUp.AddConfirmation("Overwrite nodes?", NoiseNodeManager.SaveNodes, null);
            }
            else
            {
                NoiseNodeManager.SaveNodes();
            }
        });
        saveButton.SetTextCharCount("Save", 1.2f);
        UITextButton loadButton = new("LoadButton", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (135, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        loadButton.SetOnClick(() => {
            if (!File.Exists(Path.Combine(Game.worldNoiseNodeNodeEditorPath, NoiseNodeManager.FileName + ".cWorldNode")))
            {
                Console.WriteLine("File not found!");
                PopUp.AddPopUp("File not found!");
            }
            else if (NoiseNodeManager.FileName != NoiseNodeManager.CurrentFileName)
            {
                Console.WriteLine("Save and load nodes?");
                PopUp.AddConfirmation("Save current nodes?", () => { NoiseNodeManager.SaveNodes(NoiseNodeManager.CurrentFileName); NoiseNodeManager.LoadNodes(); }, NoiseNodeManager.LoadNodes);
            }
            else
            {
                Console.WriteLine("Load nodes!");
                NoiseNodeManager.LoadNodes();
            }
        });
        loadButton.SetTextCharCount("Load", 1.2f);
        saveLoadCollection.AddElements(saveButton, loadButton);

        filePanelCollection.AddElements(nodeNameCollection, saveLoadCollection);

        FileList = new("ListPrefab", SidePanelController, (280, 300), (0, 0, 0, 0));

        foreach (var file in Directory.GetFiles(Game.worldNoiseNodeNodeEditorPath, "*.cWorldNode"))
        {
            string fileName = Path.GetFileName(file).Replace(".cWorldNode", "");
            var element = FileList.AddButton(fileName, out var button, out var deleteButton); 
            button.SetOnClick(() => {
                NoiseNodeManager.SaveNodes(NoiseNodeManager.CurrentFileName); NoiseNodeManager.LoadNodes(fileName);
            });
            deleteButton.SetOnClick(() => {
                NoiseNodeManager.DeleteFile(fileName, element);
            });
        }
        

        sidePanelVerticalCollection.AddElements(filePanelCollection, FileList.Collection);

        sidePanelCollection.AddElements(sidePanelBackground, sidePanelVerticalCollection);

        SidePanelController.AddElements(sidePanelCollection);

        // *--- NodeController ---*

        UISampleNodePrefab sampleNodePrefab = new("SampleNodePrefab", NodeController, (100, 100, 0, 0))
        {
            Depth = 1f
        };

        UIDisplayNodePrefab displayNodePrefab = new("DisplayNodePrefab", NodeController, (800, 100, 0, 0))
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(sampleNodePrefab);
        NoiseNodeManager.AddNode(displayNodePrefab);

        // *--- SelectionController ---*

        // -- Selection Collection --
        SelectionCollection = new("SelectionCollection", SelectionController, AnchorType.TopLeft, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (300, 300), (0, 0, 0, 0), 0f);
        UIVerticalCollection selectionVerticalCollection = new("SelectionVerticalCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UITextButton addSampleButton = new("AddSampleButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSampleButton.SetTextCharCount("Add Sample", 1.2f);
        UITextButton addVoronoiButton = new("AddVoronoiButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addVoronoiButton.SetTextCharCount("Add Voronoi", 1.2f);
        UITextButton addMinMaxInputButton = new("AddSingleInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMinMaxInputButton.SetTextCharCount("Add Min Max Input", 1.2f);
        UITextButton addDoubleInputButton = new("AddDoubleInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addDoubleInputButton.SetTextCharCount("Add Double Input", 1.2f);
        UITextButton addBaseInputButton = new("AddBaseInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addBaseInputButton.SetTextCharCount("Add Base Input", 1.2f);
        UITextButton addRangeButton = new("AddRangeButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addRangeButton.SetTextCharCount("Add Range", 1.2f);
        UITextButton addCombineButton = new("AddCombineButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addCombineButton.SetTextCharCount("Add Combine", 1.2f);
        UITextButton addInitMaskButton = new("AddInitMaskButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addInitMaskButton.SetTextCharCount("Add Init Mask", 1.2f);
        UITextButton addCurveButton = new("AddCurveButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addCurveButton.SetTextCharCount("Add Curve", 1.2f);

        // -- Embedded Collection --
        EmbeddedCollection = new("EmbeddedCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (300, 0, 0, 0), 0);
        
        // - AddVoronoiNodeCollection -
        UIVerticalCollection addVoronoiNodeCollection = new("AddVoronoiNodeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UITextButton addVoronoiButtonBasic = new("AddVoronoiButtonBasic", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addVoronoiButtonBasic.SetTextCharCount("Voronoi Basic", 1.2f);
        UITextButton addVoronoiButtonEdge = new("AddVoronoiButtonEdge", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addVoronoiButtonEdge.SetTextCharCount("Voronoi Edge", 1.2f);
        UITextButton addVoronoiButtonDistance = new("AddVoronoiButtonDistance", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addVoronoiButtonDistance.SetTextCharCount("Voronoi Distance", 1.2f);

        // - AddMinMaxInputTypeCollection -
        UIVerticalCollection addMinMaxInputTypeCollection = new("AddMinMaxInputTypeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UITextButton addClampButton = new("AddClampButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addClampButton.SetTextCharCount("Clamp", 1.2f);
        UITextButton addIgnoreButton = new("AddIgnoreButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addIgnoreButton.SetTextCharCount("Ignore", 1.2f);
        UITextButton addLerpButton = new("AddLerpButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addLerpButton.SetTextCharCount("Lerp", 1.2f);
        UITextButton addSlideButton = new("AddSlideButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSlideButton.SetTextCharCount("Slide", 1.2f);
        UITextButton addSmoothButton = new("AddSmoothButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f)); 
        addSmoothButton.SetTextCharCount("Smooth", 1.2f);

        // - AddDoubleInputTypeCollection -
        UIVerticalCollection addDoubleInputTypeCollection = new("AddDoubleInputTypeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UITextButton addAddButton = new("AddAddButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addAddButton.SetTextCharCount("Add", 1.2f);
        UITextButton addSubButton = new("AddSubButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSubButton.SetTextCharCount("Subtract", 1.2f); 
        UITextButton addMulButton = new("AddMulButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMulButton.SetTextCharCount("Multiply", 1.2f);
        UITextButton addDivButton = new("AddDivButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 1.2f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addDivButton.SetTextCharCount("Divide", 1.2f);
        UITextButton addMinButton = new("AddMinButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMinButton.SetTextCharCount("Min", 1.2f);
        UITextButton addMaxButton = new("AddMaxButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMaxButton.SetTextCharCount("Max", 1.2f);
        UITextButton addModButton = new("AddModButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addModButton.SetTextCharCount("Mod", 1.2f);
        UITextButton addPowerButton = new("AddPowerButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addPowerButton.SetTextCharCount("Power", 1.2f);

        // - AddBaseInputNodeCollection -
        UIVerticalCollection addBaseInputNodeCollection = new("AddBaseInputNodeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UITextButton addInvertButton = new("AddInvertButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addInvertButton.SetTextCharCount("Invert", 1.2f);
        UITextButton addAbsButton = new("AddAbsButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addAbsButton.SetTextCharCount("Absolute", 1.2f);
        UITextButton addSquareButton = new("AddSquareButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSquareButton.SetTextCharCount("Square", 1.2f);
        UITextButton addSinButton = new("AddSinButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSinButton.SetTextCharCount("Sine", 1.2f);
        UITextButton addCosButton = new("AddCosButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addCosButton.SetTextCharCount("Cosine", 1.2f);
        UITextButton addTanButton = new("AddTanButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addTanButton.SetTextCharCount("Tangent", 1.2f);

        // - AddInitMaskNodeCollection -
        UIVerticalCollection addInitMaskNodeCollection = new("AddInitMaskNodeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UITextButton addInitMaskThresholdButton = new("AddInitMaskThresholdButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addInitMaskThresholdButton.SetTextCharCount("Add Threshold", 1.2f);
        UITextButton addInitMaskMinMaxButton = new("AddInitMaskMinMaxButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addInitMaskMinMaxButton.SetTextCharCount("Add Min Max", 1.2f);


        addSampleButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UISampleNodePrefab sampleNodePrefab = new("SampleNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(sampleNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addRangeButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UIRangeNodePrefab rangeNodePrefab = new("RangeNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(rangeNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addCombineButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UICombineNodePrefab combineNodePrefab = new("CombineNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(combineNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addCurveButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UICurveNodePrefab curveNodePrefab = new("CurveNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(curveNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addVoronoiButton.SetOnClick(() =>
        {
            EmbeddedCollection.SetVisibility(false);
            addVoronoiNodeCollection.SetVisibility(!addVoronoiNodeCollection.Visible);
        });

        addMinMaxInputButton.SetOnClick(() =>
        {
            EmbeddedCollection.SetVisibility(false);
            addMinMaxInputTypeCollection.SetVisibility(!addMinMaxInputTypeCollection.Visible);
        });

        addDoubleInputButton.SetOnClick(() =>
        {
            EmbeddedCollection.SetVisibility(false);
            addDoubleInputTypeCollection.SetVisibility(!addDoubleInputTypeCollection.Visible);
        });

        addBaseInputButton.SetOnClick(() =>
        {
            EmbeddedCollection.SetVisibility(false);
            addBaseInputNodeCollection.SetVisibility(!addBaseInputNodeCollection.Visible);
        });

        addInitMaskButton.SetOnClick(() =>
        {
            EmbeddedCollection.SetVisibility(false);
            addInitMaskNodeCollection.SetVisibility(!addInitMaskNodeCollection.Visible);
        });


        addVoronoiButtonBasic.SetOnClick(() =>
        {
            AddVoronoiOperationType(VoronoiOperationType.Basic);
            SelectionCollection.SetVisibility(false);
        });

        addVoronoiButtonEdge.SetOnClick(() =>
        {
            AddVoronoiOperationType(VoronoiOperationType.Edge);
            SelectionCollection.SetVisibility(false);
        });

        addVoronoiButtonDistance.SetOnClick(() =>
        {
            AddVoronoiOperationType(VoronoiOperationType.Distance);
            SelectionCollection.SetVisibility(false);
        });


        addClampButton.SetOnClick(() =>
        {
            AddMinMaxInputType(MinMaxInputOperationType.Clamp);
            SelectionCollection.SetVisibility(false);
        });

        addIgnoreButton.SetOnClick(() =>
        {
            AddMinMaxInputType(MinMaxInputOperationType.Ignore);
            SelectionCollection.SetVisibility(false);
        });

        addLerpButton.SetOnClick(() =>
        {
            AddMinMaxInputType(MinMaxInputOperationType.Lerp);
            SelectionCollection.SetVisibility(false);
        });

        addSlideButton.SetOnClick(() =>
        {
            AddMinMaxInputType(MinMaxInputOperationType.Slide);
            SelectionCollection.SetVisibility(false);
        });

        addSmoothButton.SetOnClick(() =>
        {
            AddMinMaxInputType(MinMaxInputOperationType.Smooth);
            SelectionCollection.SetVisibility(false);
        });


        addAddButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Add);
            SelectionCollection.SetVisibility(false);
        });

        addSubButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Subtract);
            SelectionCollection.SetVisibility(false);
        });

        addMulButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Multiply);
            SelectionCollection.SetVisibility(false);
        });

        addDivButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Divide);
            SelectionCollection.SetVisibility(false);
        });

        addMaxButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Max);
            SelectionCollection.SetVisibility(false);
        });

        addMinButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Min);
            SelectionCollection.SetVisibility(false);
        });

        addModButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Mod);
            SelectionCollection.SetVisibility(false);
        });

        addPowerButton.SetOnClick(() =>
        {
            AddDoubleInputType(DoubleInputOperationType.Power);
            SelectionCollection.SetVisibility(false);
        });



        addInvertButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Invert);
            SelectionCollection.SetVisibility(false);
        });

        addAbsButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Absolute);
            SelectionCollection.SetVisibility(false);
        });

        addSquareButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Square);
            SelectionCollection.SetVisibility(false);
        });

        addSinButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Sin);
            SelectionCollection.SetVisibility(false);
        });

        addCosButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Cos);
            SelectionCollection.SetVisibility(false);
        });

        addTanButton.SetOnClick(() =>
        {
            AddBaseInputType(BaseInputOperationType.Tan);
            SelectionCollection.SetVisibility(false);
        });


        addInitMaskThresholdButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UIThresholdInitMaskNodePrefab initMaskNodePrefab = new("InitMaskNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(initMaskNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addInitMaskMinMaxButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UIMinMaxInitMaskNodePrefab initMaskNodePrefab = new("InitMaskNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(initMaskNodePrefab);
            SelectionCollection.SetVisibility(false);
        });


        addVoronoiNodeCollection.AddElements(addVoronoiButtonBasic, addVoronoiButtonEdge, addVoronoiButtonDistance);
        addMinMaxInputTypeCollection.AddElements(addClampButton, addIgnoreButton, addLerpButton, addSlideButton, addSmoothButton);
        addDoubleInputTypeCollection.AddElements(addAddButton, addSubButton, addMulButton, addDivButton, addMinButton, addMaxButton, addModButton, addPowerButton);
        addBaseInputNodeCollection.AddElements(addInvertButton, addAbsButton, addSquareButton, addSinButton, addCosButton, addTanButton);
        addInitMaskNodeCollection.AddElements(addInitMaskThresholdButton, addInitMaskMinMaxButton);

        EmbeddedCollection.AddElements(addVoronoiNodeCollection, addMinMaxInputTypeCollection, addDoubleInputTypeCollection, addBaseInputNodeCollection, addInitMaskNodeCollection);
        selectionVerticalCollection.AddElements(addSampleButton, addVoronoiButton, addMinMaxInputButton, addDoubleInputButton, addBaseInputButton, addRangeButton, addCombineButton, addInitMaskButton, addCurveButton);

        SelectionCollection.AddElements(selectionVerticalCollection, EmbeddedCollection);

        SelectionController.AddElements(SelectionCollection);
        SelectionCollection.SetVisibility(false);
    }

    public void MoveNodeWindow()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero && Input.IsKeyDown(Keys.LeftControl))
        {
            Vector3 newMouseDelta = new Vector3(mouseDelta.X, mouseDelta.Y, 0f);
            Vector3 newPosition = NodeController.Position + newMouseDelta;
            NodeController.SetPosition(newPosition);

            NoiseNodeManager.UpdateLines();
        }
    }

    public void ScaleSelectionWindow()
    {
        float delta = Input.GetMouseScrollDelta().Y;
        if (_isScalingNoise || delta == 0 || !Input.IsKeyDown(Keys.LeftControl)) 
            return; 

        float scale = Mathf.Clamp(0.2f, 10f, NodeController.Scale + delta * GameTime.DeltaTime * NodeController.Scale * 100f);
        NodeController.SetScale(scale);

        NoiseNodeManager.UpdateLines();
    }

    public void AddVoronoiOperationType(VoronoiOperationType type)
    {
        UIVoronoiPrefab voronoiNodePrefab = new("VoronoiNodePrefab", NodeController, (NodePosition.X, NodePosition.Y, 0, 0), type)
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(voronoiNodePrefab);
        SelectionCollection.SetVisibility(false);
    }

    public void AddMinMaxInputType(MinMaxInputOperationType type)
    {
        UIMinMaxInputNodePrefab singleInputNodePrefab = new("SingleInputNodePrefab", NodeController, (NodePosition.X, NodePosition.Y, 0, 0), type)
        {
            Depth = 1f 
        };

        NoiseNodeManager.AddNode(singleInputNodePrefab);
        SelectionCollection.SetVisibility(false);
    }

    public void AddDoubleInputType(DoubleInputOperationType type)
    {
        UIDoubleInputNodePrefab doubleInputNodePrefab = new("DoubleInputNodePrefab", NodeController, (NodePosition.X, NodePosition.Y, 0, 0), type)
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(doubleInputNodePrefab);
        SelectionCollection.SetVisibility(false);
    }

    public void AddBaseInputType(BaseInputOperationType type)
    {
        UIBaseInputNodePrefab baseInputNodePrefab = new("BaseInputNodePrefab", NodeController, (NodePosition.X, NodePosition.Y, 0, 0), type)
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(baseInputNodePrefab);
        SelectionCollection.SetVisibility(false);
    }


    void Start()
    {
         NoiseGlslNodeManager.Compile([]);
    }

    void Awake()
    {
        WorldManager.Delete();
    }

    void Resize()
    {
        ResizeNodeWindow();

        //_colorPicker.Resize();
        DisplayController.Resize();
        MainWindowController.Resize();
        SidePanelController.Resize();
        NodeController.Resize();
        SelectionController.Resize();
    }

    void Update()
    {
        bool holdingShift = Input.IsKeyDown(Keys.LeftShift);
        // Save
        if (Input.IsKeyAndControlPressed(Keys.S))
        {
            NoiseNodeManager.SaveNodes();
        }
        
        // Load
        if (Input.IsKeyAndControlPressed(Keys.L))
        {
            NoiseNodeManager.LoadNodes();
        }

        // Reload
        if (Input.IsKeyAndControlPressed(Keys.R))
        {
            NoiseGlslNodeManager.Reload();
        }

        if (Input.IsMousePressed(MouseButton.Right))
        {
            SelectionCollection.SetVisibility(!SelectionCollection.Visible);
            EmbeddedCollection.SetVisibility(false);
                
            NodePosition = Input.GetMousePosition();
            SelectionController.SetPosition(new Vector3(NodePosition.X, NodePosition.Y, 0f));
        }

        Selected = false;
        //_colorPicker.Update();
        NodeController.Test(NodeWindowPosition);
        UICurveNodePrefab.Update();

        if (!Selected && Input.IsMousePressed(MouseButton.Left) && !holdingShift)
        {
            DeselectNodes();
        }

        if (Input.IsKeyPressed(Keys.Delete))
        {
            DeleteNodes();
        }

        if (Input.IsKeyDown(Keys.G))
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero)
                return;

            Vector2 mousePosition = Input.GetMousePosition();
            Vector2 delta = mouseDelta * (1 / NodeController.Scale);

            MoveNodes(delta);
            NoiseNodeManager.UpdateLines();
        }
        
        DisplayController.Test();
        SidePanelController.Test();
        SelectionController.Test();

        MainWindowController.Test();

        if (Input.IsKeyAndControlPressed(Keys.B))
        {
            Console.WriteLine("Display controller:");
            DisplayController.PrintMemory();
            Console.WriteLine("Main window controller:");
            MainWindowController.PrintMemory(); 
            Console.WriteLine("Side panel controller:");
            SidePanelController.PrintMemory();
            Console.WriteLine("Node controller:");
            NodeController.PrintMemory();
            Console.WriteLine("Selection controller:");
            SelectionController.PrintMemory();
        }

        DisplayPosition = _displayPrefab.Position;
        _isScalingNoise = false;

        Vector2 mouseScrollDelta = Input.GetMouseScrollDelta();
        if (mouseScrollDelta.Y != 0)
        {
            VoronoiSize -= mouseScrollDelta.Y * GameTime.DeltaTime * 100f * VoronoiSize;
        }

        if (Input.IsKeyPressed(Keys.J)) 
        {
            NoiseNodeManager.DeleteAll();
        }

        //_inventory.Update();
    }

    public static void SelectNode(ConnectorNode node)
    {
        if (SelectedNodes.Contains(node))
            return;

        node.Select();
        SelectedNodes.Add(node);
        Selected = true;
    }

    public static void MoveNodes(Vector2 delta)
    {
        foreach (var node in SelectedNodes)
        {
            node.Move(delta);
        }
    }

    public static void DeselectNodes()
    {
        foreach (var node in SelectedNodes)
        {
            node.Deselect();
        }
        SelectedNodes = [];
    }

    public static void DeleteNodes()
    {
        foreach (var node in SelectedNodes)
        {
            NoiseNodeManager.RemoveNode(node, true);
        }
        SelectedNodes = [];
    }

    void Render()
    {
        GL.DepthFunc(DepthFunction.Less);

        MainWindowController.RenderDepthTest();

        GL.Viewport(InternalNodeWindowPosition.X + 7, InternalNodeWindowPosition.Y + 7, NodePanelWidth - 14, NodePanelHeight - 14);

        NodeController.RenderDepthTest(NodePanelProjectionMatrix);
        UICurveNodePrefab.Render(NodeController.ModelMatrix, NodePanelProjectionMatrix);
        NoiseNodeManager.RenderLine(NodePanelProjectionMatrix);

        GL.Clear(ClearBufferMask.DepthBufferBit);

        GL.Viewport(0, 0, Game.Width, Game.Height);
        
        SidePanelController.RenderDepthTest();
        DisplayController.RenderDepthTest();
        NoiseGlslNodeManager.Render(DisplayProjectionMatrix, DisplayPosition, _displayPrefab.Scale, NoiseSize, Offset, _colorPicker.Color);

        if (SelectionCollection.Visible)
            SelectionController.RenderDepthTest();

        //_colorPicker.RenderTexture();

        /*
        Matrix4 model = Matrix4.CreateTranslation(100, 100, 0);
        Matrix4 projection = UIController.OrthographicProjection;

        VoronoiShader.Bind();

        int modelLocation = VoronoiShader.GetLocation("model");
        int projectionLocation = VoronoiShader.GetLocation("projection");
        int sizeLocation = VoronoiShader.GetLocation("size");
        int voronoiSizeLocation = VoronoiShader.GetLocation("voronoiSize");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, new Vector2(500, 500));
        GL.Uniform1(voronoiSizeLocation, VoronoiSize);

        VoronoiVAO.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        VoronoiVAO.Unbind();

        VoronoiShader.Unbind();
        */


        //_inventory.Render();
    }

    void Exit()
    {
        Console.WriteLine("Exiting Noise Editor...");
        CWorldMultithreadNodeManager.Clear();
        NoiseNodeManager.Compile(CWorldMultithreadNodeManager.MainNodeManager);
        CWorldMultithreadNodeManager.Copy(ThreadPool.ThreadCount);
    }

    public void ResizeNodeWindow()
    {
        NodePanelWidth = Game.Width - 300;
        NodePanelHeight = Game.Height;

        InternalNodeWindowPosition = new Vector2i(0, Game.Height - NodePanelHeight);

        _mainWindowCollection.SetScale((NodePanelWidth, NodePanelHeight));
        _mainWindowBackground.SetScale((NodePanelWidth, NodePanelHeight));

        DisplayProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -4, 0);
        NodePanelProjectionMatrix = Matrix4.CreateOrthographicOffCenter(7, NodePanelWidth - 7, NodePanelHeight - 7, 7, -2, 2);
    }

    public void SetNodeWindowSize(Vector2i size)
    {
        NodePanelWidth = size.X;
        NodePanelHeight = size.Y;
        _mainWindowCollection.SetScale(size);
        _mainWindowBackground.SetScale(size + new Vector2(12, 12));
        _mainWindowCollection.Align();
        _mainWindowCollection.UpdateTransformation();
    }

    public void MoveNoise()
    {
        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = Input.GetMouseDelta() * GameTime.DeltaTime * 100000f * NoiseSize;

        Offset = new Vector2(Offset.X - delta.X, Offset.Y - delta.Y);

        _oldMouseButtonPosition = mousePosition;
    }
}