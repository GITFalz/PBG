using OpenTK.Mathematics;

public class UIRectangleDisplayPrefab : UIPrefab
{
    public UICollection Collection;
    public UIImage Background;
    public UIButton MoveButton;
    public UIButton ScaleButton;

    public Vector2 Position;
    public Vector2 Scale;

    private Vector4 _offset = (0, 0, 0, 0);
    private UIController _controller;

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIRectangleDisplayPrefab(Vector2 scale, Vector4 offset, UIController controller)
    {
        Position = new Vector2(offset.X, offset.Y);

        Scale = scale;
        _offset = offset;
        _controller = controller;

        UIMesh uiMesh = _controller.UiMesh;

        Collection = new UICollection("RectangleDisplayPrefabCollection", controller, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), Scale, _offset, 0);
        Background = new UIImage("RectangleDisplayPrefabBackground", controller, AnchorType.MiddleCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), Scale + (14, 14), (0, 0, 0, 0), 0, 11, (10, 0.05f), uiMesh);
        MoveButton = new UIButton("RectangleDisplayPrefabMoveButton", controller, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (Scale.X + 14, 14), (-6, -20, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable);
        ScaleButton = new UIButton("RectangleDisplayPrefabScaleButton", controller, AnchorType.BottomRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (20, 20), (6, 6, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable)
        {
            Depth = Depth + 10f
        };

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        ScaleButton.SetOnClick(SetOldMousePosition);
        ScaleButton.SetOnHold(ScaleNode);

        Collection.AddElements(Background, MoveButton, ScaleButton);
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

    public override UIElement[] GetMainElements()
    {
        return [Collection];
    }

    public override void SetVisibility(bool visible)
    {
        Collection.SetVisibility(visible);

        UIMesh uiMesh = _controller.UiMesh;
        TextMesh textMesh = _controller.textMesh;

        uiMesh.UpdateVisibility();
        textMesh.UpdateVisibility();
    }

    public override void Clear()
    {
        Collection.Clear();  
    }

    private void SetOldMousePosition() => _oldMouseButtonPosition = Input.GetMousePosition();

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