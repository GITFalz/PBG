using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingSelectionMode : ModelingBase
{   
    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public UIInputField CameraSpeedField;
    public static UIScrollView HierarchyScrollView;

    public UIController ModelingUi;

    public ModelingSelectionMode(ModelingEditor editor) : base(editor) 
    {
        ModelingUi = new UIController();

        UICollection mainPanelCollection = new("MainPanelCollection", ModelingUi, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f));

        UIVerticalCollection mainPanelStacking = new("MainPanelStacking", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (245, 0), (0, 0, 0, 0), (5, 10, 5, 5), 5, 0);



        // Main panel collection
        UICollection cullingCollection = new("CullingCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        BackfaceCullingText = new("CullingText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        BackfaceCullingText.SetText("cull: " + ModelSettings.BackfaceCulling, 1.2f);

        UIButton cullingButton = new("CullingButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        cullingButton.SetOnClick(BackFaceCullingSwitch); 

        cullingCollection.AddElements(BackfaceCullingText, cullingButton);


        // Alpha panel collection
        UICollection alphaCollection = new("AlphaCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        MeshAlphaText = new("AlphaText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2"), 1.2f);

        UIButton alphaButton = new("AlphaUpButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        alphaButton.SetOnClick(() => { Editor.blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { Editor.blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);


        // Axis panel collection
        UICollection axisStackingCollection = new("AxisStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UICollection axisStacking = new UICollection("AxisStacking", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        AxisText = new UIText("AxisText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 80, 0), (400, 20), (0, 0, 0, 0), 0);
        AxisText.SetMaxCharCount(9).SetText("axis: " + (ModelSettings.Axis.X == 1 ? "X" : "-") + (ModelSettings.Axis.Y == 1 ? "Y" : "-") + (ModelSettings.Axis.Z == 1 ? "Z" : "-"), 1.2f);
        axisStacking.SetScale((AxisText.newScale.X + 3, 20f));

        UIHorizontalCollection axisButtonStacking = new("AxisButtonStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (20, 20), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UIButton axisZButton = new("AxisZButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisZButton.SetOnClick(() => SwitchAxis("Z"));

        UIButton axisYButton = new("AxisYButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisYButton.SetOnClick(() => SwitchAxis("Y"));

        UIButton axisXButton = new("AxisXButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisXButton.SetOnClick(() => SwitchAxis("X"));

        axisButtonStacking.AddElements(axisXButton, axisYButton, axisZButton);

        axisStacking.AddElements(AxisText, axisButtonStacking);

        axisStackingCollection.AddElements(axisStacking);



        // Grid panel collection
        UICollection gridStackingCollection = new("GridStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText GridAlignedText = new("GridAlignedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 100, 0), (400, 20), (0, 0, 0, 0), 0);
        GridAlignedText.MaxCharCount = 11;
        GridAlignedText.SetText("grid: " + ModelSettings.GridAligned, 1.2f);

        UIButton gridAlignedButton = new("GridAlignedButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        gridAlignedButton.SetOnClick(() => {
            ModelSettings.GridAligned = !ModelSettings.GridAligned; 
            GridAlignedText.SetText("grid: " + ModelSettings.GridAligned).UpdateCharacters();
        });

        gridStackingCollection.AddElements(GridAlignedText, gridAlignedButton);



        // Texture Apply panel collection
        UIVerticalCollection textureApplyStackingCollection = new("TextureApplyStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 30), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection textureFileNameStacking = new UICollection("TextureFileNameStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 30), (0, 0, 0, 0), 0);

        UIImage textureFileNamePanel = new("TextureFileNameTextLabelPanel", ModelingUi, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));

        UIInputField textureFileNameField = new("TextureFileNameText", ModelingUi, AnchorType.MiddleCenter, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));

        textureFileNameField.SetMaxCharCount(24).SetText("cube", 0.92f).SetTextType(TextType.Alphanumeric);
        textureFileNameField.OnTextChange = new SerializableEvent(() => {  });

        textureFileNamePanel.SetScale((textureFileNameField.newScale.X + 20, 30f));

        textureFileNameStacking.AddElements(textureFileNamePanel, textureFileNameField);

        UIHorizontalCollection textureApplyButtonStacking = new("TextureApplyButtonStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 30), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UITextButton textureApplyButton = new("TextureApplyButton", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (110, 30), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        textureApplyButton.SetTextCharCount("Apply", 1.2f);
        textureApplyButton.SetOnClick(() => {
            string fileName = textureFileNameField.Text;
            if (fileName.Length == 0 || ModelManager.SelectedModel == null)
                return;
            
            fileName += ".png";
            string filePath = Path.Combine(Game.customTexturesPath, fileName);
            if (!File.Exists(filePath))
                ModelManager.SelectedModel.Renew("empty.png", TextureLocation.NormalTexture);
            else
                ModelManager.SelectedModel.Renew(fileName, TextureLocation.CustumTexture);
        });

        UITextButton textureReloadButton = new("TextureReloadButton", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (1, 1, 1), (0, 0, 0), (110, 30), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        textureReloadButton.SetTextCharCount("Reload", 1.2f);
        textureReloadButton.SetOnClick(() => {
            ModelManager.SelectedModel?.Reload();
        });

        textureApplyButtonStacking.AddElements(textureApplyButton, textureReloadButton);

        textureApplyStackingCollection.AddElements(textureFileNameStacking, textureApplyButtonStacking);



        // Hierarchy panel collection

        //HierarchyScrollView = hierarchyScrollView.ScrollView;
        
        

        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);
        
        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        
        CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));
        
        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; CameraSpeedField.SetText("1").UpdateCharacters(); } }); 

        speedStacking.SetScale((45, 30f));
        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);

        mainPanelStacking.AddElements(cullingCollection, alphaCollection, axisStackingCollection, gridStackingCollection, textureApplyStackingCollection);

        mainPanelCollection.AddElements(mainPanel, mainPanelStacking, cameraSpeedStacking);


        // Add elements to ui
        ModelingUi.AddElement(mainPanelCollection);
    }

    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        ModelSettings.MeshAlpha += mouseX * GameTime.DeltaTime;
        ModelSettings.MeshAlpha = Mathf.Clamp(0, 1, ModelSettings.MeshAlpha);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2")).UpdateCharacters();
    }

    public void BackFaceCullingSwitch()
    {
        ModelSettings.BackfaceCulling = !ModelSettings.BackfaceCulling;
        BackfaceCullingText.SetText("cull: " + ModelSettings.BackfaceCulling).UpdateCharacters();
    }

    public void UpdateAxisText()
    {
        string text = "";
        text += ModelSettings.Axis.X == 1 ? "X" : "-";
        text += ModelSettings.Axis.Y == 1 ? "Y" : "-";
        text += ModelSettings.Axis.Z == 1 ? "Z" : "-";
        
        AxisText.SetText("axis: " + text).UpdateCharacters();
    }

    public void SwitchAxis(string axis)
    {
        switch (axis)
        {
            case "X":
                ModelSettings.Axis.X = ModelSettings.Axis.X == 0 ? 1 : 0;
                break;
            case "Y":
                ModelSettings.Axis.Y = ModelSettings.Axis.Y == 0 ? 1 : 0;
                break;
            case "Z":
                ModelSettings.Axis.Z = ModelSettings.Axis.Z == 0 ? 1 : 0;
                break;
        }
        
        UpdateAxisText();
    }

    public override void Start()
    {
        ModelSettings.WireframeVisible = false;
        CameraSpeedField.SetText($"{Game.camera.SPEED}").UpdateCharacters();
    }

    public override void Resize()
    {
        ModelingUi.Resize();
    }

    public override void Update()
    {
        ModelingUi.Update();

        if (Model == null)
            return;

        if (Input.IsKeyPressed(Keys.Delete))
        {
            Model.Delete();
        }
    }

    public override void Render()
    {
        ModelingUi.RenderDepthTest();
    }

    public override void Exit()
    {
        
    }
}