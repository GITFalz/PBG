using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UIDoubleInputNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
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
    public OperationOperationType Type = OperationOperationType.Add;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIDoubleInputNodePrefab(
        string name,
        UIController controller,
        Vector4 offset,
        OperationOperationType type
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

        UIImage value1Background = new UIImage($"{name}value1Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), Value1InputField.Scale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        UIImage value2Background = new UIImage($"{name}value2Background", Controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), Value2InputField.Scale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, InputButton1, InputButton2, OutputButton, value1Background, value2Background, Value1InputField, Value2InputField, Value1TextField, Value2TextField);

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
        var node = new DoubleInputConnectorNode(this, Type);
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector1);
        inputGates.Add(node.InputGateConnector2);
        outputGates.Add(node.OutputGateConnector);
        connectorNode = node;
        return true;
    }
}