using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UIBaseInputNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton InputButton;
    public UIButton OutputButton;
    public UIText NameField;

    private PositionType _positionType = PositionType.Absolute;
    private Vector4 _buttonColor = BASE_INPUT_COLOR;
    private Vector4 _backgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    private Vector3 _pivot = (0, 0, 0);
    private Vector2 _scale = (100, 100);
    private float _rotation = 0;
    public BaseInputOperationType Type = BaseInputOperationType.Invert;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIBaseInputNodePrefab(
        string name,
        UIController controller,
        Vector4 offset,
        BaseInputOperationType type
    ) : base(name, controller, offset)
    {
        _scale = (120, 50);
        Type = type;

        ElementCollection = new UICollection($"{name}ElementCollection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), _scale - (6, 17), (0, 17, 0, 0), 0);

        string displayName = type.ToString();
        NameField = new UIText($"{name}{displayName}", Controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (_scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetMaxCharCount(displayName.Length).SetText(displayName, 1.2f);

        InputButton = new UIButton($"{name}InputButton", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);
        OutputButton = new UIButton($"{name}OutputButton", Controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        ElementCollection.AddElements(NameField, InputButton, OutputButton);

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
        var node = new BaseInputConnectorNode(this, Type);
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector);
        outputGates.Add(node.OutputGateConnector);
        connectorNode = node;
        return true;
    }
}