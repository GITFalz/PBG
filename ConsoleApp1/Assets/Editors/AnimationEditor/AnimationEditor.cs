using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    public Camera Camera => Game.camera;

    // === UI COMPONENTS ===
    public UIController ModelingUi;
    public UIController TimelineUI;
    public UIController KeyframeUI;
    public UIController TimerUI;

    public UIInputFieldPrefab TimelineTimeField;
    public UIScrollView TimelineScrollView;
    public UIInputField CameraSpeedField;

    public UICollection KeyframePanelCollection;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public static UIInputFieldPrefab BoneNameText;
    public static UIInputFieldPrefab AnimationNameField;

    public static UIInputField BoneXPositionField;
    public static UIInputField BoneYPositionField;
    public static UIInputField BoneZPositionField;

    public static UIInputField BoneXRotationField;
    public static UIInputField BoneYRotationField;
    public static UIInputField BoneZRotationField;

    // === DATA CONTAINERS ===
    public List<Vertex> SelectedVertices = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public string SelectedBoneName = "";
    public Bone? SelectedBone = null;

    public List<BonePivot> SelectedBonePivots = new();
    public List<Bone> SelectedBones = new();

    // === RENDERING / SHADER RESOURCES ===
    public static ShaderProgram tickShader = new ShaderProgram("Utils/Rectangles.vert", "Animation/Ticks.frag");
    public static VAO tickVao = new();

    public struct TickData
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Tilling;
    }

    public static SSBO<TickData> TickSSBO = new(new List<TickData>() {
        new TickData()
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(600, 20),
            Tilling = new Vector2(6, 1)
        },
        new TickData()
        {
            Position = new Vector2(600, 0),
            Size = new Vector2(600, 20),
            Tilling = new Vector2(6, 1)
        },
        new TickData()
        {
            Position = new Vector2(1200, 0),
            Size = new Vector2(600, 20),
            Tilling = new Vector2(6, 1)
        },
    });

    // === ANIMATION LOGIC ===
    public Animation Animation = new Animation("Test");
    public bool Playing = false;

    public int CurrentFrame
    {
        get => Int.Parse(TimelineTimeField.InputField.Text, 0);
        set => TimelineTimeField.InputField.SetText(value.ToString()).UpdateCharacters();
    }
    public Vector2 TimelinePosition = new Vector2(0, 0);

    // === PRIVATE / INTERNAL STATE ===
    private int _tickCount = 6 * 3;
    private bool regenerateVertexUi = true;

    private bool _d_pressed = false;

    // === KEYFRAME ELEMENTS ===
    public Dictionary<string, TimelineBoneAnimation> BoneAnimations = [];
    private Vector2 _oldMousePos = new Vector2(0, 0);
    private bool _moveTimeline = false;
    private int _timerIndex = 0;

    public AnimationEditor(GeneralModelingEditor editor) : base(editor)
    {
        ModelingUi = new UIController("ModelingUI");

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


        // Animation info panel collection
        UIVerticalCollection animationInfoCollection = new("AnimationInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 70), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection animationNameCollection = new("AnimationNameCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText animationNameText = new("AnimationNameText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        animationNameText.SetTextCharCount("Animation Name:", 1.2f);

        animationNameCollection.AddElements(animationNameText);

        AnimationNameField = new("AnimationNameText", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 21, "AnimationName", TextType.Alphanumeric);
        AnimationNameField.SetText("AnimationName", 1f);
        AnimationNameField.UpdateScaling();

        UICollection animationButtonCollection = new("AnimationButtonCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UITextButton animationSaveButton = new("AnimationSaveButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 20, 0), (80, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        animationSaveButton.SetTextCharCount("Save", 1f);

        animationSaveButton.SetOnClick(() =>
        {
            if (Model == null || Model.Animation == null)
                return;

            string name = AnimationNameField.InputField.Text;
            if (string.IsNullOrEmpty(name))
            {
                PopUp.AddPopUp("Animation name cannot be empty");
                return;
            }
            if (!AnimationManager.TryGet(Animation.Name, out Animation? animation))
            {
                AnimationManager.Add(Animation);
            }
            AnimationManager.ChangeName(Animation.Name, name);
            Animation.Name = name;

            AnimationManager.Save(name);
        });

        UITextButton animationLoadButton = new("AnimationLoadButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 20, 0), (80, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        animationLoadButton.SetTextCharCount("Load", 1f);

        animationLoadButton.SetOnClick(() =>
        {
            if (Model == null || Model.Animation == null)
                return;

            string name = AnimationNameField.InputField.Text;
            AnimationManager.Load(name);

            if (!AnimationManager.TryGet(name, out Animation? animation))
            {
                PopUp.AddPopUp("Animation not found");
                return;
            }

            Animation = animation;
            Model.Animation = animation;

            GenerateAnimationTimeline(Model, Animation);
        });

        animationButtonCollection.AddElements(animationSaveButton.Collection, animationLoadButton.Collection);

        animationInfoCollection.AddElements(animationNameCollection, AnimationNameField.Collection, animationButtonCollection);



        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));

        CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));

        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = Int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; CameraSpeedField.SetText("1").UpdateCharacters(); } });

        speedStacking.SetScale((45, 30f));
        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);

        mainPanelStacking.AddElements(cullingCollection, alphaCollection, boneInfoCollection, animationInfoCollection);

        mainPanelCollection.AddElements(mainPanel, mainPanelStacking, cameraSpeedStacking);


        // Add elements to ui
        ModelingUi.AddElement(mainPanelCollection);


        TimelineUI = new UIController("TimelineUI");

        UICollection timelinePanelCollection = new("TimelinePanelCollection", TimelineUI, AnchorType.ScaleBottom, PositionType.Absolute, (0, 0, 0), (0, 250), (5, -5, 255, 5), 0);

        UIImage timelineBackground = new("TimelineBackground", TimelineUI, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width, 250), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        UIButton timelineMoveButton = new("TimelineMoveButton", TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0, 0), (0, 0, 0), (Game.Width - 385, 30), (7, 0, 0, 0), 0, 0, (0, 0), UIState.InvisibleInteractable);
        timelineMoveButton.SetOnHold(() =>
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero || !_moveTimeline)
                return;

            TimelinePosition.X = Mathf.Clamp(-1000, 0, TimelinePosition.X + mouseDelta.X);
        });

        UIVerticalCollection timelineSettingsStacking = new("TimelineSettingsStacking2", TimelineUI, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (100, 250), (0, 0, 0, 0), (10, 10, 5, 5), 5, 0);

        TimelineTimeField = new("TimelineTimeField", TimelineUI, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 11, 4, "0", TextType.Numeric);

        timelineSettingsStacking.AddElements(TimelineTimeField.Collection);

        UICollection timelineScrollViewCollection = new("TimelineScrollViewCollection", TimelineUI, AnchorType.ScaleBottom, PositionType.Relative, (0, 0, 0), (Game.Width - 10, 250 - 30), (5, -5, 110, 5), 0);

        UIImage timelineScrollViewBackground = new("TimelineScrollViewBackground", TimelineUI, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width - 10, 250 - 10), (0, 0, 0, 0), 0, 11, (10, 0.05f));
        TimelineScrollView = new("TimelineScrollView", TimelineUI, AnchorType.ScaleFull, PositionType.Relative, CollectionType.Vertical, (Game.Width - 14, 250 - 14), (7, 7, 7, 7));

        timelineScrollViewCollection.AddElements(timelineScrollViewBackground, TimelineScrollView);

        timelinePanelCollection.AddElements(timelineBackground, timelineMoveButton, timelineSettingsStacking, timelineScrollViewCollection);

        TimelineUI.AddElement(timelinePanelCollection);
        TimelineUI.GenerateBuffers();


        KeyframeUI = new UIController("KeyframeUI");

        UICollection mainKeyframePanelCollection = new("MainKeyframePanelCollection", KeyframeUI, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        KeyframePanelCollection = new("KeyframePanelCollection", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        KeyframeUI.AddElement(KeyframePanelCollection);
        KeyframeUI.GenerateBuffers();



        TimerUI = new UIController("TimerCollection");

        UICollection timerPanelCollection = new("TimerPanelCollection", TimerUI, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (5, 2, 0, 0), 0);

        UIButton timelineIndexButton = new("TimelineIndexButton", TimerUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (12, 20), (0, 0, 0, 0), 0, 10, (5, 0.05f), UIState.Interactable) { Depth = 3f };
        timelineIndexButton.SetOnClick(() => { _oldMousePos = Input.GetMousePosition(); _moveTimeline = false; });
        timelineIndexButton.SetOnHold(() =>
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero)
                return;

            Vector2 mousePosition = Input.GetMousePosition() + (10 * Mathf.Sign(mouseDelta.X), 0);

            int sign = Mathf.Sign(mouseDelta.X);
            float delta = Mathf.Step(20, mousePosition.X - _oldMousePos.X);

            if (delta != 0)
            {
                if (timelineIndexButton.Offset.X + delta < 0)
                    return;

                timelineIndexButton.Offset.X += delta;
                _timerIndex = Mathf.FloorToInt(timelineIndexButton.Offset.X / 20);
                CurrentFrame = _timerIndex;
                Model?.SetAnimationFrame(_timerIndex);
                timelineIndexButton.Align();
                timelineIndexButton.UpdateTransformation();
                _oldMousePos = mousePosition;
            }
        });
        timelineIndexButton.SetOnRelease(() => { _moveTimeline = true; });

        timerPanelCollection.AddElements(timelineIndexButton);

        TimerUI.AddElement(timerPanelCollection);
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
        CameraSpeedField.SetText($"{Game.camera.SPEED}").UpdateCharacters();

        Console.WriteLine("Start Rigging Editor");

        if (Model == null)
            return;

        Model.Animation = Animation;
    }

    public override void Resize()
    {
        ModelingUi.Resize();
        TimelineUI.Resize();
        KeyframeUI.Resize();
        TimerUI.Resize();
    }

    public override void Awake()
    {
        ModelSettings.WireframeVisible = false;

        Console.WriteLine("Awake Animation Editor");

        Model?.SetAnimationRig();
        Model?.SetAnimation();

        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = true;
        }

        foreach (var (name, model) in ModelManager.SelectedModels)
        {
            model.Model.Animation = new Animation($"{name}_Animation");
        }

        if (Model != null)
        {
            Model.Animation = new Animation("Default_Animation");
            Animation = Model.Animation;
            GenerateAnimationKeyframes(Model);
        }
            
        KeyframePanelCollection.Align();

        Editor.AfterNewSelectedModelAction = () =>
        {
            if (Model == null)
            {
                return;
            }
            else
            {
                GenerateAnimationTimeline(Model, Model.Animation);
            }
        };

        Info.RenderInfo = false;
        Handle_BoneMovement();
        regenerateVertexUi = true;
    }

    public void GenerateAnimationKeyframes(Model model)
    {   
        if (model.Animation == null || model.Rig == null)
            return;

        
        TimelineScrollView.DeleteSubElements();

        Animation animation = model.Animation;
        TimelineScrollView.ScrollPosition = 0;
        BoneAnimations = [];

        if (model.Rig != null)
        {
            int i = 0;
            foreach (var bone in model.Rig.BonesList)
            {
                animation.AddBoneAnimation(bone.Name, out var boneAnimation);
                TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
                timelineAnimation.Index = i;
                
                UITextButton boneTextButton = new UITextButton(bone.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (10, 0.05f));
                boneTextButton.SetTextCharCount(bone.Name, 1f);
                TimelineScrollView.AddElement(boneTextButton.Collection);
                TimelineUI.AddElement(boneTextButton.Collection);
                BoneAnimations.Add(bone.Name, timelineAnimation);

                AnimationKeyframe keyframe = new AnimationKeyframe(0, bone);
                boneAnimation.AddOrUpdateKeyframe(keyframe);

                UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, i * 25, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
                KeyframePanelCollection.AddElement(keyframeButton);
                KeyframeUI.AddElement(keyframeButton);

                keyframeButton.SetOnClick(() =>
                {
                    _oldMousePos = Input.GetMousePosition();
                });

                keyframeButton.SetOnHold(() =>
                {
                    Vector2 mouseDelta = Input.GetMouseDelta();
                    if (mouseDelta == Vector2.Zero)
                        return;

                    Vector2 mousePosition = Input.GetMousePosition();

                    int sign = Mathf.Sign(mouseDelta.X);
                    float delta = Mathf.Step(20, mousePosition.X - _oldMousePos.X);
                    
                    if (delta != 0)
                    {
                        if (keyframeButton.Offset.X + delta < 0)
                            return;

                        int index = Mathf.FloorToInt((delta + keyframeButton.Offset.X) / 20);
                        int i = 0;
                        while (boneAnimation.ContainsIndex(index))
                        {
                            i += sign;
                            index += i;
                        }

                        keyframe.SetIndex(index);
                        delta += i * 20;

                        keyframeButton.Offset.X += delta;
                        keyframeButton.Align();
                        keyframeButton.UpdateTransformation();
                        _oldMousePos = mousePosition;
                    }
                });

                timelineAnimation.Add(keyframeButton, keyframe);

                i++;
            }
        }

        TimelineScrollView.ResetInit();
    }

    public void GenerateAnimationTimeline(Model model, Animation? animation)
    {
        if (model.Rig == null || animation == null)
            return;

        TimelineScrollView.DeleteSubElements();

        foreach (var boneAnimation in BoneAnimations)
        {
            boneAnimation.Value.Clear();
        }

        BoneAnimations = [];

        int i = 0;
        foreach (var bone in model.Rig.BonesList)
        {
            if (!animation.TryGetBoneAnimation(bone.Name, out var boneAnimation))
            {
                boneAnimation = new BoneAnimation(bone.Name);
                animation.AddBoneAnimation(boneAnimation);
            }

            TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
            timelineAnimation.Index = i;

            UITextButton boneTextButton = new UITextButton(bone.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (10, 0.05f));
            boneTextButton.SetTextCharCount(bone.Name, 1f);
            TimelineScrollView.AddElement(boneTextButton.Collection);
            TimelineUI.AddElement(boneTextButton.Collection);
            BoneAnimations.Add(bone.Name, timelineAnimation);

            if (boneAnimation.Keyframes.Count == 0)
            {
                AnimationKeyframe keyframe = new AnimationKeyframe(0, bone);
                boneAnimation.AddOrUpdateKeyframe(keyframe);
            }

            foreach (var keyframe in boneAnimation.Keyframes)
            {
                UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (keyframe.Index * 20, i * 25, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
                KeyframePanelCollection.AddElement(keyframeButton);

                KeyframeUI.AddElement(keyframeButton);

                keyframeButton.SetOnClick(() =>
                {
                    _oldMousePos = Input.GetMousePosition();
                });

                keyframeButton.SetOnHold(() =>
                {
                    Vector2 mouseDelta = Input.GetMouseDelta();
                    if (mouseDelta == Vector2.Zero)
                        return;

                    Vector2 mousePosition = Input.GetMousePosition();

                    int sign = Mathf.Sign(mouseDelta.X);
                    float delta = Mathf.Step(20, mousePosition.X - _oldMousePos.X);

                    if (delta != 0)
                    {
                        if (keyframeButton.Offset.X + delta < 0)
                            return;

                        int index = Mathf.FloorToInt((delta + keyframeButton.Offset.X) / 20);
                        int i = 0;
                        while (boneAnimation.ContainsIndex(index))
                        {
                            i += sign;
                            index += i;
                        }

                        keyframe.SetIndex(index);
                        delta += i * 20;

                        keyframeButton.Offset.X += delta;
                        keyframeButton.Align();
                        keyframeButton.UpdateTransformation();
                        _oldMousePos = mousePosition;
                    }
                });

                timelineAnimation.Add(keyframeButton, keyframe);
            }
            i++;
        }

        TimelineScrollView.ResetInit();
        KeyframePanelCollection.Align();
        model.Animation = animation;
    }

    public override void Render()
    {
        Editor.RenderModel();

        ModelingUi.RenderDepthTest();
        TimelineUI.RenderDepthTest();

        GL.Viewport(220, 10, Game.Width - 590, 240);

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        tickShader.Bind();

        Matrix4 tickModel = Matrix4.CreateTranslation((TimelinePosition.X, TimelinePosition.Y, 0));
        Matrix4 tickProjection = Matrix4.CreateOrthographicOffCenter(0, Game.Width - 590, 240, 0, -1, 1);

        int tickModelLoc = GL.GetUniformLocation(tickShader.ID, "model");
        int tickProjectionLoc = GL.GetUniformLocation(tickShader.ID, "projection");
        int lineDataLoc = GL.GetUniformLocation(tickShader.ID, "lineData");

        GL.UniformMatrix4(tickModelLoc, true, ref tickModel);
        GL.UniformMatrix4(tickProjectionLoc, true, ref tickProjection);
        GL.Uniform4(lineDataLoc, new Vector4(0.03f, 0.1f, 0.8f, 0.5f));

        TickSSBO.Bind(0);
        tickVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, _tickCount);

        Shader.Error("Tick shader error: ");

        tickVao.Unbind();
        TickSSBO.Unbind();

        tickShader.Unbind();

        TimerUI.RenderDepthTest(tickProjection);

        GL.Viewport(220, 16, Game.Width - 590, 207);

        Matrix4 keyframeProjection = Matrix4.CreateOrthographicOffCenter(0, Game.Width - 590, 207, 0, -1, 1);
        KeyframeUI.RenderDepthTest(keyframeProjection);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }


    public override void Update()
    {
        KeyframeUI.SetPosition(TimelinePosition.X, TimelineScrollView.ScrollPosition);
        Vector2 keyframeScreenPos = new Vector2(220, Game.Height - 223);

        TimerUI.SetPosition(TimelinePosition.X, TimerUI.Position.Y);
        Vector2 timerScreenPos = new Vector2(220, Game.Height - 256);

        TimerUI.Test(timerScreenPos);
        KeyframeUI.Test(keyframeScreenPos);
        ModelingUi.Update();
        TimelineUI.Update();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            Editor.freeCamera = !Editor.freeCamera;

            if (Editor.freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.Unlock();
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
                Game.camera.Lock();
                Model?.UpdateVertexPosition();
                Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
            }
        }

        if (!Editor.freeCamera)
        {
            if (Input.IsKeyPressed(Keys.Space) && Model != null)
            {
                Playing = !Playing;
                Model.Animate = Playing;
                regenerateVertexUi = !Playing;
            }
        }

        if (Playing)
            ModelManager.Update();
        else
            RigUpdate();
    }

    private void RigUpdate()
    {
        if (Input.IsMousePressed(MouseButton.Left))
            TestBonePosition();

        TestAdd();
        TestRemove();

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
            Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
            regenerateVertexUi = true;
        }

        if (Input.IsKeyReleased(Keys.R))
        {
            Game.SetCursorState(CursorState.Normal);
            Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
        }

        if (regenerateVertexUi)
        {
            Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
            regenerateVertexUi = false;
        }
    }

    public void Handle_BoneMovement()
    {
        if (Model == null || Model.Rig == null)
            return;

        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta == Vector2.Zero)
            return;

        foreach (var bone in Model.Rig.BonesList)
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
        Model.UpdateRig();

        foreach (var bone in Model.Rig.BonesList)
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
            Console.WriteLine($"Distance: {distance}, Position: {position}, MousePos: {mousePos}, Radius: {radius}");
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

    public HashSet<Bone> GetSelectedBones()
    {
        HashSet<Bone> selectedBones = new HashSet<Bone>();
        foreach (var pivot in SelectedBonePivots)
        {
            selectedBones.Add(pivot.Bone);
        }
        return selectedBones;
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

        Model?.UpdateRig();
    }

    public override void Exit()
    {
        TimelineScrollView.DeleteSubElements();
        TimelineUI.RemoveElements();
        TimelineUI.UpdateMeshes();

        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = true;
        }

        if (Model != null)
        {   
            Model.RenderBones = false;
            Model.Rig?.Delete();
            Model.SetAnimationRig();
        }

        foreach (var boneAnimation in BoneAnimations)
        {
            boneAnimation.Value.Clear();
        }
        
        KeyframeUI.RemoveElements();
        KeyframeUI.UpdateMeshes();

        Info.RenderInfo = true;

        Playing = false;
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
                Console.WriteLine("Add keyframe");
                AddKeyframe();
            }
        }

        if (Input.IsKeyReleased(Keys.Q))
        {
            _d_pressed = false;
        }
    }
    
    public void TestRemove()
    {
        if (Input.IsKeyDown(Keys.LeftControl) && Input.IsKeyPressed(Keys.Delete))
        {
            Console.WriteLine("Remove keyframe");
            RemoveKeyframe();
            Model?.SetAnimationFrame(CurrentFrame);
            Model?.UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
            if (Model == null || Model.Rig == null)
                return;

            Model.UpdateRig();
            foreach (var bone in Model.Rig.BonesList)
            {
                bone.UpdateEndTarget();
            }
        }
    }

    public void AddKeyframe()
    {
        HashSet<Bone> selectedBones = GetSelectedBones();
        foreach (var bone in selectedBones)
        {
            if (!BoneAnimations.TryGetValue(bone.Name, out TimelineBoneAnimation? timelineAnimation))
                continue;

            int currentFrame = CurrentFrame;
            AnimationKeyframe keyframe = new AnimationKeyframe(CurrentFrame, bone.Position, bone.Rotation, bone.Scale);
            Animation.AddOrUpdateKeyframe(bone.Name, keyframe, out bool added);

            if (added)
            {
                UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (currentFrame * 20, timelineAnimation.Index * 25, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
                KeyframePanelCollection.AddElement(keyframeButton);
                KeyframePanelCollection.Align();

                keyframeButton.SetOnClick(() =>
                {
                    _oldMousePos = Input.GetMousePosition();
                });

                keyframeButton.SetOnHold(() =>
                {
                    Vector2 mouseDelta = Input.GetMouseDelta();
                    if (mouseDelta == Vector2.Zero)
                        return;

                    Vector2 mousePosition = Input.GetMousePosition();

                    int sign = Mathf.Sign(mouseDelta.X);
                    float delta = Mathf.Step(20, mousePosition.X - _oldMousePos.X);

                    if (delta != 0)
                    {
                        if (keyframeButton.Offset.X + delta < 0)
                            return;

                        int index = Mathf.FloorToInt((delta + keyframeButton.Offset.X) / 20);
                        int i = 0;
                        while (timelineAnimation.Animation.ContainsIndex(index))
                        {
                            i += sign;
                            index += i;
                        }

                        keyframe.SetIndex(index);
                        delta += i * 20;

                        keyframeButton.Offset.X += delta;
                        keyframeButton.Align();
                        keyframeButton.UpdateTransformation();
                        _oldMousePos = mousePosition;
                    }
                });

                timelineAnimation.Add(keyframeButton, keyframe);

                KeyframeUI.AddElement(keyframeButton);
            }
        }
    }

    public void RemoveKeyframe()
    {
        HashSet<Bone> selectedBones = GetSelectedBones();
        foreach (var bone in selectedBones)
        {
            if (Animation.RemoveKeyframe(bone.Name, CurrentFrame, out var keyframe))
            {
                if (BoneAnimations.TryGetValue(bone.Name, out TimelineBoneAnimation? timelineAnimation))
                {
                    if (timelineAnimation.Get(keyframe, out UIButton? button))
                    {
                        button.Delete();
                        timelineAnimation.Remove(keyframe);
                    }
                }
            }
        }
    }

    public void UpdateVertexPosition(System.Numerics.Matrix4x4 projection, System.Numerics.Matrix4x4 view)
    {
        Vertices.Clear();

        /*
        foreach (var vert in Mesh.VertexList)
        {
            Vector2? screenPos = Mathf.WorldToScreen(vert, projection, view);
            if (screenPos == null)
                continue;
            
            Vertices.Add(vert, screenPos.Value);
        }
        */
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

    public class TimelineBoneAnimation
    {
        public string Name;
        public int Index;
        public BoneAnimation Animation;
        public Dictionary<UIButton, AnimationKeyframe> ButtonKeyframes = new Dictionary<UIButton, AnimationKeyframe>();
        public Dictionary<AnimationKeyframe, UIButton> KeyframeButtons = new Dictionary<AnimationKeyframe, UIButton>();

        public TimelineBoneAnimation(string name, BoneAnimation animation)
        {
            Name = name;
            Animation = animation;
        }

        public void Add(UIButton button, AnimationKeyframe keyframe)
        {
            ButtonKeyframes.Add(button, keyframe);
            KeyframeButtons.Add(keyframe, button);
        }

        public void Remove(UIButton button)
        {
            if (ButtonKeyframes.TryGetValue(button, out AnimationKeyframe? keyframe))
            {
                ButtonKeyframes.Remove(button);
                KeyframeButtons.Remove(keyframe);
            }
        }

        public void Remove(AnimationKeyframe keyframe)
        {
            if (KeyframeButtons.TryGetValue(keyframe, out UIButton? button))
            {
                KeyframeButtons.Remove(keyframe);
                ButtonKeyframes.Remove(button);
            }
        }

        public bool Get(UIButton button, [NotNullWhen(true)] out AnimationKeyframe? keyframe)
        {
            if (ButtonKeyframes.TryGetValue(button, out AnimationKeyframe? value))
            {
                keyframe = value;
                return true;
            }
            keyframe = null;
            return false;
        }

        public bool Get(AnimationKeyframe keyframe, [NotNullWhen(true)] out UIButton? button)
        {
            if (KeyframeButtons.TryGetValue(keyframe, out UIButton? value))
            {
                button = value;
                return true;
            }
            button = null;
            return false;
        }

        public void Clear()
        {
            foreach (var button in ButtonKeyframes.Keys)
            {
                button.Delete();
            }
            Animation.Clear();
            ButtonKeyframes = [];
            KeyframeButtons = [];
        }
    }
}