using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIEditor : ScriptingNode
{
    // -- Main Window --
    public UIController MainWindowController;

    // -- Editor Window --
    public UIController EditorWindowController;

    public UICollection EditorWindowCollection;

    // -- Side Panel --
    public UIController SidePanelController;

    public UICollection HierarchyCollection;

    public int Width = Game.Width - 300;
    public int Height = Game.Height;

    public Matrix4 EditorWindowMatrix;

    public UIEditor()
    {
        MainWindowController = new();
        EditorWindowController = new();
        SidePanelController = new();

        EditorWindowMatrix = Matrix4.CreateOrthographicOffCenter(7, Width - 7, Height - 7, 7, -2, 2);
    }

    void Start()
    {
        // -- Main Window --

        // -- Editor Window --

        EditorWindowCollection = new UICollection("editorWindowCollection", EditorWindowController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Width, Height), (7, 7, 0, 0), 0);

        EditorWindowController.AddElements(EditorWindowCollection);

        // -- Side Panel --
        
        UICollection sidePanelCollection = new UICollection("sidePanelCollection", SidePanelController, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (300, 200), (0, 0, 0, 0), 0);

        UIImage sidePanelBackground = new UIImage("sidePanelBackground", SidePanelController, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        //UIScrollView sidePanelScrollView = new UIScrollView("sidePanelVerticalCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, CollectionType.Vertical, (300, 200), (0, 0, 0, 0));
        UIVerticalCollection sidePanelScrollView = new UIVerticalCollection("sidePanelVerticalCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), (10, 10, 10, 10), 5f, 0);


        // Section: Save and Load

        // Section: Add UI Elements

        UICollection addUIElementsCollection = new UICollection("addUIElementsCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (280, 200), (0, 0, 0, 0), 0);

        UIImage addUIElementsBackground = new UIImage("addUIElementsBackground", SidePanelController, AnchorType.ScaleFull, PositionType.Relative, (0.4f, 0.4f, 0.4f, 1f), (0, 0, 0), (280, 100), (0, 0, 0, 0), 0, 11, (10, 0.05f));
        UIScrollView addUIElementsScrollView = new UIScrollView("addUIElementsVerticalCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, CollectionType.Vertical, (260, 300), (10, 10, 0, 0));
        addUIElementsScrollView.SetBorder((0, 0, 0, 0)); 

        // Add UIImage
        UICollection addUIImageCollection = new UICollection("addUIImageCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (260, 30), (0, 0, 0, 0), 0);
        UIButton addUIImageButton = new UIButton("addUIImageButton", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (260, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        UIText addUIImageText = new UIText("addUIImageText", SidePanelController, AnchorType.MiddleCenter, PositionType.Relative, Vector4.One, (0, 0, 0), (260, 30), (0, 0, 0, 0), 0);
        addUIImageButton.SetOnClick(AddUIImage);
        addUIImageText.SetTextCharCount("Add UIImage", 0.5f).SetTextType(TextType.Alphanumeric);
        addUIImageCollection.AddElements(addUIImageButton, addUIImageText);

        // Add UIButton
        UICollection addUIButtonCollection = new UICollection("addUIButtonCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (260, 30), (0, 0, 0, 0), 0);
        UIButton addUIButtonButton = new UIButton("addUIButtonButton", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (260, 30), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        UIText addUIButtonText = new UIText("addUIButtonText", SidePanelController, AnchorType.MiddleCenter, PositionType.Relative, Vector4.One, (0, 0, 0), (260, 30), (0, 0, 0, 0), 0);



        addUIElementsScrollView.AddElement(addUIImageCollection);

        float collectionHeight = addUIElementsScrollView.GetYScale();
        addUIElementsCollection.SetScale((280, 320));

        addUIElementsCollection.AddElements(addUIElementsBackground, addUIElementsScrollView);

        Console.WriteLine($"Add UI Elements Collection Height: {addUIElementsCollection.GetYScale()}");

        // Section: UI Element Hierarchy

        HierarchyCollection = new UICollection("uiElementHierarchyCollection", SidePanelController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (280, 200), (0, 0, 0, 0), 0);
        
        UIImage hierarchyBackground = new UIImage("hierarchyBackground", SidePanelController, AnchorType.ScaleFull, PositionType.Relative, (0.4f, 0.4f, 0.4f, 1f), (0, 0, 0), (280, 200), (0, 0, 0, 0), 0, 11, (10, 0.05f));
        UIScrollView hierarchyScrollView = new UIScrollView("hierarchyScrollView", SidePanelController, AnchorType.TopLeft, PositionType.Relative, CollectionType.Vertical, (280, 200), (0, 7, 0, 0));

        HierarchyCollection.AddElements(hierarchyBackground, hierarchyScrollView);

        // Section: Initialization
        sidePanelScrollView.AddElements(addUIElementsCollection, HierarchyCollection);

        sidePanelCollection.AddElements(sidePanelBackground, sidePanelScrollView);

        SidePanelController.AddElements(sidePanelCollection);
    }

    void Resize()
    {
        Width = Game.Width - 300;

        EditorWindowMatrix = Matrix4.CreateOrthographicOffCenter(7, Width - 7, Height - 7, 7, -2, 2);
    }

    void Update()
    {
        EditorWindowController.Test();
        SidePanelController.Test();
    }

    void Render()
    {
        GL.Viewport(7, 7, Width - 14, Height - 14);

        EditorWindowController.RenderNoDepthTest(EditorWindowMatrix);

        GL.Viewport(0, 0, Game.Width, Game.Height);

        SidePanelController.RenderNoDepthTest();
    }


    public void AddUIImage()
    {
        UIImage image = new UIImage("image", EditorWindowController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 100), (0, 0, 0, 0), 0, 10, (10f, 0.05f));
        EditorWindowCollection.AddElement(image);
        EditorWindowController.AddElement(image);
    }

    public void AddUIButton()
    {
        UIButton button = new UIButton("button", EditorWindowController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 100), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        EditorWindowCollection.AddElement(button);
        EditorWindowController.AddElement(button);
    }



    public void RegenerateHierarchy()
    {
    }
}