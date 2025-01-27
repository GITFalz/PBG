using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UiEditor : Component
{
    public UIController MainUi = new UIController();
    public UIController EditUi = new UIController();
    
    private List<UIElement> _selectedElements = new List<UIElement>();

    private Vector2 mouseOffset = (220, 0);
    
    public override void OnResize()
    {
        Console.WriteLine("UiEditor Resize");
        EditUi.OnResize();
    }
    
    public override void Start()
    {
        Console.WriteLine("UiEditor Start");

        // Main UI
        gameObject.Scene.UiController = MainUi;

        UIPanel mainPanel = new("MainPanel", AnchorType.ScaleLeft, PositionType.Absolute, (0, 0, 0), (210, 100), (5, 5, 5, 5), 0, 0, null);

        UIButton addPanelButton = new("AddPanelButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 0, 0), 0, 0, null, UIState.Static);
        UIButton addTextButton = new("AddTextButton", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 30, 0, 0), 0, 0, null, UIState.Static);

        addPanelButton.OnClick = new SerializableEvent(AddPanel);

        mainPanel.AddChild(addPanelButton);
        mainPanel.AddChild(addTextButton);

        MainUi.AddElement(mainPanel);

        MainUi.GenerateBuffers();

        // Edit UI
        UIPanel panel = new("TestPanel", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0, 0, null);
        UIText text = new("TestText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0, 0, null);

        text.SetText("Hello world", 0.7f);
        panel.AddChild(text);
        
        EditUi.AddElement(panel);
        
        EditUi.GenerateBuffers();

        foreach (var go in gameObject.Scene.GetHierarchy())
        {
            Console.WriteLine(new string(' ', go.offset) + go.gameObject.Name);
        }
    }

    public override void Awake()
    {
        Console.WriteLine("UiEditor Awake");
    }
    
    public override void Update()
    {
        EditUi.Test(mouseOffset);
        MainUi.Test();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            if (Game.Instance.CursorState == CursorState.Grabbed)
                Game.SetCursorState(CursorState.Normal);
            else
                Game.SetCursorState(CursorState.Grabbed);
        }
        
        if (Input.AreKeysPressed(Keys.Escape, Keys.Enter))
        {
            UIController.activeInputField = null;
        }
        
        if (Input.IsMousePressed(MouseButton.Left))
        {
            Console.WriteLine("Mouse Pressed");

            if (Input.IsKeyDown(Keys.LeftShift))
            {
                UIElement? element = EditUi.IsMouseOverIgnore(_selectedElements, mouseOffset);
                if (element == null)
                    return;

                _selectedElements.Add(element);
            }
            else
            {
                _selectedElements.Clear();
                
                UIElement? element = EditUi.IsMouseOver(mouseOffset);
                if (element == null)
                    return;

                Console.WriteLine("Element: " + element.Name);
                
                _selectedElements.Add(element);
            }
        }

        if (Input.IsKeyDown(Keys.G))
        {
            Vector2 delta = Input.GetMouseDelta();

            Console.WriteLine(_selectedElements.Count);

            foreach (var element in _selectedElements)
            {
                if (element.ParentElement != null && _selectedElements.Contains(element.ParentElement))
                    continue;

                element.Offset += new Vector4(delta.X, delta.Y, -delta.X, -delta.Y);
            }

            AlignList(_selectedElements);
            EditUi.UpdateMatrices();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            if (Input.IsKeyPressed(Keys.S))
            {
                //EditUi.SaveUi("test");
            }
        }
    }

    public override void Render()
    {
        int width = Game.width;
        int height = Game.height;
        
        GL.Viewport(220, 0, width - 440, height);
        Game.width = width - 440;
        Game.centerX = Game.width / 2;
        
        UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Game.width, Game.height, 0, -1, 1);

        EditUi.Render();
        
        GL.Viewport(0, 0, width, height);
        Game.width = width;
        Game.centerX = Game.width / 2;
        
        UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Game.width, Game.height, 0, -1, 1);
    }

    public override void Exit()
    {
        base.Exit();
    }


    public void AlignList(List<UIElement> elements)
    {
        foreach (var element in elements)
        {
            if (element is UIPanel panel)
            {
                panel.AlignAll();
                panel.UpdateAllTransformation();
            }
            else
            {
                element.Align();
                element.UpdateTransformation();
            }
        }
    }

    public void AddPanel()
    {
        UIPanel panel = new(EditUi.GetNextElementName(), AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0, 0, null);
        while (EditUi.ElementSharePosition(panel))
        {
            panel.Offset += new Vector4(10, 10, 0, 0);
        }
        EditUi.AddElement(panel);
        panel.Generate();
        EditUi.Buffers();
    }

    public void AddButton()
    {
        UIButton button = new(EditUi.GetNextElementName(), AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 20), (10, 10, 0, 0), 0, 0, null, UIState.Static);
        while (EditUi.ElementSharePosition(button))
        {
            button.Offset += new Vector4(10, 10, 0, 0);
        }
        EditUi.AddElement(button);
        button.Generate();
        EditUi.Buffers();
    }
}