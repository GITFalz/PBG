using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RiggingEditor : BaseEditor
{
    public UIController ModelingUi;

    public UIInputField CameraSpeedField;

    public Camera Camera => Game.camera;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public static UIInputFieldPrefab BoneNameText;
    public static UIInputFieldPrefab RigNameField;

    public static UIInputField BoneXPositionField;
    public static UIInputField BoneYPositionField;
    public static UIInputField BoneZPositionField;

    public static UIInputField BoneXRotationField;
    public static UIInputField BoneYRotationField;
    public static UIInputField BoneZRotationField;

    
    public List<Vertex> SelectedVertices = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public string SelectedBoneName = "";
    public Bone? SelectedBone = null;

    public List<BonePivot> SelectedBonePivots = new();
    public List<Bone> SelectedBones = new();

    public bool renderSelection = false;
    public Vector2 oldMousePos = Vector2.Zero;

    // Input data
    private bool _d_pressed = false;
    private bool _started = false;
    private bool _modelSelected = true;


    public RiggingEditor(GeneralModelingEditor editor) : base(editor)
    {
        ModelingUi = new UIController();

        UICollection mainPanelCollection = new("MainPanelCollection", ModelingUi, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        mainPanel.SetBottomPc(50);

        UIScrollView mainPanelStacking = new("MainPanelStacking", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, CollectionType.Vertical, (245, 0), (0, 0, 0, 0));
        mainPanelStacking.SetBorder((0, 10, 5, 5));
        mainPanelStacking.SetSpacing(5);
        mainPanelStacking.SetTopPx(5);
        mainPanelStacking.SetBottomPc(50);
        mainPanelStacking.AddBottomPx(5);



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
        alphaButton.SetOnClick(() => { blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);


        // Bone info panel collection
        UIVerticalCollection boneInfoCollection = new("BoneInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 45), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection boneTextCollection = new("BoneTextCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText boneNameText = new("BoneNameText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneNameText.SetTextCharCount("Name:", 1.2f);

        UITextButton boneSetNameButton = new("BoneSetNameButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        boneSetNameButton.SetTextCharCount("Set", 1f);
        boneSetNameButton.Collection.SetScale((80, 20f));
        boneSetNameButton.SetOnClick(() =>
        {
            if (SelectedBones.Count == 0 || Model == null || Model.Rig == null)
                return;

            if (!Model.Rig.GetBone(SelectedBoneName, out Bone? bone))
            {
                PopUp.AddPopUp("Bone name does not exist");
                return;
            }

            string newName = BoneNameText.InputField.Text;
            bone.SetName(newName);
            SelectedBoneName = bone.Name;
            SelectedBone = bone;
            SetBonePositionText();
            BoneNameText.SetText(bone.Name, 1.2f).UpdateCharacters();
            Model.Rig.Create();

            PopUp.AddPopUp("Bone name changed successfully");
        });

        boneTextCollection.AddElements(boneNameText, boneSetNameButton.Collection);

        BoneNameText = new UIInputFieldPrefab("BoneNameText", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 21, "BoneName", TextType.Alphanumeric);
        BoneNameText.SetText("BoneName", 1f);
        BoneNameText.UpdateScaling();
        BoneNameText.Collection.SetAnchorType(AnchorType.TopLeft);


        UIVerticalCollection bonePositionCollection = new("BonePositionCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection bonePositionTextCollection = new("BonePositionTextCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText bonePositionText = new("BonePositionText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionText.SetTextCharCount("Position:", 1.2f);

        bonePositionTextCollection.AddElements(bonePositionText);

        UICollection bonePositionXCollection = new("BonePositionXCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText bonePositionXText = new("BonePositionXText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionXText.SetTextCharCount("X:", 1f);

        UIInputFieldPrefab bonePositionXField = new("BonePositionXField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        bonePositionXField.SetText("0", 1f);
        bonePositionXField.UpdateScaling();
        bonePositionXField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneXPositionField = bonePositionXField.InputField;
        BoneXPositionField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float x = Float.Parse(BoneXPositionField.Text);
            SelectedBone.Position = (x, SelectedBone.Position.Y, SelectedBone.Position.Z);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        bonePositionXCollection.AddElements(bonePositionXText, bonePositionXField.Collection);

        UICollection bonePositionYCollection = new("BonePositionYCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText bonePositionYText = new("BonePositionYText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionYText.SetTextCharCount("Y:", 1f);

        UIInputFieldPrefab bonePositionYField = new("BonePositionYField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        bonePositionYField.SetText("0", 1f);
        bonePositionYField.UpdateScaling();
        bonePositionYField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneYPositionField = bonePositionYField.InputField;
        BoneYPositionField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float y = Float.Parse(BoneYPositionField.Text);
            SelectedBone.Position = (SelectedBone.Position.X, y, SelectedBone.Position.Z);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        bonePositionYCollection.AddElements(bonePositionYText, bonePositionYField.Collection);

        UICollection bonePositionZCollection = new("BonePositionZCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText bonePositionZText = new("BonePositionZText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionZText.SetTextCharCount("Z:", 1f);

        UIInputFieldPrefab bonePositionZField = new("BonePositionZField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        bonePositionZField.SetText("0", 1f);
        bonePositionZField.UpdateScaling();
        bonePositionZField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneZPositionField = bonePositionZField.InputField;
        BoneZPositionField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float z = Float.Parse(BoneZPositionField.Text);
            SelectedBone.Position = (SelectedBone.Position.X, SelectedBone.Position.Y, z);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        bonePositionZCollection.AddElements(bonePositionZText, bonePositionZField.Collection);

        bonePositionCollection.AddElements(bonePositionTextCollection, bonePositionXCollection, bonePositionYCollection, bonePositionZCollection);


        UIVerticalCollection boneRotationCollection = new("BoneRotationCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection boneRotationTextCollection = new("BoneRotationTextCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText boneRotationText = new("BoneRotationText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationText.SetTextCharCount("Rotation:", 1.2f);

        boneRotationTextCollection.AddElements(boneRotationText);

        UICollection boneRotationXCollection = new("BoneRotationXCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText boneRotationXText = new("BoneRotationXText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationXText.SetTextCharCount("X:", 1f);

        UIInputFieldPrefab boneRotationXField = new("BoneRotationXField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        boneRotationXField.SetText("0", 1f);
        boneRotationXField.UpdateScaling();
        boneRotationXField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneXRotationField = boneRotationXField.InputField;
        BoneXRotationField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float x = MathHelper.DegreesToRadians(Float.Parse(BoneXRotationField.Text));
            Vector3 rotation = SelectedBone.Rotation.ToEulerAngles();
            rotation.X = x;
            SelectedBone.Rotation = Quaternion.FromEulerAngles(rotation);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        boneRotationXCollection.AddElements(boneRotationXText, boneRotationXField.Collection);

        UICollection boneRotationYCollection = new("BoneRotationYCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText boneRotationYText = new("BoneRotationYText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationYText.SetTextCharCount("Y:", 1f);

        UIInputFieldPrefab boneRotationYField = new("BoneRotationYField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        boneRotationYField.SetText("0", 1f);
        boneRotationYField.UpdateScaling();
        boneRotationYField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneYRotationField = boneRotationYField.InputField;
        BoneYRotationField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float y = MathHelper.DegreesToRadians(Float.Parse(BoneYRotationField.Text));
            Vector3 rotation = SelectedBone.Rotation.ToEulerAngles();
            rotation.Y = y;
            SelectedBone.Rotation = Quaternion.FromEulerAngles(rotation);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        boneRotationYCollection.AddElements(boneRotationYText, boneRotationYField.Collection);

        UICollection boneRotationZCollection = new("BoneRotationZCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText boneRotationZText = new("BoneRotationZText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationZText.SetTextCharCount("Z:", 1f);

        UIInputFieldPrefab boneRotationZField = new("BoneRotationZField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 19, "0", TextType.Decimal);
        boneRotationZField.SetText("0", 1f);
        boneRotationZField.UpdateScaling();
        boneRotationZField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneZRotationField = boneRotationZField.InputField;
        BoneZRotationField.SetOnTextChange(() =>
        {
            if (SelectedBone == null)
                return;

            float z = MathHelper.DegreesToRadians(Float.Parse(BoneZRotationField.Text));
            Vector3 rotation = SelectedBone.Rotation.ToEulerAngles();
            rotation.Z = z;
            SelectedBone.Rotation = Quaternion.FromEulerAngles(rotation);
            Model?.Rig?.RootBone.UpdateGlobalTransformation();
            Model?.Mesh.UpdateRig();
        });

        boneRotationZCollection.AddElements(boneRotationZText, boneRotationZField.Collection);

        boneRotationCollection.AddElements(boneRotationTextCollection, boneRotationXCollection, boneRotationYCollection, boneRotationZCollection);


        boneInfoCollection.AddElements(boneTextCollection, BoneNameText.Collection, bonePositionCollection, boneRotationCollection);


        // Rig info panel collection
        UIVerticalCollection rigInfoCollection = new("RigInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 70), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection rigNameCollection = new("RigNameCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText rigNameText = new("RigNameText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        rigNameText.SetTextCharCount("Rig Name:", 1.2f);

        rigNameCollection.AddElements(rigNameText);

        RigNameField = new("RigNameText", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 21, "RigName", TextType.Alphanumeric);
        RigNameField.SetText("RigName", 1f);
        RigNameField.UpdateScaling();

        UICollection rigButtonCollection = new("RigButtonCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UITextButton rigSaveButton = new("RigSaveButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 20, 0), (80, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        rigSaveButton.SetTextCharCount("Save", 1f);

        rigSaveButton.SetOnClick(() =>
        {
            if (Model == null || Model.Rig == null)
                return;

            string name = RigNameField.InputField.Text;
            if (string.IsNullOrEmpty(name))
            {
                PopUp.AddPopUp("Rig name cannot be empty");
                return;
            }

            Model.Rig.SetName(name);
            RigManager.Update(Model.Rig);
            RigManager.Save(name);
        });

        UITextButton rigLoadButton = new("RigLoadButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 20, 0), (80, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        rigLoadButton.SetTextCharCount("Load", 1f);

        rigLoadButton.SetOnClick(() =>
        {
            if (Model == null || Model.Rig == null)
                return;

            string name = RigNameField.InputField.Text;
            if (string.IsNullOrEmpty(name))
            {
                PopUp.AddPopUp("Rig name cannot be empty");
                return;
            }

            RigManager.Load(name);
            if (!RigManager.TryGet(name, out Rig? rig))
            {
                PopUp.AddPopUp("Rig name does not exist");
                return;
            }

            Model.Rig = rig;
            Model.Rig.Create();
            Model.Rig.Initialize();
            Model.InitRig();
            PopUp.AddPopUp("Rig loaded successfully");
        });

        rigButtonCollection.AddElements(rigSaveButton.Collection, rigLoadButton.Collection);

        rigInfoCollection.AddElements(rigNameCollection, RigNameField.Collection, rigButtonCollection);



        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

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

        mainPanelStacking.AddElements(cullingCollection, alphaCollection, boneInfoCollection, rigInfoCollection, cameraSpeedStacking);

        mainPanelCollection.AddElements(mainPanel, mainPanelStacking);

        // Add elements to ui
        ModelingUi.AddElement(mainPanelCollection);
    }

    public void SetBonePositionText()
    {
        if (SelectedBone == null)
            return;

        BoneXPositionField.SetText(SelectedBone.Position.X.ToString()).UpdateCharacters();
        BoneYPositionField.SetText(SelectedBone.Position.Y.ToString()).UpdateCharacters();
        BoneZPositionField.SetText(SelectedBone.Position.Z.ToString()).UpdateCharacters();
    }

    public void SetBoneRotationText()
    {
        if (SelectedBone == null)
            return;

        Vector3 rotation = SelectedBone.Rotation.ToEulerAngles();
        BoneXRotationField.SetText(MathHelper.RadiansToDegrees(rotation.X).ToString()).UpdateCharacters();
        BoneYRotationField.SetText(MathHelper.RadiansToDegrees(rotation.Y).ToString()).UpdateCharacters();
        BoneZRotationField.SetText(MathHelper.RadiansToDegrees(rotation.Z).ToString()).UpdateCharacters();
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

    public override void Start()
    {
        Started = true;

        Console.WriteLine("Start Rigging Editor");

        _started = true;
    }

    public override void Resize()
    {
        ModelingUi.Resize();
    }

    public override void Awake()
    {
        ModelSettings.WireframeVisible = true;
        CameraSpeedField.SetText($"{Game.camera.SPEED}").UpdateCharacters();

        foreach (var (_, model) in ModelManager.SelectedModels)
        {
            model.Model.SetStaticRig();
            model.Model.SetAnimation();
        }

        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = true;
        }

        if (Model == null)
            return;

        Model.RenderBones = true;
        Model.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
    }
    
    public override void Render()
    {
        Editor.RenderModel();

        ModelingUi.RenderNoDepthTest();

        if (renderSelection)
        {
            ModelingEditor.selectionShader.Bind();

            Matrix4 model = Matrix4.CreateTranslation((oldMousePos.X, oldMousePos.Y, 0));
            Matrix4 projection = UIController.OrthographicProjection;
            Vector2 selectionSize = Input.GetMousePosition() - oldMousePos;
            Vector3 color = new Vector3(1, 0.5f, 0.25f);

            var modelLoc = GL.GetUniformLocation(ModelingEditor.selectionShader.ID, "model");
            var projectionLoc = GL.GetUniformLocation(ModelingEditor.selectionShader.ID, "projection");
            var selectionSizeLoc = GL.GetUniformLocation(ModelingEditor.selectionShader.ID, "selectionSize");
            var colorLoc = GL.GetUniformLocation(ModelingEditor.selectionShader.ID, "color");

            GL.UniformMatrix4(modelLoc, true, ref model);
            GL.UniformMatrix4(projectionLoc, true, ref projection);
            GL.Uniform2(selectionSizeLoc, selectionSize);
            GL.Uniform3(colorLoc, color);

            ModelingEditor.selectionVao.Bind();

            GL.DrawArrays(PrimitiveType.Lines, 0, 8);

            ModelingEditor.selectionVao.Unbind();

            ModelingEditor.selectionShader.Unbind();
        }
    }

    public override void Update()
    {
        ModelingUi.Update();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            Editor.freeCamera = !Editor.freeCamera;

            if (Editor.freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.Unlock();
                renderSelection = false;
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
                Game.camera.Lock();
                Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
                Model?.UpdateVertexPosition();
            }
        }

        if (Input.IsMousePressed(MouseButton.Left) && !_modelSelected)
        {
            foreach (var (name, selected) in ModelManager.SelectedModels)
            {
                ModelManager.UnSelect(selected.Model);
                selected.Button.Color = (0.5f, 0.5f, 0.5f, 1f);
                selected.Button.UpdateColor();
            }
            ModelManager.SelectedModels = [];
        }

        if (!Editor.freeCamera)
        {
            MultiSelect();
        }

        RigUpdate();
    }

    private void RigUpdate()
    {
        if (Input.IsMousePressed(MouseButton.Left))
        {
            TestBonePosition();
            HandleVertexSelection();
        }

        TestAdd();

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            if (Input.IsKeyPressed(Keys.B))
            {
                BindSelectedVertices();
            }

            if (Input.IsKeyPressed(Keys.N)) Model?.GetConnectedVertices();
        }

        if (Input.IsKeyPressed(Keys.G))
        {
            Game.SetCursorState(CursorState.Grabbed);
        }

        if (Input.IsKeyPressed(Keys.R))
        {
            Game.SetCursorState(CursorState.Grabbed);
        }

        if (Input.IsKeyDown(Keys.G))
        {
            Handle_BoneMovement();
            SetBonePositionText();
            SetBoneRotationText();
        }

        if (Input.IsKeyDown(Keys.R))
        {
            Handle_BoneRotation();
            SetBoneRotationText();
        }

        if (Input.IsKeyReleased(Keys.G))
        {
            Game.SetCursorState(CursorState.Normal);
            Model?.UpdateBonePosition(Camera.ProjectionMatrix, Camera.ViewMatrix);
        }

        if (Input.IsKeyReleased(Keys.R))
        {
            Game.SetCursorState(CursorState.Normal);
            Model?.UpdateBonePosition(Camera.ProjectionMatrix, Camera.ViewMatrix);
        }
    }

    public override void Exit()
    {
        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = false;
        }

        Model?.SetStaticRig();
    }

    public void BindSelectedVertices()
    {
        if (Model == null || Model.Rig == null)
            return;

        var bones = GetSelectedBones();
        if (bones.Count == 0)
            return;

        var bone = bones.First();
        foreach (var vert in Model.SelectedVertices)
        {
            vert.Bone = bone;
            vert.BoneName = bone.Name;
        }

        Model.BindRig();
        Model.Mesh.UpdateModel();
    }

    public void MultiSelect()
    {
        if (Model == null)
            return;

        if (Input.IsMousePressed(MouseButton.Left))
        {
            oldMousePos = Input.GetMousePosition();
        }

        if (Input.IsMouseDown(MouseButton.Left) && !blocked)
        {
            renderSelection = true;
            
            Vector2 mousePos = Input.GetMousePosition();
            Vector2 max = Mathf.Max(mousePos, oldMousePos);
            Vector2 min = Mathf.Min(mousePos, oldMousePos); 
            float distance = Vector2.Distance(mousePos, oldMousePos);
            bool regenColor = false;

            if (distance < 5)
                return;

            foreach (var vert in Model.Vertices)
            {
                Vector2 vPos = vert.Value;
                if (vPos.X >= min.X && vPos.X <= max.X && vPos.Y >= min.Y && vPos.Y <= max.Y)
                {
                    if (!Model.SelectedVertices.Contains(vert.Key))
                    {
                        regenColor = true;
                        Model.SelectedVertices.Add(vert.Key);
                    }
                }
                else
                {
                    if (!Input.IsKeyDown(Keys.LeftShift) && Model.SelectedVertices.Contains(vert.Key))
                    {
                        regenColor = true;
                        Model.SelectedVertices.Remove(vert.Key);
                    }
                }
            }

            if (regenColor)
                Model.GenerateVertexColor();
        }

        if (Input.IsMouseReleased(MouseButton.Left))
        {
            renderSelection = false;
            oldMousePos = Vector2.Zero;
        }
    }

    public void HandleVertexSelection()
    {   
        if (Model == null)
            return;
            
        if (!Input.IsKeyDown(Keys.LeftShift))
            Model.SelectedVertices.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        Vertex? closestVert = null;
    
        foreach (var vert in Model.Vertices)
        {
            float distance = Vector2.Distance(mousePos, vert.Value);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = vert.Value;
                closestVert = vert.Key;
            }
        }

        if (closestVert != null && !Model.SelectedVertices.Remove(closestVert))
            Model.SelectedVertices.Add(closestVert);

        Model.GenerateVertexColor();
    }

    public void TestAdd()
    {
        if (Input.IsKeyDown(Keys.Q) && Input.IsKeyPressed(Keys.D))
        {
            if (!_d_pressed)
            {
                _d_pressed = true;
            }
            else
            {
                _d_pressed = false;
                Console.WriteLine("Added bone");
                AddBone();
            }
        }

        if (Input.IsKeyReleased(Keys.Q))
        {
            _d_pressed = false;
        }
    }

    public void AddBone()
    {
        if (Model == null || Model.Rig == null)
            return;

        var bones = GetSelectedBones();
        if (bones.Count == 0)
            return;

        var bone = bones.First();
        ChildBone child = new ChildBone("ChildBone" + Model.Rig.Bones.Count, bone);
        child.Position = new Vector3(0, 2, 0) * 0.1f;

        Model.Rig.RootBone.UpdateGlobalTransformation();
        Model.InitRig();
        Model.BindRig();
    }

    public void Handle_BoneMovement()
    {
        if (Model == null || Model.Rig == null)
            return;

        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta == Vector2.Zero)
            return;

        foreach (var bone in SelectedBones)
        {
            if (bone.Selection == BoneSelection.End)
            {
                bone.Rotate();
            }
            else if (bone.Selection == BoneSelection.Pivot || bone.Selection == BoneSelection.Both)
            {
                bone.Move();
            }
        }

        Model.Rig.RootBone.UpdateGlobalTransformation();
        Model.Mesh.UpdateRig();
        Model.Mesh.UpdateRigVertexPosition();

        foreach (var bone in SelectedBones)
        {
            bone.UpdateEndTarget();
        }
    }

    public void Handle_BoneRotation()
    {
        if (Model == null || Model.Rig == null)
            return;

        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta == Vector2.Zero)
            return;

        foreach (var bone in SelectedBones)
        {
            bone.Rotate(Camera.front, mouseDelta.X * GameTime.DeltaTime * 50f);
        }

        Model.Rig.RootBone.UpdateGlobalTransformation();
        Model.Mesh.UpdateRig();
        Model.Mesh.UpdateRigVertexPosition();

        foreach (var bone in SelectedBones)
        {
            bone.UpdateEndTarget();
        }
    }

    public HashSet<Bone> GetSelectedBones()
    {
        HashSet<Bone> selectedBones = new HashSet<Bone>();
        foreach (var pivot in SelectedBonePivots)
        {
            selectedBones.Add(pivot.Bone);
        }
        return selectedBones;
    }

    public void TestBonePosition()
    {
        if (Model == null)
            return;

        if (!Input.IsKeyDown(Keys.LeftShift))
        {
            SelectedBonePivots = [];
            SelectedBoneName = "";
            SelectedBone = null;
            BoneNameText.SetText("", 1f);
        }
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        BonePivot? closestBone = null;
    
        foreach (var (pivot, (position, radius)) in Model.BonePivots)
        {
            float distance = Vector2.Distance(mousePos, position);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
            
            if (distance < distanceClosest && distance < radius)
            {
                closest = position;
                closestBone = pivot;
            }
        }

        if (closestBone != null)
        {
            if (!SelectedBonePivots.Remove(closestBone))
            {
                SelectedBonePivots.Add(closestBone);
                if (!SelectedBones.Contains(closestBone.Bone))
                    SelectedBones.Add(closestBone.Bone);

                string name = closestBone.Bone.Name;
                SelectedBoneName = name;
                SelectedBone = closestBone.Bone;
                SetBonePositionText();
                SetBoneRotationText();
                BoneNameText.SetText(name, 1f);
            }
            else
            {
                SelectedBones.Remove(closestBone.Bone);
                if (SelectedBonePivots.Count > 0)
                {
                    string name = SelectedBones[0].Name;
                    SelectedBoneName = name;
                    SelectedBone = SelectedBones[0];
                    SetBonePositionText();
                    SetBoneRotationText();
                    BoneNameText.SetText(name, 1f);
                }
            }
        }

        if (SelectedBonePivots.Count == 0)
        {
            SelectedBoneName = "";
            SelectedBone = null;
            BoneNameText.SetText("", 1f);
        }

        BoneNameText.InputField.UpdateCharacters();

        UpdateBoneColors();
    }

    public void UpdateBoneColors()
    {
        if (Model == null)
            return;

        HashSet<Bone> seenBones = [];
        foreach (var (pivot, _) in Model.BonePivots)
        {
            Bone bone = pivot.Bone;
            if (SelectedBonePivots.Contains(pivot))
            {
                bool isPivot = pivot.IsPivot();
                if (isPivot)
                {
                    bone.Selection = BoneSelection.Pivot;
                }
                else if (seenBones.Contains(bone)) // Pivot is processed first, so we can check if the bone is already selected
                {
                    bone.Selection = BoneSelection.Both;
                }
                else
                {
                    bone.Selection = BoneSelection.End;
                }
                seenBones.Add(bone);
            }
            else if (!seenBones.Contains(bone))
            {
                bone.Selection = BoneSelection.None;
            }
        }

        Model.Mesh.UpdateRig();
    }

    public Vector3 GetMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        return move;
    }

    public static bool IsPointCloseToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float threshold)
    {
        float distance = DistancePointToLine(point, lineStart, lineEnd);
        return distance <= threshold;
    }
    
    private static float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        Vector2 pointVector = point - lineStart;
        float lineLengthSquared = line.LengthSquared;
        
        if (lineLengthSquared == 0f)
        {
            return Vector2.Distance(point, lineStart);
        }
        float t = Mathf.Clamp01(Vector2.Dot(pointVector, line) / lineLengthSquared);
        Vector2 projection = lineStart + t * line;
        return Vector2.Distance(point, projection);
    }
}