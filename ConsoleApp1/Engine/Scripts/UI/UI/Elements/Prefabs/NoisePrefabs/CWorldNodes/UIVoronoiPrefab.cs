using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class UIVoronoiPrefab : UINoiseNodePrefab
{
    public UIImage SelectionImage;
    public UIButton MoveButton;
    public UIImage Background;

    public UIVerticalCollection ElementCollection;
    public UIButton InputButton1;
    public UIButton InputButton2;
    public UIButton OutputButton;
    public UIButton OutputCellXButton;
    public UIButton OutputCellYButton;
    public UIText NameField;

    public UIInputField ScaleInputField;
    public UIInputField OffsetXInputField;
    public UIInputField OffsetYInputField;
    
    public UIText OutputTextField;
    public UIText CellXTextField;
    public UIText CellYTextField;

    public UICollection RegionCollection;
    public UIInputField ValueInputField;
    public UIInputField ThresholdInputField;
    public UIText ValueTextField;
    public UIText ThresholdTextField;

    public Action? RegionToggleAction = null;

    public UIText ScaleTextField;
    public UIText OffsetXTextField;
    public UIText OffsetYTextField;

    public PositionType PositionType = PositionType.Absolute;
    public Vector4 ButtonColor = VORONOI_NODE_COLOR;
    public Vector4 BackgroundColor = (0.5f, 0.5f, 0.5f, 1f);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public float Rotation = 0;

    public float Depth
    {
        get => Collection.Depth;
        set => Collection.Depth = value;
    }

    public bool Region = false;

    public VoronoiOperationType Type;

    public UIVoronoiPrefab(string name, UIController controller, Vector4 offset, VoronoiOperationType type, bool region = false) : base(name, controller, offset)
    {
        Type = type;
        Scale = (300, 210);
        Region = region;
        if (Region)
        {
            Scale.Y += 60;
        }

        ElementCollection = new UIVerticalCollection($"{name}ElementCollection", controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), Scale - (6, 17), (0, 0, 0, 0), (0, 10, 0, 5), 5, 0);

        // Name elements
        UICollection nameCollection = new UICollection($"{name}NameCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        NameField = new UIText($"{name}Text", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (Scale.X - 14, 20), (5, 6, 0, 0), 0);
        NameField.SetTextCharCount(type.ToString() + " Voronoi", 1.2f).SetTextType(TextType.Alphanumeric);

        nameCollection.AddElements(NameField);

        // Input elements
        UICollection outputCollection = new UICollection($"{name}OutputCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        OutputButton = new UIButton($"{name}OutputButton", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        OutputTextField = new UIText($"{name}ValueTextField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-30, 0, 0, 0), 0);
        OutputTextField.SetTextCharCount("Value", 1.2f).SetTextType(TextType.Alphabetic);

        outputCollection.AddElements(OutputTextField, OutputButton);

        // Cell X elements
        UICollection cellXCollection = new UICollection($"{name}CellXCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);
        
        OutputCellXButton = new UIButton($"{name}OutputCellXButton", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        CellXTextField = new UIText($"{name}CellXTextField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-30, 0, 0, 0), 0);
        CellXTextField.SetTextCharCount("Cell X", 1.2f).SetTextType(TextType.Alphabetic);

        cellXCollection.AddElements(CellXTextField, OutputCellXButton);

        // Cell Y elements
        UICollection cellYCollection = new UICollection($"{name}CellYCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        OutputCellYButton = new UIButton($"{name}OutputCellYButton", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        CellYTextField = new UIText($"{name}CellYTextField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-30, 0, 0, 0), 0);
        CellYTextField.SetTextCharCount("Cell Y", 1.2f).SetTextType(TextType.Alphabetic);

        cellYCollection.AddElements(CellYTextField, OutputCellYButton);


        // Region elements
        RegionCollection = new UIVerticalCollection($"{name}RegionCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 60), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);
        RegionCollection.IgnoreInvisibleElements = true;

        // Region toggle button
        UICollection regionToggleCollection = new UICollection($"{name}RegionToggleCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (282, 20), (6, 0, 0, 0), 0);

        UITextButton regionSectionToggle = new UITextButton($"{name}RegionToggle", controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (217, 20), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        regionSectionToggle.SetTextCharCount("Region", 1f).SetTextType(TextType.Alphabetic);
        regionSectionToggle.Collection.AddedOffset = (6, 0, 0, 0);

        UITextButton regionToggle = new UITextButton($"{name}RegionToggle", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (60, 20), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        regionToggle.SetMaxCharCount(5).SetText("False", 1f).SetTextType(TextType.Alphabetic);
        regionToggle.Collection.AddedOffset = (6, 0, 0, 0);

        regionToggleCollection.AddElements(regionSectionToggle, regionToggle);


        // Value elements
        UICollection valueCollection = new UICollection($"{name}ValueCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        ValueInputField = new UIInputField($"{name}ValueInputField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-6, 0, 0, 0), 0, 11, (10f, 0.05f));
        ValueInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        ValueTextField = new UIText($"{name}ValueTextField", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, 0, 0, 0), 0);
        ValueTextField.SetTextCharCount("Value", 1.2f).SetTextType(TextType.Alphabetic);

        UIImage valueBackground = new UIImage($"{name}ValueBackground", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ValueInputField.Scale + (16, 16), (0, 0, 0, 0), 0, 11, (10f, 0.05f));

        valueCollection.AddElements(ValueTextField, ValueInputField, valueBackground);

        // Threshold elements
        UICollection thresholdCollection = new UICollection($"{name}ThresholdCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        ThresholdInputField = new UIInputField($"{name}ThresholdInputField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-6, 0, 0, 0), 0, 11, (10f, 0.05f));
        ThresholdInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        ThresholdTextField = new UIText($"{name}ThresholdTextField", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, 0, 0, 0), 0);
        ThresholdTextField.SetTextCharCount("Threshold", 1.2f).SetTextType(TextType.Alphabetic);

        UIImage thresholdBackground = new UIImage($"{name}ThresholdBackground", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ThresholdInputField.Scale + (16, 16), (0, 0, 0, 0), 0, 11, (10f, 0.05f));

        thresholdCollection.AddElements(ThresholdTextField, ThresholdInputField, thresholdBackground);

        RegionCollection.AddElements(regionToggleCollection, valueCollection, thresholdCollection);


        // Main elements
        UIVerticalCollection mainCollection = new UIVerticalCollection($"{name}MainCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), Scale, (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        // Main name
        UICollection mainNameCollection = new UICollection($"{name}MainNameCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        UIText mainText = new UIText($"{name}RegionToggle", controller, AnchorType.MiddleCenter, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (282, 20), (0, 0, 0, 0), 0);
        mainText.SetTextCharCount("Main", 1f).SetTextType(TextType.Alphabetic);

        UIImage mainBackground = new UIImage($"{name}MainBackground", controller, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (282, 20), (0, 0, 0, 0), 0, 10, (10f, 0.05f));

        mainNameCollection.AddElements(mainText, mainBackground);

        // Scale elements
        UICollection scaleCollection = new UICollection($"{name}ScaleCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        ScaleInputField = new UIInputField($"{name}ScaleInputField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-6, 0, 0, 0), 0, 11, (10f, 0.05f));
        ScaleInputField.SetMaxCharCount(10).SetText("1.0", 1.2f).SetTextType(TextType.Decimal);

        ScaleTextField = new UIText($"{name}ScaleTextField", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (6, 0, 0, 0), 0);
        ScaleTextField.SetTextCharCount("Scale", 1.2f).SetTextType(TextType.Alphanumeric);

        UIImage scaleBackground = new UIImage($"{name}ScaleBackground", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), ScaleInputField.Scale + (16, 16), (0, 0, 0, 0), 0, 11, (10f, 0.05f));

        scaleCollection.AddElements(ScaleTextField, ScaleInputField, scaleBackground);

        // Offset X elements
        UICollection offsetXCollection = new UICollection($"{name}OffsetXCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        InputButton1 = new UIButton($"{name}InputButton1", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        OffsetXTextField = new UIText($"{name}OffsetXTextField", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (30, 0, 0, 0), 0);
        OffsetXTextField.SetTextCharCount("Offset X", 1.2f).SetTextType(TextType.Alphanumeric);
        
        OffsetXInputField = new UIInputField($"{name}OffsetXInputField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-6, 0, 0, 0), 0, 11, (10f, 0.05f));
        OffsetXInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        UIImage offsetXBackground = new UIImage($"{name}OffsetXBackground", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetXInputField.Scale + (16, 16), (0, 0, 0, 0), 0, 11, (10f, 0.05f));

        offsetXCollection.AddElements(InputButton1, OffsetXTextField, OffsetXInputField, offsetXBackground);

        // Offset Y elements
        UICollection offsetYCollection = new UICollection($"{name}OffsetYCollection", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (294, 30), (6, 0, 0, 0), 0);

        InputButton2 = new UIButton($"{name}InputButton2", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (10f, 0.05f), UIState.Interactable);

        OffsetYTextField = new UIText($"{name}OffsetYTextField", controller, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (30, 0, 0, 0), 0);
        OffsetYTextField.SetTextCharCount("Offset Y", 1.2f).SetTextType(TextType.Alphanumeric);

        OffsetYInputField = new UIInputField($"{name}OffsetYInputField", controller, AnchorType.MiddleRight, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (20, 20), (-6, 0, 0, 0), 0, 11, (10f, 0.05f));
        OffsetYInputField.SetMaxCharCount(10).SetText("0.0", 1.2f).SetTextType(TextType.Decimal);

        UIImage offsetYBackground = new UIImage($"{name}OffsetYBackground", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), OffsetYInputField.Scale + (16, 16), (0, 0, 0, 0), 0, 11, (10f, 0.05f));

        offsetYCollection.AddElements(InputButton2, OffsetYTextField, OffsetYInputField, offsetYBackground);

        mainCollection.AddElements(mainNameCollection, scaleCollection, offsetXCollection, offsetYCollection);

        ElementCollection.AddElements(nameCollection, outputCollection, cellXCollection, cellYCollection, RegionCollection, mainCollection);

        float elementScaleY = ElementCollection.GetElementScaleY();

        Collection = new UICollection($"{name}Collection", controller, AnchorType.TopLeft, PositionType, Pivot, Scale + (0, 14), Offset, Rotation);
        SelectionImage = new UIImage($"{name}SelectionImage", controller, AnchorType.TopLeft, PositionType.Relative, SELECTION_COLOR, (0, 0, 0), (Scale.X + 10, elementScaleY + 10), (-5, -5, 0, 0), 0, 2, (10f, 0.05f));
        UICollection mainElements = new UICollection($"{name}MainElements", controller, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), Scale, (0, 0, 0, 0), 0);
        MoveButton = new UIButton($"{name}MoveButton", controller, AnchorType.TopLeft, PositionType.Relative, ButtonColor, (0, 0, 0), (Scale.X, 14), (0, 0, 0, 0), 0, 10, (5f, 0.025f), UIState.Interactable);
        Background = new UIImage($"{name}Background", controller, AnchorType.TopLeft, PositionType.Relative, BackgroundColor, (0, 0, 0), (Scale.X, elementScaleY - 14), (0, 14, 0, 0), 0, 10, (10f, 0.05f));

        regionSectionToggle.SetOnClick((Action)(() =>
        {
            valueCollection.SetVisibility(!valueCollection.Visible);
            thresholdCollection.SetVisibility(valueCollection.Visible);
            ElementCollection.ResetInit();
            RegionCollection.ResetInit();
            float elementScaleY = ElementCollection.GetElementScaleY();
            SelectionImage.SetScale((X:(float)SelectionImage.Scale.X, elementScaleY + 10));
            Background.SetScale((X:(float)Background.Scale.X, elementScaleY - 14));
            Collection.Align();
            Collection.UpdateTransformation();
            Collection.UpdateScale();
        }));

        regionToggle.SetOnClick(() =>
        {
            Region = !Region;
            regionToggle.SetText(Region ? "True" : "False", 1f).UpdateCharacters();
            RegionToggleAction?.Invoke();
            NoiseNodeManager.Compile();
        });

        MoveButton.SetOnClick(SetOldMousePosition);
        MoveButton.SetOnHold(MoveNode);

        mainElements.AddElements(MoveButton, Background, ElementCollection);
        Collection.AddElements(SelectionImage, mainElements);

        SelectionImage.SetVisibility(false);
        valueCollection.SetVisibility(Region);
        thresholdCollection.SetVisibility(Region);

        Controller.AddElements(this);
    }

    private void UpdateScale()
    {
        Collection.Align();
        Collection.UpdateScale();
    }
    
    public override bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode)
    {
        var node = new VoronoiConnectorNode(this, Type);
        connectorNode = node;
        noiseNodes.Add(this, node);
        inputGates.Add(node.InputGateConnector1);
        inputGates.Add(node.InputGateConnector2);
        outputGates.Add(node.Output);
        outputGates.Add(node.OutputCellXConnector);
        outputGates.Add(node.OutputCellYConnector);
        return true;
    }
}