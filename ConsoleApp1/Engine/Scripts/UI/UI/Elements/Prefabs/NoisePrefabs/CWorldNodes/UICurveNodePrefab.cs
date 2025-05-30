using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UICurveNodePrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
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
    public Vector4 ButtonColor = CURVE_NODE_COLOR;
    public Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public float Rotation = 0;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public CurveWindow CurveWindow;
    public static List<CurveWindow> CurveWindows = new List<CurveWindow>();

    public UICurveNodePrefab(string name, UIController controller, Vector4 offset) : base(name, controller, offset)
    {
        Scale = (300, 120);
        CurveWindow = new CurveWindow(controller, offset.Xy + (7, 141), (0, 134, 0, 0), (300, 300));
        CurveWindows.Add(CurveWindow);

        ElementCollection = new UICollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 17, 0, 0), 0);

        NameField = new UIText($"{name}Curve", controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (Scale.X - 14, 20), (6, 6, 0, 0), 0);
        NameField.SetTextCharCount("Curve", 1.2f);

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
        SelectionImage = new UIImage($"{name}SelectionImage", controller, AnchorType.TopLeft, PositionType.Relative, SELECTION_COLOR, (0, 0, 0), Scale + (10, 324), (-5, -5, 0, 0), 0, 2, (10f, 0.05f));
        UICollection mainElements = new UICollection($"{name}MainElements", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), Scale, (0, 0, 0, 0), 0);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, ButtonColor, (0, 0, 0), (Scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, BackgroundColor, (0, 0, 0), Scale, (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        mainElements.AddElements(MoveButton, Background, ElementCollection, CurveWindow.Collection);
        Collection.AddElements(SelectionImage, mainElements);

        SelectionImage.SetVisibility(false);

        Controller.AddElements(this);
    }

    public override void MoveNode()
    {
        base.MoveNode();
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta == Vector2.Zero)
            return;

        CurveWindow.Position += mouseDelta * (1 / Collection.UIController.Scale);
    }

    public static void Update()
    {
        foreach (var curveWindow in CurveWindows)
        {
            curveWindow.Update();
        }
    }

    public static void Render(Matrix4 model, Matrix4 projection)
    {
        foreach (var curveWindow in CurveWindows)
        {
            curveWindow.Render(model, projection);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        CurveWindow.Destroy();
        CurveWindows.Remove(CurveWindow);
    }
    
    public override bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var node = new CurveConnectorNode(this);
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector);
        outputGates.Add(node.OutputGateConnector);
        connectorNode = node;
        return true;
    }
}