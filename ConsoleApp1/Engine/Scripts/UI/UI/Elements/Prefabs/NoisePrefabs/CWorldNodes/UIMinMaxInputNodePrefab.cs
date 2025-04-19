using OpenTK.Mathematics;

public class UIMinMaxInputNodePrefab : UINoiseNodePrefab
{
    public UICollection Collection;
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton InputButton;
    public UIButton OutputButton;
    public UIText NameField;

    public UIText MinTextField;
    public UIText MaxTextField;

    public UIInputField MinInputField;
    public UIInputField MaxInputField;

    private PositionType _positionType = PositionType.Absolute;
    private Vector3 _buttonColor = (0.6f, 0.6f, 0.6f);
    private Vector3 _backgroundColor = (0.5f, 0.5f, 0.5f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private Vector4 _offset = (100, 100, 0, 0);
    private float _rotation = 0;
    private UIController _controller;
    public MinMaxInputOperationType Type = MinMaxInputOperationType.Clamp;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UIMinMaxInputNodePrefab(
        string name, 
        UIController controller,
        Vector4 offset,
        MinMaxInputOperationType type
    ){
        Name = "display";
        _scale = (300, 120);
        _controller = controller;
        _offset = offset;
        Type = type;

        UIMesh uiMesh = _controller.UiMesh;
        TextMesh textMesh = _controller.textMesh;

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);
        
        string displayName = type.ToString();
        NameField = new UIText($"{name}{displayName}", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0, textMesh);
        NameField.SetMaxCharCount(displayName.Length).SetText(displayName, 0.5f);
        
        InputButton = new UIButton($"{name}InputButton", controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), uiMesh, UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), uiMesh, UIState.Interactable);

        MinInputField = new UIInputField($"{name}MinInputField", controller, AnchorType.BottomRight, PositionType.Relative, (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f), textMesh);
        MinInputField.SetMaxCharCount(10).SetTextType(TextType.Decimal).SetText("0.0", 0.5f);

        MaxInputField = new UIInputField($"{name}MaxInputField", controller, AnchorType.BottomRight, PositionType.Relative, (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 11, (10f, 0.05f), textMesh);
        MaxInputField.SetMaxCharCount(10).SetTextType(TextType.Decimal).SetText("1.0", 0.5f);

        MinTextField = new UIText($"{name}MinTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (0, 0, 0), (20, 20), (6, -36, 0, 0), 0, textMesh);
        MinTextField.SetMaxCharCount(3).SetText("Min", 0.5f).SetTextType(TextType.Alphabetic);

        MaxTextField = new UIText($"{name}MaxTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0, textMesh);
        MaxTextField.SetMaxCharCount(3).SetText("Max", 0.5f).SetTextType(TextType.Alphabetic);

        UIImage minBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), MinInputField.newScale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f), uiMesh);
        UIImage maxBackground = new UIImage($"{name}MaxBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), MaxInputField.newScale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f), uiMesh);

        ElementCollection.AddElements(NameField, InputButton, OutputButton, MinTextField, MinInputField, minBackground, MaxTextField, MaxInputField, maxBackground);

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, _positionType, _pivot, _scale + (0, 14), _offset, _rotation);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), uiMesh, UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), _scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f), uiMesh);

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

    
    public override void Destroy()
    {
        _controller.RemoveElements(GetMainElements());
    }

    private void SetOldMousePosition() => _oldMouseButtonPosition = Input.GetMousePosition();

    private void MoveNode()
    {
        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = (mousePosition - _oldMouseButtonPosition) * (1 / Collection.UIController.Scale);

        Collection.SetOffset(Collection.Offset + new Vector4(delta.X, delta.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();

        AddedMoveAction.Invoke();
    }
}