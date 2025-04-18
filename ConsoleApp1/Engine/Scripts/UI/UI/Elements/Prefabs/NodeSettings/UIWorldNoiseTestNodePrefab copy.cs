using OpenTK.Mathematics;

public class UIWorldNoiseTestNodePrefab : UIPrefab
{
    public UICollection Collection;
    public UIButton MoveButton;
    public UIImage Background;

    public UIVerticalCollection VerticalCollection;
    public UIVerticalCollection SubElementCollection1;
    public List<UIPrefab> SubElements1 = [];

    private string _name = "WorldNoiseTestNodePrefab";
    private AnchorType _anchorType = AnchorType.TopLeft;
    private PositionType _positionType = PositionType.Relative;
    private Vector3 _buttonColor = (0.6f, 0.6f, 0.6f);
    private Vector3 _backgroundColor = (0.5f, 0.5f, 0.5f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private Vector4 _offset = (0, 0, 0, 0);
    private float _rotation = 0;
    private int _buttonIndex = 0;
    private int _backgroundIndex = 0;
    private UIController _controller;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UIWorldNoiseTestNodePrefab(
        string name, 
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3? buttonColor,
        Vector3? backgroundColor,
        Vector3? pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float? rotation, 
        int? buttonIndex,
        int? backgroundIndex, 
        UIController controller
    ){
        _name = name;
        _anchorType = anchorType;
        _positionType = positionType;
        _buttonColor = buttonColor ?? (0.6f, 0.6f, 0.6f);
        _backgroundColor = backgroundColor ?? (0.5f, 0.5f, 0.5f);
        _pivot = pivot ?? (0, 0, 0);
        _scale = scale;
        _offset = offset;
        _rotation = rotation ?? 0;
        _buttonIndex = buttonIndex ?? 0;
        _backgroundIndex = backgroundIndex ?? 0;
        _controller = controller;

        UIMesh uiMesh = _controller.UiMesh;
        TextMesh textMesh = _controller.textMesh;

        VerticalCollection = new UIVerticalCollection($"{name}VerticalCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), (0, 0, 0, 0), 0, 0);

        UINodeOutElementPrefab testImage1 = new UINodeOutElementPrefab((_scale.X - 6, 40), (0, 0, 0, 0), _controller);
        UINodeOutElementPrefab testImage2 = new UINodeOutElementPrefab((_scale.X - 6, 40), (0, 0, 0, 0), _controller);

        SubElementCollection1 = new UIVerticalCollection($"{name}SubElements1", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), (0, 0, 0, 0), 0, 0);

        UINodeOutElementPrefab testImage3 = new UINodeOutElementPrefab((_scale.X - 6, 40), (0, 0, 0, 0), _controller);
        UINodeOutElementPrefab testImage4 = new UINodeOutElementPrefab((_scale.X - 6, 40), (0, 0, 0, 0), _controller);

        SubElementCollection1.AddElements(testImage3.GetMainElements(), testImage4.GetMainElements());

        VerticalCollection.AddElements(testImage1.GetMainElements(), [SubElementCollection1], testImage2.GetMainElements());

        float yScale = VerticalCollection.GetYScale();

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, _positionType, _pivot, _scale, _offset, _rotation);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, _buttonIndex, (5f, 0.025f), uiMesh, UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), (_scale.X, yScale + 6), (0, 14, 0, 0), 0, _backgroundIndex, (10f, 0.05f), uiMesh);

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        Collection.AddElements(MoveButton, Background, VerticalCollection);
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
        Vector2 delta = mousePosition - _oldMouseButtonPosition;

        Collection.SetOffset(Collection.Offset + new Vector4(delta.X, delta.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();
    }

    public UIWorldNoiseTestNodePrefab Copy()
    {
        return new UIWorldNoiseTestNodePrefab(
            $"{_name}Copy",
            _anchorType,
            _positionType,
            _buttonColor,
            _backgroundColor,
            _pivot,
            _scale,
            _offset,
            _rotation,
            _buttonIndex,
            _backgroundIndex,
            _controller
        );
    }
}