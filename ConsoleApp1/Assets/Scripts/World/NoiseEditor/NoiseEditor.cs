using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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

    public static ConnectorNode? SelectedNode = null;

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

    private Vector2 NodePosition = new Vector2(0, 0);

    private bool _isScalingNoise = false;

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
        UIMesh displayUiMesh = DisplayController.UiMesh;
        TextMesh displayTextMesh = DisplayController.TextMesh;

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
        UIMesh windowUiMesh = MainWindowController.UiMesh;
        TextMesh windowTextMesh = MainWindowController.TextMesh;

        _mainWindowCollection = new("MainWindowCollection", MainWindowController, AnchorType.TopLeft, PositionType.Absolute, (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 5f)
        {
            Depth = 0f
        };
        _mainWindowBackground = new("MainWindowBackground", MainWindowController, AnchorType.MiddleCenter, PositionType.Relative, (0.4f, 0.4f, 0.4f), (1, 1, 1), (NodePanelWidth, NodePanelHeight), (0, 0, 0, 0), 0, 11, (10, 0.05f), windowUiMesh);

        _mainWindowBackground.SetOnHover(ScaleSelectionWindow);
        _mainWindowBackground.SetOnHold(MoveNodeWindow);

        _mainWindowCollection.AddElements(_mainWindowBackground);

        MainWindowController.AddElements(_mainWindowCollection);

        // *--- SidePanelController ---*
        UIMesh sideUiMesh = SidePanelController.UiMesh;
        TextMesh sideTextMesh = SidePanelController.TextMesh;

        UICollection sidePanelCollection = new("SidePanelCollection", SidePanelController, AnchorType.ScaleRight, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (300, Game.Height), (0, 0, 0, 0), 0f);
        
        UIImage sidePanelBackground = new("SidePanelBackground", SidePanelController, AnchorType.ScaleRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0, 10, (10, 0.05f), sideUiMesh);
        UIVerticalCollection sidePanelVerticalCollection = new("SidePanelVerticalCollection", SidePanelController, AnchorType.ScaleRight, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (10, 10, 10, 10), 5f, 0);

        UIInputField nodeNameField = new("NodeNameField", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (280, 30), (0, 0, 0, 0), 0f, 10, (10f, 0.05f), sideTextMesh);
        nodeNameField.SetTextCharCount("Noise", 0.5f).SetTextType(TextType.Alphanumeric);

        sidePanelVerticalCollection.AddElements(nodeNameField);

        sidePanelCollection.AddElements(sidePanelBackground, sidePanelVerticalCollection);

        SidePanelController.AddElements(sidePanelCollection);

        // *--- NodeController ---*
        UIMesh nodeUiMesh = NodeController.UiMesh;
        TextMesh nodeTextMesh = NodeController.TextMesh;

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
        UIMesh selectionUiMesh = SelectionController.UiMesh;
        TextMesh selectionTextMesh = SelectionController.TextMesh;

        UIMesh maskSelectionMesh = SelectionController.maskMesh;
        UIMesh maskedSelectionMesh = SelectionController.maskeduIMesh;
        TextMesh maskedSelectionTextMesh = SelectionController.maskedTextMesh;

        // -- Selection Collection --
        SelectionCollection = new("SelectionCollection", SelectionController, AnchorType.TopLeft, PositionType.Absolute, (0.5f, 0.5f, 0.5f), (300, 300), (0, 0, 0, 0), 0f);
        UIVerticalCollection selectionVerticalCollection = new("SelectionVerticalCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0f, 0);

        UITextButton addSampleButton = new("AddSampleButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSampleButton.SetTextCharCount("Add Sample", 0.5f);
        UITextButton addMinMaxInputButton = new("AddSingleInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMinMaxInputButton.SetTextCharCount("Add Min Max Input", 0.5f);
        UITextButton addDoubleInputButton = new("AddDoubleInputButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addDoubleInputButton.SetTextCharCount("Add Double Input", 0.5f);

        // -- Embedded Collection --
        EmbeddedCollection = new("EmbeddedCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (300, 0, 0, 0), 0);
        
        // - AddMinMaxInputTypeCollection -
        UIVerticalCollection addMinMaxInputTypeCollection = new("AddMinMaxInputTypeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UITextButton addClampButton = new("AddClampButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addClampButton.SetTextCharCount("Clamp", 0.5f);
        UITextButton addIgnoreButton = new("AddIgnoreButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addIgnoreButton.SetTextCharCount("Ignore", 0.5f);
        UITextButton addLerpButton = new("AddLerpButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addLerpButton.SetTextCharCount("Lerp", 0.5f);
        UITextButton addSlideButton = new("AddSlideButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSlideButton.SetTextCharCount("Slide", 0.5f);
        UITextButton addSmoothButton = new("AddSmoothButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f)); 
        addSmoothButton.SetTextCharCount("Smooth", 0.5f);

        // - AddDoubleInputTypeCollection -
        UIVerticalCollection addDoubleInputTypeCollection = new("AddDoubleInputTypeCollection", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 30), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UITextButton addAddButton = new("AddAddButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addAddButton.SetTextCharCount("Add", 0.5f);
        UITextButton addSubButton = new("AddSubButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addSubButton.SetTextCharCount("Subtract", 0.5f); 
        UITextButton addMulButton = new("AddMulButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMulButton.SetTextCharCount("Multiply", 0.5f);
        UITextButton addDivButton = new("AddDivButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addDivButton.SetTextCharCount("Divide", 0.5f);
        UITextButton addMinButton = new("AddMinButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMinButton.SetTextCharCount("Min", 0.5f);
        UITextButton addMaxButton = new("AddMaxButton", SelectionController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (300, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        addMaxButton.SetTextCharCount("Max", 0.5f);
        

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


        addMinMaxInputTypeCollection.AddElements(addClampButton, addIgnoreButton, addLerpButton, addSlideButton, addSmoothButton);
        addDoubleInputTypeCollection.AddElements(addAddButton, addSubButton, addMulButton, addDivButton, addMinButton, addMaxButton);

        EmbeddedCollection.AddElements(addMinMaxInputTypeCollection, addDoubleInputTypeCollection);
        selectionVerticalCollection.AddElements(addSampleButton, addMinMaxInputButton, addDoubleInputButton);

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

        if (Input.IsMousePressed(MouseButton.Right))
        {
            SelectionCollection.SetVisibility(!SelectionCollection.Visible);
            EmbeddedCollection.SetVisibility(false);
                
            NodePosition = Input.GetMousePosition();
            SelectionController.SetPosition(new Vector3(NodePosition.X, NodePosition.Y, 0f));
        }

        NodeController.Test(NodeWindowPosition);
        DisplayController.Test();
        SidePanelController.Test();
        SelectionController.Test();

        MainWindowController.Test();

        if (SelectedNode != null)
        {
            if (Input.IsKeyPressed(Keys.Delete))
            {
                NoiseNodeManager.RemoveNode(SelectedNode, true);
                SelectedNode = null;
            }
        }

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
    }

    public override void Render()
    {
        MainWindowController.RenderNoDepthTest();

        GL.Viewport(InternalNodeWindowPosition.X + 7, InternalNodeWindowPosition.Y + 7, NodePanelWidth - 14, NodePanelHeight - 14);

        NodeController.RenderNoDepthTest(NodePanelProjectionMatrix);
        NoiseNodeManager.RenderLine(NodePanelProjectionMatrix);

        GL.Viewport(0, 0, Game.Width, Game.Height);
        
        SidePanelController.RenderNoDepthTest();
        DisplayController.RenderDepthTest();
        NoiseGlslNodeManager.Render(DisplayProjectionMatrix, DisplayPosition, _displayPrefab.Scale, NoiseSize, Offset);

        if (SelectionCollection.Visible)
            SelectionController.RenderNoDepthTest();
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