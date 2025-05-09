using OpenTK.Mathematics;

public class UIDoubleInputNodePrefab : UINoiseNodePrefab
{
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton InputButton1;
    public UIButton InputButton2;
    public UIButton OutputButton;
    public UIText NameField;

    public UIInputField Value1InputField;
    public UIInputField Value2InputField;

    public UIText Value1TextField;
    public UIText Value2TextField;

    private PositionType _positionType = PositionType.Absolute;
    private Vector4 _buttonColor = DOUBLE_INPUT_COLOR;
    private Vector4 _backgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private float _rotation = 0;
    public DoubleInputOperationType Type = DoubleInputOperationType.Add;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UIDoubleInputNodePrefab(
        string name, 
        UIController controller,
        Vector4 offset,
        DoubleInputOperationType type
    ) : base(name, controller, offset)
    {
        _scale = (300, 150);
        Type = type;

        ElementCollection = new UICollection($"{name}ElementCollection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);
        
        string displayName = type.ToString();
        NameField = new UIText($"{name}{displayName}", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetMaxCharCount(displayName.Length).SetText(displayName, 1.2f);
        
        InputButton1 = new UIButton($"{name}InputButton1", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        InputButton2 = new UIButton($"{name}InputButton2", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 52, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", Controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        Value1InputField = new UIInputField($"{name}Value1InputField", Controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f));
        Value1InputField.SetMaxCharCount(10).SetText("1.0", 1.2f).SetTextType(TextType.Decimal);

        Value2InputField = new UIInputField($"{name}Value2InputField", Controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 11, (10f, 0.05f));
        Value2InputField.SetMaxCharCount(10).SetText("1.0", 1.2f).SetTextType(TextType.Decimal);

        Value1TextField = new UIText($"{name}Value1TextField", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -36, 0, 0), 0);
        Value1TextField.SetTextCharCount("Value 1", 1.2f).SetTextType(TextType.Alphanumeric);

        Value2TextField = new UIText($"{name}OffsetYTextField", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        Value2TextField.SetTextCharCount("Value 2", 1.2f).SetTextType(TextType.Alphanumeric);

        UIImage value1Background = new UIImage($"{name}value1Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), Value1InputField.newScale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        UIImage value2Background = new UIImage($"{name}value2Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), Value2InputField.newScale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, InputButton1, InputButton2, OutputButton, value1Background, value2Background, Value1InputField, Value2InputField, Value1TextField, Value2TextField);

        Collection = new UICollection($"{name}Collection", Controller, AnchorType.TopLeft, _positionType, _pivot, _scale + (0, 14), Offset, _rotation);
        MoveButton = new UIButton($"{name}MoveButton", Controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", Controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), _scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        Collection.AddElements(MoveButton, Background, ElementCollection);

        Controller.AddElements(this);
    }

    private void SetOldMousePosition() => _oldMouseButtonPosition = Input.GetMousePosition();

    private void MoveNode()
    {
        if (Input.GetMouseDelta() == Vector2.Zero)
            return;

        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = (mousePosition - _oldMouseButtonPosition) * (1 / Collection.UIController.Scale);

        Collection.SetOffset(Collection.Offset + new Vector4(delta.X, delta.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();

        AddedMoveAction.Invoke();
    }
}