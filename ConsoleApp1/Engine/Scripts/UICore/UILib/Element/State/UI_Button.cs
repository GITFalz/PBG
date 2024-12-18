using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public abstract class UI_Button : UI_Base
{
    public Action OnClick;
    public Action<MouseState> OnHold;
    public Action OnRelease;
    
    public bool holding = false;

    public void SetPosition(Vector3 pos)
    {
        position = pos;
    }

    public abstract void SetTextAlignment(UiAnchorAlignment alignment);
    
    public bool IsPointInBounds(Vector2 pos)
    {
        return pos.X > position.X && pos.X < position.X + size.X && pos.Y > position.Y && pos.Y < position.Y + size.Y;
    }
    
    public bool IsButtonPressed(MouseState mouse)
    {
        if (!IsPointInBounds(mouse.Position) || !mouse.IsButtonPressed(MouseButton.Left)) return false;
        
        holding = true;
        return true;
    }

    public bool IsButtonHeld(MouseState mouse)
    {
        return IsPointInBounds(mouse.Position) && holding;
    }

    public bool IsButtonReleased(MouseState mouse)
    {
        holding = false;
        
        return IsPointInBounds(mouse.Position);
    }

    private readonly Dictionary<TextType, TextMesh> _textMeshes = new Dictionary<TextType, TextMesh>()
    {
        { TextType.Static, new StaticTextMesh() },
        { TextType.Dynamic, new DynamicTextMesh() }
    };
    
    private readonly Dictionary<TextType, UI_Text> _textTypes = new Dictionary<TextType, UI_Text>()
    {
        { TextType.Static, new UI_StaticText() },
        { TextType.Dynamic, new UI_DynamicText() }
    };
}

public enum TextType
{
    Static,
    Dynamic
}