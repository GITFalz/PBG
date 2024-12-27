using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class StaticButton : StaticElement
{
    public UiMesh Mesh;
    public TextMesh TextMesh;
    
    public StaticText? Text;
    
    public Action OnClick;
    public Action OnHold;
    public Action OnRelease;
    
    private bool _clicked = false;
    
    public StaticButton(StaticText text)
    {
        Name = "Static Panel";
        
        Text = text;
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public StaticButton()
    {
        Name = "Static Panel";
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public void SetMesh(UiMesh mesh)
    {
        Mesh = mesh;
    }
    
    public void SetMesh(UiMesh mesh, TextMesh textMesh)
    {
        Mesh = mesh;
        TextMesh = textMesh;
        
        Text?.SetMesh(TextMesh);
    }
    
    public override void Generate()
    {
        Align();
        Panel panel = UI.GeneratePanel(Origin, 64, 64, Scale.X, Scale.Y, 10f, new Vector4(10f, 10f, 10f, 10f));
        Mesh.AddUiElement(panel);
        
        Text?.Generate();
    }
    
    public void UpdateText()
    {
        Text?.UpdateText();
    }
    
    public bool IsMouseOver()
    {
        Vector2 pos = InputManager.GetMousePosition();
        return pos.X >= Origin.X && pos.X <= Origin.X + Scale.X && pos.Y >= Origin.Y && pos.Y <= Origin.Y + Scale.Y;
    }

    public void ButtonTest()
    {
        bool mouseOver = IsMouseOver();
        
        if (mouseOver && InputManager.IsMousePressed(MouseButton.Left) && !_clicked)
        {
            OnClick?.Invoke();
            _clicked = true;
        }
        
        if (_clicked)
        {
            Game.SetCursorState(CursorState.Grabbed);
            OnHold?.Invoke();
        }
            
        if (InputManager.IsMouseReleased(MouseButton.Left) && _clicked)
        {
            Game.SetCursorState(CursorState.Normal);
            OnRelease?.Invoke();
            _clicked = false;
        }
    }
}