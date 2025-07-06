using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UIMinMaxInitMaskNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton ChildButton;
    public UIButton MaskButton;
    public UIButton OutputButton;
    public UIText NameField;

    public UIInputField MinInputField;
    public UIInputField MaxInputField;

    public UIText ChildTextField;
    public UIText MaskTextField;
    public UIText MinText;
    public UIText MaxText;

    public bool IsFlipped = false;

    private PositionType _positionType = PositionType.Absolute;
    private Vector4 _buttonColor = INIT_MASK_NODE_COLOR;
    private Vector4 _backgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private float _rotation = 0;
    public OperationOperationType Type = OperationOperationType.Add;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIMinMaxInitMaskNodePrefab(
        string name,
        UIController controller,
        Vector4 offset
    ) : base(name, controller, offset)
    {
        _scale = (300, 150);

        ElementCollection = new UICollection($"{name}ElementCollection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);

        NameField = new UIText($"{name}", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetTextCharCount("Init mask", 1.2f);

        ChildButton = new UIButton($"{name}ChildButton1", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        MaskButton = new UIButton($"{name}MaskButton2", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 52, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", Controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        MinInputField = new UIInputField($"{name}MinInputField", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 10, (10f, 0.05f));
        MinInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        MaxInputField = new UIInputField($"{name}MaxInputField", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 10, (10f, 0.05f));
        MaxInputField.SetMaxCharCount(10).SetText("1.0", 1.2f).SetTextType(TextType.Decimal);

        ChildTextField = new UIText($"{name}ChildTextField", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (36, 28, 0, 0), 0);
        ChildTextField.SetTextCharCount("Child", 1.2f).SetTextType(TextType.Alphanumeric);

        MaskTextField = new UIText($"{name}MaskTextField", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (36, 58, 0, 0), 0);
        MaskTextField.SetTextCharCount("Mask", 1.2f).SetTextType(TextType.Alphanumeric);

        MinText = new UIText($"{name}MinText", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -36, 0, 0), 0);
        MinText.SetTextCharCount("Min", 1.2f).SetTextType(TextType.Alphanumeric);

        MaxText = new UIText($"{name}MaxText", Controller, AnchorType.BottomLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (20, 20), (6, -6, 0, 0), 0);
        MaxText.SetTextCharCount("Max", 1.2f).SetTextType(TextType.Alphanumeric);


        UIImage minBackground = new UIImage($"{name}minBackground", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), MinInputField.Scale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));
        UIImage maxBackground = new UIImage($"{name}maxBackground", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), MaxInputField.Scale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, ChildButton, MaskButton, OutputButton, minBackground, maxBackground, MinInputField, MaxInputField, ChildTextField, MaskTextField, MinText, MaxText);

        Collection = new UICollection($"{name}Collection", Controller, AnchorType.TopLeft, _positionType, _pivot, _scale + (0, 14), Offset, _rotation);
        SelectionImage = new UIImage($"{name}SelectionImage", controller, AnchorType.TopLeft, PositionType.Relative, SELECTION_COLOR, (0, 0, 0), _scale + (10, 24), (-5, -5, 0, 0), 0, 2, (10f, 0.05f));
        UICollection mainElements = new UICollection($"{name}MainElements", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), _scale, (0, 0, 0, 0), 0);
        MoveButton = new UIButton($"{name}MoveButton", Controller, AnchorType.TopLeft, PositionType.Relative, _buttonColor, (0, 0, 0), (_scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", Controller, AnchorType.TopLeft, PositionType.Relative, _backgroundColor, (0, 0, 0), _scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        mainElements.AddElements(MoveButton, Background, ElementCollection);
        Collection.AddElements(SelectionImage, mainElements);

        SelectionImage.SetVisibility(false);

        Controller.AddElements(this);
    }
    
    public override bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var node = new InitMaskMinMaxConnectorNode(this);
        noiseNodes.Add(this, node);
        inputGates.Add(node.ChildGateConnector);
        inputGates.Add(node.MaskGateConnector);
        outputGates.Add(node.OutputGateConnector);
        connectorNode = node;
        return true;
    }
}