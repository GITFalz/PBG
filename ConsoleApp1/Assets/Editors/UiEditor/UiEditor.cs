using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UiEditor : Updateable
{
    public OldUIController MainUi = new OldUIController();
    public OldUIController EditUi = new OldUIController();
    
    private List<UiElement> _selectedElements = new List<UiElement>();

    private Vector2 mouseOffset = (220, 0);
    
    public override void OnResize()
    {
        Console.WriteLine("UiEditor Resize");
        EditUi.Resize();
    }
    
    public override void Awake()
    {
        Console.WriteLine("UiEditor Awake");
    }
    
    public override void Start()
    {
        Console.WriteLine("UiEditor Start");

        gameObject.Scene.UiController = MainUi;
        gameObject.Scene.LoadUi();
        
        EditUi.LoadUi("test");
        
        EditUi.SetElementScreenOffset((220, 220, 0, 0));
        
        MainUi.Generate();
        EditUi.Generate();
    }

    public void Test()
    {
        Console.WriteLine("Test");
    }
    
    public override void Update()
    {
        EditUi.TestButtons(mouseOffset);
        
        if (Input.AreKeysPressed(Keys.Escape, Keys.Enter))
            OldUIController.activeInputField = null;
        
        if (Input.IsMousePressed(MouseButton.Left))
        {
            if (Input.IsKeyDown(Keys.LeftShift))
            {
                UiElement? element = EditUi.IsMouseOverIgnore(_selectedElements, mouseOffset);
                if (element == null)
                    return;

                _selectedElements.Add(element);
            }
            else
            {
                _selectedElements.Clear();
                
                UiElement? element = EditUi.IsMouseOver(mouseOffset);
                if (element == null)
                    return;
                
                _selectedElements.Add(element);
            }
        }

        if (Input.IsKeyDown(Keys.G))
        {
            Vector2 delta = Input.GetMouseDelta();

            foreach (var element in _selectedElements)
            {
                element.Move((delta.X, 0, delta.Y, 0));
            }
            
            EditUi.GenerateUi();
            EditUi.Update();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            if (Input.IsKeyPressed(Keys.S))
            {
                EditUi.SaveUi("test");
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
        
        OldUIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Game.width, Game.height, 0, -1, 1);

        EditUi.Render();
        
        GL.Viewport(0, 0, width, height);
        Game.width = width;
        Game.centerX = Game.width / 2;
        
        OldUIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Game.width, Game.height, 0, -1, 1);
    }

    public override void Exit()
    {
        base.Exit();
    }
}