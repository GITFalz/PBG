using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class NoiseEditor : ScriptingNode
{
    public static int NodePanelWidth = 1200;
    public static int NodePanelHeight = 700;

    public static Vector2i NodeWindowPosition { get; private set; } = new Vector2i(0, 0);
    public static Vector2i InternalNodeWindowPosition { get; private set; } = new Vector2i(0, 200);
    public static float NoiseSize = 0.01f;
    public static Vector2 Offset = new Vector2(0, 0);

    public static Vector2 DisplayPosition = new Vector2(100, 100);
    public static Vector2 DisplayWindowSize = new Vector2(100, 100);
    
    public static Matrix4 DisplayProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -4, 0);
    public static Matrix4 NodePanelProjectionMatrix = Matrix4.CreateOrthographicOffCenter(7, NodePanelWidth - 7, NodePanelHeight - 7, 7, -2, 2);
    public static Matrix4 SelectionProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -3, 0);

    public UIController DisplayController;
    public UIController MainWindowController;
    public UIController SidePanelController;
    public UIController NodeController;
    public UIController SelectionController;

    private UICollection _mainWindowCollection;
    private UIImage _mainWindowBackground;

    private UIRectangleDisplayPrefab _displayPrefab;


    public UICollection SelectionCollection;
    public UICollection EmbeddedCollection;

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public NoiseEditor()
    {
        InternalNodeWindowPosition = new Vector2i(0, Game.Height - NodePanelHeight);

        DisplayController = new();
        MainWindowController = new();
        SidePanelController = new();
        NodeController = new();
        SelectionController = new();

        // *--- DisplayController ---*
        UIMesh displayUiMesh = DisplayController.UiMesh;
        TextMesh displayTextMesh = DisplayController.textMesh;

        _displayPrefab = new(DisplayWindowSize, (DisplayPosition.X, DisplayPosition.Y, 0, 0), DisplayController)
        {
            Depth = 10f
        };
        _displayPrefab.Collection.CanTest = true;
        _displayPrefab.Collection.SetOnHold(MoveNoise);

        DisplayController.AddElements(_displayPrefab.GetMainElements());

        // *--- MainWindowController ---*
        UIMesh windowUiMesh = MainWindowController.UiMesh;
        TextMesh windowTextMesh = MainWindowController.textMesh;

        _mainWindowCollection = new("MainWindowCollection", MainWindowController, AnchorType.TopLeft, PositionType.Absolute, (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 5f)
        {
            Depth = 0f
        };
        _mainWindowBackground = new("MainWindowBackground", MainWindowController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f), (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 5f, 0, (10, 0.05f), windowUiMesh);

        _mainWindowCollection.AddElements(_mainWindowBackground);

        MainWindowController.AddElements(_mainWindowCollection);

        // *--- SidePanelController ---*
        UIMesh sideUiMesh = SidePanelController.UiMesh;
        TextMesh sideTextMesh = SidePanelController.textMesh;

        UIImage sidePanelBackground = new("SidePanelBackground", SidePanelController, AnchorType.ScaleRight, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 5f, 0, (10, 0.05f), sideUiMesh);

        SidePanelController.AddElements(sidePanelBackground);

        // *--- NodeController ---*
        UIMesh nodeUiMesh = NodeController.UiMesh;
        TextMesh nodeTextMesh = NodeController.textMesh;

        UISampleNodePrefab sampleNodePrefab = new("SampleNodePrefab", NodeController, (100, 100, 0, 0))
        {
            Depth = 1f
        };

        UISingleInputNodePrefab singleInputNodePrefab = new("SingleInputNodePrefab", NodeController, (300, 100, 0, 0), SingleInputOperationType.Smooth)
        {
            Depth = 1f
        };

        UIDisplayNodePrefab displayNodePrefab = new("DisplayNodePrefab", NodeController, (800, 100, 0, 0))
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(sampleNodePrefab);
        NoiseNodeManager.AddNode(singleInputNodePrefab);
        NoiseNodeManager.AddNode(displayNodePrefab);

        NodeController.AddElements(sampleNodePrefab, singleInputNodePrefab, displayNodePrefab);

        // *--- SelectionController ---*
        UIMesh selectionUiMesh = SelectionController.UiMesh;
        TextMesh selectionTextMesh = SelectionController.textMesh;

        UIMesh maskSelectionMesh = SelectionController.maskMesh;
        UIMesh maskedSelectionMesh = SelectionController.maskeduIMesh;
        TextMesh maskedSelectionTextMesh = SelectionController.maskedTextMesh;

        // -- Selection Collection --
        SelectionCollection = new("SelectionCollection", SelectionController, AnchorType.TopLeft, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (300, 300), (0, 0, 0, 0), 0f);
        UIVerticalCollection selectionVerticalCollection = new("SelectionVerticalCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UIButton addSampleButton = new("AddSampleButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);
        UIButton addSingleInputButton = new("AddSingleInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);

        // -- Embedded Collection --
        EmbeddedCollection = new("EmbeddedCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (300, 0, 0, 0), 0);
        
        // - AddSingleInputTypeCollection -
        UIVerticalCollection addSingleInputTypeCollection = new("AddSingleInputTypeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UIButton addClampButton = new("AddClampButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);
        UIButton addIgnoreButton = new("AddIgnoreButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);
        UIButton addLerpButton = new("AddLerpButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);
        UIButton addSlideButton = new("AddSlideButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);
        UIButton addSmoothButton = new("AddSmoothButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), selectionUiMesh, UIState.Interactable);


        addSampleButton.SetOnClick(() =>
        {
            Vector2 mousePosition = Input.GetMousePosition();
            UISampleNodePrefab sampleNodePrefab = new("SampleNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0))
            {
                Depth = 1f
            };

            NoiseNodeManager.AddNode(sampleNodePrefab);
            NodeController.AddElements(sampleNodePrefab);
            SelectionCollection.SetVisibility(false);
        });

        addSingleInputButton.SetOnClick(() =>
        {
            Console.WriteLine("AddSingleInputButton Clicked");
            EmbeddedCollection.SetVisibility(false);
            addSingleInputTypeCollection.SetVisibility(!addSingleInputTypeCollection.Visible);
        });


        addClampButton.SetOnClick(() =>
        {
            AddSingleInputType(SingleInputOperationType.Clamp);
            SelectionCollection.SetVisibility(false);
        });

        addIgnoreButton.SetOnClick(() =>
        {
            AddSingleInputType(SingleInputOperationType.Ignore);
            SelectionCollection.SetVisibility(false);
        });

        addLerpButton.SetOnClick(() =>
        {
            AddSingleInputType(SingleInputOperationType.Lerp);
            SelectionCollection.SetVisibility(false);
        });

        addSlideButton.SetOnClick(() =>
        {
            AddSingleInputType(SingleInputOperationType.Slide);
            SelectionCollection.SetVisibility(false);
        });

        addSmoothButton.SetOnClick(() =>
        {
            AddSingleInputType(SingleInputOperationType.Smooth);
            SelectionCollection.SetVisibility(false);
        });

        addSingleInputTypeCollection.AddElements(addClampButton, addIgnoreButton, addLerpButton, addSlideButton, addSmoothButton);

        EmbeddedCollection.AddElements(addSingleInputTypeCollection);
        selectionVerticalCollection.AddElements(addSampleButton, addSingleInputButton);

        SelectionCollection.AddElements(selectionVerticalCollection, EmbeddedCollection);

        SelectionController.AddElements(SelectionCollection);
        SelectionCollection.SetVisibility(false);
    }

    public void AddSingleInputType(SingleInputOperationType type)
    {
        Vector2 mousePosition = Input.GetMousePosition();
        UISingleInputNodePrefab singleInputNodePrefab = new("SingleInputNodePrefab", NodeController, (mousePosition.X, mousePosition.Y, 0, 0), type)
        {
            Depth = 1f
        };

        NoiseNodeManager.AddNode(singleInputNodePrefab);
        NodeController.AddElements(singleInputNodePrefab);
        SelectionCollection.SetVisibility(false);
    }


    public override void Start()
    {
         NoiseGlslNodeManager.Compile([]);
    }

    public override void Awake()
    {
    
    }

    public override void Resize()
    {
        ResizeNodeWindow();

        DisplayController.Resize();
        MainWindowController.Resize();
        SidePanelController.Resize();
        NodeController.Resize();
        SelectionController.Resize();
    }

    public override void Update()
    {
        float scrollDelta = Input.GetMouseScrollDelta().Y;
        if (scrollDelta != 0)
            NoiseSize -= scrollDelta * GameTime.DeltaTime * 100 * NoiseSize;

        if (Input.IsMousePressed(MouseButton.Right))
        {
            SelectionCollection.SetVisibility(!SelectionCollection.Visible);
            EmbeddedCollection.SetVisibility(false);
                
            Vector2 mousePosition = Input.GetMousePosition();
            SelectionController.Position = new Vector3(mousePosition.X, mousePosition.Y, 1);
        }

        DisplayController.Test();
        MainWindowController.Test();
        SidePanelController.Test();
        NodeController.Test(NodeWindowPosition);
        SelectionController.Test();

        DisplayPosition = _displayPrefab.Position;
    }

    public override void Render()
    {
        NoiseGlslNodeManager.Render(DisplayProjectionMatrix, DisplayPosition, _displayPrefab.Scale, NoiseSize, Offset);

        DisplayController.Render();
        //MainWindowController.Render();
        //SidePanelController.Render();
        if (SelectionCollection.Visible)
            SelectionController.Render();

        NoiseNodeManager.RenderLine();

        GL.Viewport(InternalNodeWindowPosition.X + 7, InternalNodeWindowPosition.Y + 7, NodePanelWidth - 14, NodePanelHeight - 14);

        NodeController.Render(NodePanelProjectionMatrix);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }

    public override void Exit()
    {

    }

    public void ResizeNodeWindow()
    {
        NodePanelWidth = Game.Width - 300;
        NodePanelHeight = Mathf.Min(700, Game.Height - 200);

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