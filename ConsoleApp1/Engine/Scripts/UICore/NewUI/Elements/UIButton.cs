using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIButton : UIElement
{
    public UIMesh? uIMesh;

    public UIButton(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, UIMesh? uIMesh, UIState state) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex)
    {
        State = state;
    }

    public SerializableEvent? OnHover = null;
    public SerializableEvent? OnClick = null;
    public SerializableEvent? OnHold = null;
    public SerializableEvent? OnRelease = null;

    private bool _clicked = false;

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    public override bool Test()
    {
        TestButtons(IsMouseOver());
        return true;
    }
    
    public override bool Test(Vector2 offset)
    {
        TestButtons(IsMouseOver(offset));
        return true;
    }

    public override void Generate()
    {
        Align();
        if (State == UIState.InvisibleInteractable || uIMesh == null)
            return;
        GenerateUIQuad(out panel, uIMesh);
    }

    private void TestButtons(bool mouseOver)
    {
        if (mouseOver)
        {
            OnHover?.Invoke();
            
            if ( Input.IsMousePressed(MouseButton.Left) && !_clicked)
            {
                OnClick?.Invoke();
                _clicked = true;
            }
        }
        
        if (_clicked)
        {
            Game.SetCursorState(CursorState.Grabbed);
            OnHold?.Invoke();
        }
            
        if (Input.IsMouseReleased(MouseButton.Left) && _clicked)
        {
            Game.SetCursorState(CursorState.Normal);
            OnRelease?.Invoke();
            _clicked = false;
        }
    }
}