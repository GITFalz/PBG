using OpenTK.Mathematics;

public class UIInitMaskNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
    public Action AddedMoveAction = () => { };
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton ChildButton;
    public UIButton MaskButton;
    public UIButton OutputButton;
    public UIText NameField;

    public UIInputField ThresholdInputField;

    public UIText ChildTextField;
    public UIText MaskTextField;
    public UIText ThresholdText;

    public bool IsFlipped = false;

    private PositionType _positionType = PositionType.Absolute;
    private Vector4 _buttonColor = INIT_MASK_NODE_COLOR;
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

    public UIInitMaskNodePrefab(
        string name, 
        UIController controller,
        Vector4 offset
    ) : base(name, controller, offset)
    {
        _scale = (300, 120);

        ElementCollection = new UICollection($"{name}ElementCollection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);
        
        NameField = new UIText($"{name}", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetTextCharCount("Init mask", 1.2f);
        
        ChildButton = new UIButton($"{name}ChildButton1", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        MaskButton = new UIButton($"{name}MaskButton2", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 52, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", Controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        ThresholdInputField = new UIInputField($"{name}ThresholdInputField", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 10, (10f, 0.05f));
        ThresholdInputField.SetMaxCharCount(10).SetText("0.5", 1.2f).SetTextType(TextType.Decimal);

        ChildTextField = new UIText($"{name}ChildTextField", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (36, 28, 0, 0), 0);
        ChildTextField.SetTextCharCount("Child", 1.2f).SetTextType(TextType.Alphanumeric);

        MaskTextField = new UIText($"{name}MaskTextField", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (36, 58, 0, 0), 0);
        MaskTextField.SetTextCharCount("Mask", 1.2f).SetTextType(TextType.Alphanumeric);

        ThresholdText = new UIText($"{name}FlippedText", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        ThresholdText.SetTextCharCount("Threshold", 1.2f).SetTextType(TextType.Alphanumeric);


        UIImage thresholdBackground = new UIImage($"{name}value2Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ThresholdInputField.newScale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, ChildButton, MaskButton, OutputButton, thresholdBackground, ThresholdInputField, ChildTextField, MaskTextField, ThresholdText);

        Collection = new UICollection($"{name}Collection", Controller, AnchorType.TopLeft, _positionType, _pivot, _scale + (0, 14), Offset, _rotation);
        SelectionImage = new UIImage($"{name}SelectionImage", controller, AnchorType.TopLeft, PositionType.Relative, SELECTION_COLOR, (0, 0, 0), _scale + (10, 24), (-5, -5, 0, 0), 0, 2, (10f, 0.05f));
        UICollection mainElements = new UICollection ($"{name}MainElements", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), _scale, (0, 0, 0, 0), 0);
        MoveButton = new UIButton($"{name}MoveButton", Controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", Controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), _scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        mainElements.AddElements(MoveButton, Background, ElementCollection);
        Collection.AddElements(SelectionImage, mainElements);

        SelectionImage.SetVisibility(false);

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