using OpenTK.Mathematics;

public class UISampleNodePrefab : UINoiseNodePrefab
{
    public UICollection Collection;
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton OutputButton;
    public UIText NameField;

    public Vector3 OutputPosition => OutputButton.Center;

    private PositionType _positionType = PositionType.Absolute;
    private Vector3 _buttonColor = (0.6f, 0.6f, 0.6f);
    private Vector3 _backgroundColor = (0.5f, 0.5f, 0.5f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private Vector4 _offset = (100, 100, 0, 0);
    private float _rotation = 0;
    private UIController _controller;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UISampleNodePrefab(
        string name, 
        UIController controller,
        Vector4 offset
    ){
        Name = name;
        _scale = (300, 200);
        _controller = controller;
        _offset = offset;

        UIMesh uiMesh = _controller.UiMesh;
        TextMesh textMesh = _controller.textMesh;

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);
        
        NameField = new UIText($"{name}Text", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (_scale.X - 14, 20), (5, 6, 0, 0), 0, textMesh);
        NameField.SetMaxCharCount(6).SetText("Sample", 0.5f).SetTextType(TextType.Alphanumeric);
        
        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (20, 20), (0, 2, 0, 0), 0, 11, (10f, 0.05f), uiMesh, UIState.Interactable);
        
        ElementCollection.AddElements(NameField, OutputButton);

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, _positionType, _pivot, _scale, _offset, _rotation);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), uiMesh, UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), (_scale.X, 30), (0, 14, 0, 0), 0, 10, (10f, 0.05f), uiMesh);

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        Collection.AddElements(MoveButton, Background, ElementCollection);
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

        AddedMoveAction.Invoke();
    }
}