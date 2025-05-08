using OpenTK.Mathematics;

public class UIRangeNodePrefab : UINoiseNodePrefab
{
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton StartButton;
    public UIButton HeightButton;
    public UIButton OutputButton;
    public UIText NameField;

    public UIInputField StartInputField;
    public UIInputField HeightInputField;
    public UIButton FlippedButton;

    public UIText StartTextField;
    public UIText HeightTextField;
    public UIText FlippedText;

    public bool IsFlipped = false;

    private PositionType _positionType = PositionType.Absolute;
    private Vector4 _buttonColor = (0.6f, 0.6f, 0.6f, 1f);
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

    public UIRangeNodePrefab(
        string name, 
        UIController controller,
        Vector4 offset
    ) : base(name, controller, offset)
    {
        _scale = (300, 175);

        ElementCollection = new UICollection($"{name}ElementCollection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);
        
        NameField = new UIText($"{name}", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetTextCharCount("Range", 1.2f);
        
        StartButton = new UIButton($"{name}StartButton1", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        HeightButton = new UIButton($"{name}HeightButton2", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 52, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", Controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        StartInputField = new UIInputField($"{name}StartInputField", Controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -66, 0, 0), 0, 11, (10f, 0.05f));
        StartInputField.SetMaxCharCount(10).SetText("20", 1.2f).SetTextType(TextType.Numeric);

        HeightInputField = new UIInputField($"{name}HeightInputField", Controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f));
        HeightInputField.SetMaxCharCount(10).SetText("20", 1.2f).SetTextType(TextType.Numeric);

        FlippedButton = new UIButton($"{name}FlippedTextButton", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (-2, 2, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        FlippedButton.SetOnClick(() => { IsFlipped = !IsFlipped; FlippedText.SetText($"Flip: {IsFlipped}", 1.2f).UpdateCharacters(); });

        StartTextField = new UIText($"{name}StartTextField", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -66, 0, 0), 0);
        StartTextField.SetTextCharCount("Start", 1.2f).SetTextType(TextType.Alphanumeric);

        HeightTextField = new UIText($"{name}HeightTextField", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -36, 0, 0), 0);
        HeightTextField.SetTextCharCount("Height", 1.2f).SetTextType(TextType.Alphanumeric);

        FlippedText = new UIText($"{name}FlippedText", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        FlippedText.SetTextCharCount("Flip: False", 1.2f).SetTextType(TextType.Alphabetic);


        UIImage startBackground = new UIImage($"{name}value1Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), StartInputField.newScale + (16, 16), (0, -58, 0, 0), 0, 11, (10f, 0.05f));
        UIImage heightBackground = new UIImage($"{name}value2Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), HeightInputField.newScale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        FlippedButton.SetScale((50, HeightInputField.newScale.Y + 16));

        ElementCollection.AddElements(NameField, StartButton, HeightButton, OutputButton, startBackground, heightBackground, StartInputField, HeightInputField, FlippedButton, StartTextField, HeightTextField, FlippedText);

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