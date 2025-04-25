using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class UIRectangleDisplayPrefab : UIPrefab
{
    public UIImage Background;
    public UIButton MoveButton;
    public UIButton ScaleButton;

    public Vector2 Position;
    public Vector2 Scale;

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIRectangleDisplayPrefab(Vector2 scale, Vector4 offset, UIController controller) : base("RectangleDisplayPrefab", controller, offset)
    {
        Position = new Vector2(offset.X, offset.Y);

        Scale = scale;

        Collection = new UICollection("RectangleDisplayPrefabCollection", controller, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), Scale, Offset, 0);
        Background = new UIImage("RectangleDisplayPrefabBackground", controller, AnchorType.MiddleCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), Scale + (14, 14), (0, 0, 0, 0), 0, 11, (10, 0.05f));
        MoveButton = new UIButton("RectangleDisplayPrefabMoveButton", controller, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (Scale.X + 14, 14), (-6, -20, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable);
        ScaleButton = new UIButton("RectangleDisplayPrefabScaleButton", controller, AnchorType.BottomRight, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (20, 20), (6, 6, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable)
        {
            Depth = Depth + 10f
        };

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        ScaleButton.SetOnClick(SetOldMousePosition);
        ScaleButton.SetOnHold(ScaleNode);

        Collection.AddElements(Background, MoveButton, ScaleButton);

        Controller.AddElements(this);
    }

    public void SetScale(float constant)
    {
        Scale = new Vector2(constant, constant);
        Collection.SetScale(Scale);
        Background.SetScale(Scale + (14, 14));
        MoveButton.SetScale((Scale.X + 14, 14));
        Collection.Align();
        Collection.UpdateScale();
    }

    private void SetOldMousePosition()
    {
        _oldMouseButtonPosition = Input.GetMousePosition();
        Game.SetCursorState(CursorState.Normal);
    } 

    private void MoveNode()
    {
        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = (mousePosition - _oldMouseButtonPosition) * (1 / Collection.UIController.Scale);

        Position = new Vector2(Position.X + delta.X, Position.Y + delta.Y);

        Collection.SetOffset(new Vector4(Position.X, Position.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();
    }

    private void ScaleNode()
    {
        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = mousePosition - _oldMouseButtonPosition;

        Scale = new Vector2(Scale.X + delta.X, Scale.Y + delta.Y);

        Collection.SetScale(new Vector2(Scale.X + delta.X, Scale.Y + delta.Y));
        Background.SetScale(Scale + (14, 14));
        MoveButton.SetScale((Scale.X + 14, 14));
        ScaleButton.Align();
        ScaleButton.UpdateTransformation();
        Collection.UpdateScale();
        SetOldMousePosition();
    }
}