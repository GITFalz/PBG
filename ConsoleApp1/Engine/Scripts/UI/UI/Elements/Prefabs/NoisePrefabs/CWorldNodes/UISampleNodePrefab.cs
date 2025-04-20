using OpenTK.Mathematics;

public class UISampleNodePrefab : UINoiseNodePrefab
{
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton OutputButton;
    public UIText NameField;

    public UIInputField ScaleInputField;
    public UIInputField OffsetXInputField;
    public UIInputField OffsetYInputField;

    public UIText ScaleTextField;
    public UIText OffsetXTextField;
    public UIText OffsetYTextField;

    public Vector3 OutputPosition => OutputButton.Center;

    public PositionType PositionType = PositionType.Absolute;
    public Vector4 ButtonColor = (0.6f, 0.6f, 0.6f, 1f);
    public Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public float Rotation = 0;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UISampleNodePrefab(string name, UIController controller, Vector4 offset) : base(name, controller, offset)
    {
        Scale = (300, 150);

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 17, 0, 0), 0);
        
        NameField = new UIText($"{name}Text", controller, AnchorType.TopLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (Scale.X - 14, 20), (5, 6, 0, 0), 0);
        NameField.SetTextCharCount("Sample", 0.5f).SetTextType(TextType.Alphanumeric);
        
        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        

        ScaleInputField = new UIInputField($"{name}ScaleInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -66, 0, 0), 0, 11, (10f, 0.05f));
        ScaleInputField.SetMaxCharCount(10).SetText("1.0", 0.5f).SetTextType(TextType.Decimal);

        OffsetXInputField = new UIInputField($"{name}OffsetXInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f));
        OffsetXInputField.SetMaxCharCount(10).SetText("0.0", 0.5f).SetTextType(TextType.Decimal);

        OffsetYInputField = new UIInputField($"{name}OffsetYInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 11, (10f, 0.05f));
        OffsetYInputField.SetMaxCharCount(10).SetText("0.0", 0.5f).SetTextType(TextType.Decimal);

        ScaleTextField = new UIText($"{name}ScaleTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, -66, 0, 0), 0);
        ScaleTextField.SetTextCharCount("Scale", 0.5f).SetTextType(TextType.Alphanumeric);

        OffsetXTextField = new UIText($"{name}OffsetXTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, -36, 0, 0), 0);
        OffsetXTextField.SetTextCharCount("Offset X", 0.5f).SetTextType(TextType.Alphanumeric);

        OffsetYTextField = new UIText($"{name}OffsetYTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        OffsetYTextField.SetTextCharCount("Offset Y", 0.5f).SetTextType(TextType.Alphanumeric);

        UIImage scaleBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ScaleInputField.newScale + (16, 16), (0, -58, 0, 0), 0, 11, (10f, 0.05f));
        UIImage offsetXBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetXInputField.newScale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        UIImage offsetYBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetYInputField.newScale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, OutputButton, ScaleInputField, OffsetXInputField, OffsetYInputField, ScaleTextField, OffsetXTextField, OffsetYTextField, scaleBackground, offsetXBackground, offsetYBackground);

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, PositionType, Pivot, Scale + (0, 14), Offset, Rotation);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, ButtonColor, (0, 0, 0), (Scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, BackgroundColor, (0, 0, 0), Scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        Collection.AddElements(MoveButton, Background, ElementCollection);

        Controller.AddElements(this);
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