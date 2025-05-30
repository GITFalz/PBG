using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UIDisplayNodePrefab : UINoiseNodePrefab
{
    public UIButton MoveButton;
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton InputButton;
    public UIText NameField;

    private PositionType PositionType = PositionType.Absolute;
    private Vector4 ButtonColor = (0.6f, 0.6f, 0.6f, 1f);
    private Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    private Vector3 Pivot = (0, 0, 0);
    private Vector2 Scale = (100, 100);
    private float Rotation = 0;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public UIDisplayNodePrefab(string name, UIController controller, Vector4 offset) : base(name, controller, offset)
    {
        Scale = (300, 50);

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 17, 0, 0), 0);

        NameField = new UIText($"{name}Display", controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (Scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetMaxCharCount(7).SetText("Display", 1.2f);

        InputButton = new UIButton($"{name}InputButton", controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 22, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        ElementCollection.AddElements(NameField, InputButton);

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, PositionType, Pivot, Scale + (0, 14), Offset, Rotation);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, ButtonColor, (0, 0, 0), (Scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, BackgroundColor, (0, 0, 0), Scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        Collection.AddElements(MoveButton, Background, ElementCollection);

        Controller.AddElements(this);
    }

    public override bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var node = new DisplayConnectorNode(this);
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector);
        connectorNode = node;
        return true;
    }
}