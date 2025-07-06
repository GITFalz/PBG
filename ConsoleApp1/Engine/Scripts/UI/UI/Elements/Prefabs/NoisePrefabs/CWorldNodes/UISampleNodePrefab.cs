using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UISampleNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton InputButton1;
    public UIButton InputButton2;
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
    public Vector4 ButtonColor = SAMPLE_NODE_COLOR;
    public Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public float Rotation = 0;

    public SampleOperationType Type;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UISampleNodePrefab(string name, UIController controller, Vector4 offset, SampleOperationType type) : base(name, controller, offset) 
    {
        Scale = (300, 150);
        Type = type;

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 17, 0, 0), 0);

        NameField = new UIText($"{name}Text", controller, AnchorType.TopLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (Scale.X - 14, 20), (5, 6, 0, 0), 0);
        NameField.SetTextCharCount(type.ToString() + " Sample", 1.2f).SetTextType(TextType.Alphanumeric);

        InputButton1 = new UIButton($"{name}InputButton1", Controller, AnchorType.BottomLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, -32, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        InputButton2 = new UIButton($"{name}InputButton2", Controller, AnchorType.BottomLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, -2, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);


        ScaleInputField = new UIInputField($"{name}ScaleInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -66, 0, 0), 0, 11, (10f, 0.05f));
        ScaleInputField.SetMaxCharCount(10).SetText("1.0", 1.2f).SetTextType(TextType.Decimal);

        OffsetXInputField = new UIInputField($"{name}OffsetXInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -36, 0, 0), 0, 11, (10f, 0.05f));
        OffsetXInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        OffsetYInputField = new UIInputField($"{name}OffsetYInputField", controller, AnchorType.BottomRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-8, -6, 0, 0), 0, 11, (10f, 0.05f));
        OffsetYInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        ScaleTextField = new UIText($"{name}ScaleTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, -66, 0, 0), 0);
        ScaleTextField.SetTextCharCount("Scale", 1.2f).SetTextType(TextType.Alphanumeric);

        OffsetXTextField = new UIText($"{name}OffsetXTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (30, -36, 0, 0), 0);
        OffsetXTextField.SetTextCharCount("Offset X", 1.2f).SetTextType(TextType.Alphanumeric);

        OffsetYTextField = new UIText($"{name}OffsetYTextField", controller, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (30, -6, 0, 0), 0);
        OffsetYTextField.SetTextCharCount("Offset Y", 1.2f).SetTextType(TextType.Alphanumeric);

        UIImage scaleBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ScaleInputField.Scale + (16, 16), (0, -58, 0, 0), 0, 11, (10f, 0.05f));
        UIImage offsetXBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetXInputField.Scale + (16, 16), (0, -28, 0, 0), 0, 11, (10f, 0.05f));
        UIImage offsetYBackground = new UIImage($"{name}MinBackground", controller, AnchorType.BottomRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetYInputField.Scale + (16, 16), (0, 2, 0, 0), 0, 11, (10f, 0.05f));

        ElementCollection.AddElements(NameField, InputButton1, InputButton2, OutputButton, scaleBackground, offsetXBackground, offsetYBackground, ScaleInputField, OffsetXInputField, OffsetYInputField, ScaleTextField, OffsetXTextField, OffsetYTextField);

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, PositionType, Pivot, Scale + (0, 14), Offset, Rotation);
        SelectionImage = new UIImage($"{name}SelectionImage", controller, AnchorType.TopLeft, PositionType.Relative, SELECTION_COLOR, (0, 0, 0), Scale + (10, 24), (-5, -5, 0, 0), 0, 2, (10f, 0.05f));
        UICollection mainElements = new UICollection($"{name}MainElements", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), Scale, (0, 0, 0, 0), 0);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, ButtonColor, (0, 0, 0), (Scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, BackgroundColor, (0, 0, 0), Scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        mainElements.AddElements(MoveButton, Background, ElementCollection);
        Collection.AddElements(SelectionImage, mainElements);

        SelectionImage.SetVisibility(false);

        Controller.AddElements(this);
    }
    
    public override bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var node = new SampleConnectorNode(this, Type);
        connectorNode = node;
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector1);
        inputGates.Add(node.InputGateConnector2);
        outputGates.Add(node.OutputGateConnector);
        return true;
    }
} 