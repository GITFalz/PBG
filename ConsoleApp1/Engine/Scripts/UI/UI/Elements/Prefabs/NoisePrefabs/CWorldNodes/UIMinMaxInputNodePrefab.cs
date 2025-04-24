using OpenTK.Mathematics;

public class UIMinMaxInputNodePrefab : UINoiseNodePrefab
{
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

    public PositionType PositionType = PositionType.Absolute;
    public Vector4 ButtonColor = (0.6f, 0.6f, 0.6f, 1f);
    public Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public float Rotation = 0;
    public MinMaxInputOperationType Type = MinMaxInputOperationType.Clamp;

    public float Depth {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UIMinMaxInputNodePrefab(string name, UIController controller,Vector4 offset,MinMaxInputOperationType type) : base(name, controller, offset)
    {
        Scale = (300, 120);
        Type = type;

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 17, 0, 0), 0);
        
        string displayName = type.ToString();
        NameField = new UIText($"{name}{displayName}", controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (Scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetMaxCharCount(displayName.Length).SetText(displayName, 1.2f);
        
        InputButton = new UIButton($"{name}InputButton", controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        MinInputField = new UIInputField($"{name}MinInputField", controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f));
        MinInputField.SetMaxCharCount(10).SetTextType(TextType.Decimal).SetText("0.0", 1.2f);

        MaxInputField = new UIInputField($"{name}MaxInputField", controller, AnchorType.BottomRight, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 11, (10f, 0.05f));
        MaxInputField.SetMaxCharCount(10).SetTextType(TextType.Decimal).SetText("1.0", 1.2f);

        MinTextField = new UIText($"{name}MinTextField", controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -36, 0, 0), 0);
        MinTextField.SetMaxCharCount(3).SetText("Min", 1.2f).SetTextType(TextType.Alphabetic);

        MaxTextField = new UIText($"{name}MaxTextField", controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        MaxTextField.SetMaxCharCount(3).SetText("Max", 1.2f).SetTextType(TextType.Alphabetic);

        UIImage minBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), MinInputField.newScale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        UIImage maxBackground = new UIImage($"{name}MaxBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), MaxInputField.newScale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, InputButton, OutputButton, minBackground, maxBackground, MinTextField, MinInputField, MaxTextField, MaxInputField);

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