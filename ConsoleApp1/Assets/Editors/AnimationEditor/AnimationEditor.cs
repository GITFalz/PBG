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
    public UIController AnimationSavingUI;

    public UIScrollView MainPanelStacking;
    public UIInputFieldPrefab TimelineTimeField;
    public UICollection TimelineKeyframeEasingCollection;
    public UIScrollView TimelineScrollView;
    public UIInputField CameraSpeedField;

    public UICollection KeyframePanelCollection;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public static UIInputFieldPrefab BoneNameText;

    public static UIInputField BoneXPositionField;
    public static UIInputField BoneYPositionField;
    public static UIInputField BoneZPositionField;

    public static UIInputField BoneXRotationField;
    public static UIInputField BoneYRotationField;
    public static UIInputField BoneZRotationField;

    public UIVerticalCollection BoneInfoCollection;
    public UIVerticalCollection AnimationCollection;
    public UIVerticalCollection AnimationInfoCollection;
    public UIVerticalCollection KeyframeInfoCollection;


    public UIImage PositionEasingPanel;
    public UIImage RotationEasingPanel;


    public bool SaveAnimation
    {
        get { return _saveAnimation; }
        set
        {
            _saveAnimation = value;
            ToggleAnimationSavingUI();
        }
    }
    
    private bool _saveAnimation = false;
    public bool GenerateEveryKeyframe = false;

    private UIButton _animationGenerateEveryKeyframeButton;

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
    public Dictionary<Model, (UITextButton, Dictionary<string, TimelineBoneAnimation>)> SideBoneAnimations = [];
    private Vector2 _oldMousePos = new Vector2(0, 0);
    private bool _moveTimeline = false;
    private int _timerIndex = 0;

    public struct SelectedKeyframeStruct
    {
        public UIButton Button;
        public AnimationKeyframe Keyframe;
        public TimelineBoneAnimation Animation;
        public Bone Bone;

        public SelectedKeyframeStruct(UIButton button, AnimationKeyframe keyframe, TimelineBoneAnimation animation, Bone bone)
        {
            Button = button;
            Keyframe = keyframe;
            Animation = animation;
            Bone = bone;
        }
    }
    public SelectedKeyframeStruct? SelectedKeyframe = null;

    public AnimationEditor(GeneralModelingEditor editor) : base(editor)
    {
        ModelingUi = new UIController("ModelingUI");

        UICollection mainPanelCollection = new("MainPanelCollection", ModelingUi, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (7.5f, 0.05f));
        mainPanel.SetBottomPc(50);

        MainPanelStacking = new("MainPanelStacking", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, CollectionType.Vertical, (245, 0), (0, 0, 0, 0));
        MainPanelStacking.SetBorder((0, 10, 5, 5));
        MainPanelStacking.SetSpacing(5);
        MainPanelStacking.SetTopPx(5);
        MainPanelStacking.SetBottomPc(50);
        MainPanelStacking.AddBottomPx(5);


        // Main panel collection
        UICollection cullingCollection = new("CullingCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        BackfaceCullingText = new("CullingText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        BackfaceCullingText.SetText("cull: " + ModelSettings.BackfaceCulling, 1.2f);

        UIButton cullingButton = new("CullingButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.Static);
        cullingButton.SetOnClick(BackFaceCullingSwitch);

        cullingCollection.AddElements(BackfaceCullingText, cullingButton);


        // Alpha panel collection
        UICollection alphaCollection = new("AlphaCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        MeshAlphaText = new("AlphaText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2"), 1.2f);

        UIButton alphaButton = new("AlphaUpButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.Static);
        alphaButton.SetOnClick(() => { blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);


        // Bone info panel collection
        UITextButton boneInfoToggleButton = new UITextButton("BoneInfoToggleButton", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (225, 30), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        boneInfoToggleButton.SetTextCharCount("Bone Info", 0.9f);

        BoneInfoCollection = new("BoneInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 45), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        boneInfoToggleButton.SetOnClick(() => ToggleElements(!BoneInfoCollection.Visible, BoneInfoCollection));

        UICollection boneTextCollection = new("BoneTextCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIText boneNameText = new("BoneNameText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneNameText.SetTextCharCount("Name:", 1.2f);

        UITextButton boneSetNameButton = new("BoneSetNameButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.Static);
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
        BoneNameText.SetText("BoneName", 0.9f);
        BoneNameText.UpdateScaling();
        BoneNameText.Collection.SetAnchorType(AnchorType.TopLeft);


        UIVerticalCollection bonePositionCollection = new("BonePositionCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection bonePositionTextCollection = new("BonePositionTextCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIText bonePositionText = new("BonePositionText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionText.SetTextCharCount("Position:", 1.2f);

        bonePositionTextCollection.AddElements(bonePositionText);

        UICollection bonePositionXCollection = new("BonePositionXCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton bonePositionXButton = new("BonePositionXButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        bonePositionXButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneXPositionField.UpdateText(((float)Math.Round(Float.Parse(BoneXPositionField.Text) + delta * 0.1f, 2)).ToString());
        });


        UIText bonePositionXText = new("BonePositionXText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionXText.SetTextCharCount("X:", 1f);

        UIInputFieldPrefab bonePositionXField = new("BonePositionXField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        bonePositionXField.SetText("0", 1f);
        bonePositionXField.UpdateScaling();
        bonePositionXField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneXPositionField = bonePositionXField.InputField;
        BoneXPositionField.SetOnTextChange(() =>
        {
            MoveBone(0, BoneXPositionField.Text);
        });


        bonePositionXCollection.AddElements(bonePositionXButton, bonePositionXText, bonePositionXField.Collection);

        UICollection bonePositionYCollection = new("BonePositionYCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton bonePositionYButton = new("BonePositionYButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        bonePositionYButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneYPositionField.UpdateText(((float)Math.Round(Float.Parse(BoneYPositionField.Text) + delta * 0.1f, 2)).ToString());
        });

        UIText bonePositionYText = new("BonePositionYText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionYText.SetTextCharCount("Y:", 1f);

        UIInputFieldPrefab bonePositionYField = new("BonePositionYField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        bonePositionYField.SetText("0", 1f);
        bonePositionYField.UpdateScaling();
        bonePositionYField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneYPositionField = bonePositionYField.InputField;
        BoneYPositionField.SetOnTextChange(() =>
        {
            MoveBone(1, BoneYPositionField.Text);
        });

        bonePositionYCollection.AddElements(bonePositionYButton, bonePositionYText, bonePositionYField.Collection);

        UICollection bonePositionZCollection = new("BonePositionZCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton bonePositionZButton = new("BonePositionZButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        bonePositionZButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneZPositionField.UpdateText(((float)Math.Round(Float.Parse(BoneZPositionField.Text) + delta * 0.1f, 2)).ToString());
        });

        UIText bonePositionZText = new("BonePositionZText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        bonePositionZText.SetTextCharCount("Z:", 1f);

        UIInputFieldPrefab bonePositionZField = new("BonePositionZField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        bonePositionZField.SetText("0", 1f);
        bonePositionZField.UpdateScaling();
        bonePositionZField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneZPositionField = bonePositionZField.InputField;
        BoneZPositionField.SetOnTextChange(() =>
        {
            MoveBone(2, BoneZPositionField.Text);
        });

        bonePositionZCollection.AddElements(bonePositionZButton, bonePositionZText, bonePositionZField.Collection);

        bonePositionCollection.AddElements(bonePositionTextCollection, bonePositionXCollection, bonePositionYCollection, bonePositionZCollection);


        UIVerticalCollection boneRotationCollection = new("BoneRotationCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection boneRotationTextCollection = new("BoneRotationTextCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton boneRotationXButton = new("BoneRotationXButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        boneRotationXButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneXRotationField.UpdateText(((float)Math.Round((Float.Parse(BoneXRotationField.Text) + delta * 0.1f + 360) % 360, 2)).ToString());
        });

        UIText boneRotationText = new("BoneRotationText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationText.SetTextCharCount("Rotation:", 1.2f);

        boneRotationTextCollection.AddElements(boneRotationText);

        UICollection boneRotationXCollection = new("BoneRotationXCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIText boneRotationXText = new("BoneRotationXText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationXText.SetTextCharCount("X:", 1f);

        UIInputFieldPrefab boneRotationXField = new("BoneRotationXField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        boneRotationXField.SetText("0", 1f);
        boneRotationXField.UpdateScaling();
        boneRotationXField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneXRotationField = boneRotationXField.InputField;
        BoneXRotationField.SetOnTextChange(() =>
        {
            RotateBone(0, BoneXRotationField.Text);
        });

        boneRotationXCollection.AddElements(boneRotationXButton, boneRotationXText, boneRotationXField.Collection);

        UICollection boneRotationYCollection = new("BoneRotationYCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton boneRotationYButton = new("BoneRotationYButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        boneRotationYButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneYRotationField.UpdateText(((float)Math.Round((Float.Parse(BoneYRotationField.Text) + delta * 0.1f + 360) % 360, 2)).ToString());
        });

        UIText boneRotationYText = new("BoneRotationYText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationYText.SetTextCharCount("Y:", 1f);

        UIInputFieldPrefab boneRotationYField = new("BoneRotationYField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        boneRotationYField.SetText("0", 1f);
        boneRotationYField.UpdateScaling();
        boneRotationYField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneYRotationField = boneRotationYField.InputField;
        BoneYRotationField.SetOnTextChange(() =>
        {
            RotateBone(1, BoneYRotationField.Text);
        });

        boneRotationYCollection.AddElements(boneRotationYButton, boneRotationYText, boneRotationYField.Collection);

        UICollection boneRotationZCollection = new("BoneRotationZCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIButton boneRotationZButton = new("BoneRotationZButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 20), (-5, 0, 0, 0), 0, 0, (7.5f, 0.05f), UIState.InvisibleInteractable);
        boneRotationZButton.SetOnHold(() =>
        {
            float delta = Input.GetMouseDelta().X;
            if (delta == 0) return;
            BoneZRotationField.UpdateText(((float)Math.Round((Float.Parse(BoneZRotationField.Text) + delta * 0.1f + 360) % 360, 2)).ToString());
        });

        UIText boneRotationZText = new("BoneRotationZText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        boneRotationZText.SetTextCharCount("Z:", 1f);

        UIInputFieldPrefab boneRotationZField = new("BoneRotationZField", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 15, "0", TextType.Decimal);
        boneRotationZField.SetText("0", 1f);
        boneRotationZField.UpdateScaling();
        boneRotationZField.Collection.SetAnchorType(AnchorType.MiddleRight);
        BoneZRotationField = boneRotationZField.InputField;
        BoneZRotationField.SetOnTextChange(() =>
        {
            RotateBone(2, BoneZRotationField.Text);
        });

        boneRotationZCollection.AddElements(boneRotationZButton, boneRotationZText, boneRotationZField.Collection);

        boneRotationCollection.AddElements(boneRotationTextCollection, boneRotationXCollection, boneRotationYCollection, boneRotationZCollection);


        BoneInfoCollection.AddElements(boneTextCollection, BoneNameText.Collection, bonePositionCollection, boneRotationCollection);



        UITextButton animationInfoToggleButton = new UITextButton("AnimationInfoToggleButton", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (225, 30), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        animationInfoToggleButton.SetTextCharCount("Animation Info", 0.9f);

        AnimationCollection = new UIVerticalCollection("AnimationCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 70), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        animationInfoToggleButton.SetOnClick(() => ToggleElements(!AnimationCollection.Visible, AnimationCollection));

        // Animation info panel collection

        UIVerticalCollection animationInfoCollection = new("AnimationInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 70), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        UICollection animationButtonCollection = new("AnimationButtonCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UITextButton animationSaveButton = new("AnimationSaveButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 20, 0), (210, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        animationSaveButton.SetTextCharCount("Manage", 1f);

        animationSaveButton.SetOnClick(() => { SaveAnimation = true; });

        animationButtonCollection.AddElements(animationSaveButton.Collection);

        animationInfoCollection.AddElements(animationButtonCollection);

        // Animation keyframe collection
        UITextButton keyframeInfoToggleButton = new("KeyframeInfoToggleButton", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (210, 30), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        keyframeInfoToggleButton.SetTextCharCount("Keyframe Info", 1f);

        KeyframeInfoCollection = new UIVerticalCollection("KeyframeInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (210, 70), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        keyframeInfoToggleButton.SetOnClick(() => { ToggleElements(!KeyframeInfoCollection.Visible, KeyframeInfoCollection); });

        // Position easing collection
        UICollection keyframePositionEasingCollection = new("KeyframePositionEasingCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIText keyframePositionEasingText = new("KeyframePositionEasingText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        keyframePositionEasingText.SetTextCharCount("Position:", 0.9f);

        PositionEasingPanel = new("PositionEasingPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, -50, 0), 0, 60, (-1, -1));

        UIImage positionEasingLeftPanel = new("PositionEasingLeftPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, -25, 0), 0, 70, (-1, -1));
        positionEasingLeftPanel.SetOnClick(() =>
        {
            if (SelectedKeyframe == null)
                return;
            AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;
            TimelineBoneAnimation animation = SelectedKeyframe.Value.Animation;

            int ease = PositionEasingPanel.TextureIndex % 60;
            int newEase = ease - 1;
            if (ease == 0)
                return;

            if (newEase >= 0)
            {
                PositionEasingPanel.TextureIndex = newEase + 60;
                PositionEasingPanel.UpdateTexture();
                keyframe.PositionEasing = (EasingType)newEase;
                animation.RegenerateBoneKeyframes();
            }
        });

        UIImage positionEasingRightPanel = new("PositionEasingRightPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, 0, 0), 0, 71, (-1, -1));
        positionEasingRightPanel.SetOnClick(() =>
        {
            if (SelectedKeyframe == null)
                return;
            AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;
            TimelineBoneAnimation animation = SelectedKeyframe.Value.Animation;

            int ease = PositionEasingPanel.TextureIndex % 60;
            int newEase = ease + 1;
            if (ease == 5)
                return;

            if (newEase <= 5)
            {
                PositionEasingPanel.TextureIndex = newEase + 60;
                PositionEasingPanel.UpdateTexture();
                keyframe.PositionEasing = (EasingType)newEase;
                animation.RegenerateBoneKeyframes();
            }
        });

        keyframePositionEasingCollection.AddElements(keyframePositionEasingText, PositionEasingPanel, positionEasingLeftPanel, positionEasingRightPanel);

        // Rotation easing collection
        UICollection keyframeRotationEasingCollection = new("KeyframeRotationEasingCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (210, 20), (0, 0, 0, 0), 0);

        UIText keyframeRotationEasingText = new("KeyframeRotationEasingText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        keyframeRotationEasingText.SetTextCharCount("Rotation:", 0.9f);

        RotationEasingPanel = new("RotationEasingPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, -50, 0), 0, 60, (-1, -1));

        UIImage rotationEasingLeftPanel = new("RotationEasingLeftPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, -25, 0), 0, 70, (-1, -1));
        rotationEasingLeftPanel.SetOnClick(() =>
        {
            if (SelectedKeyframe == null)
                return;
            AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;
            TimelineBoneAnimation animation = SelectedKeyframe.Value.Animation;

            int ease = RotationEasingPanel.TextureIndex % 60;
            int newEase = ease - 1;
            if (ease == 0)
                return;

            if (newEase >= 0)
            {
                RotationEasingPanel.TextureIndex = newEase + 60;
                RotationEasingPanel.UpdateTexture();
                keyframe.RotationEasing = (EasingType)newEase;
                animation.RegenerateBoneKeyframes();
            }
        });

        UIImage rotationEasingRightPanel = new("RotationEasingRightPanel", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 20, 0), (20, 20), (0, 0, 0, 0), 0, 71, (-1, -1));
        rotationEasingRightPanel.SetOnClick(() =>
        {
            if (SelectedKeyframe == null)
                return;
            AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;
            TimelineBoneAnimation animation = SelectedKeyframe.Value.Animation;

            int ease = RotationEasingPanel.TextureIndex % 60;
            int newEase = ease + 1;
            if (ease == 5)
                return;

            if (newEase <= 5)
            {
                RotationEasingPanel.TextureIndex = newEase + 60;
                RotationEasingPanel.UpdateTexture();
                keyframe.RotationEasing = (EasingType)newEase;
                animation.RegenerateBoneKeyframes();
            }
        });

        keyframeRotationEasingCollection.AddElements(keyframeRotationEasingText, RotationEasingPanel, rotationEasingLeftPanel, rotationEasingRightPanel);

        KeyframeInfoCollection.AddElements(keyframePositionEasingCollection, keyframeRotationEasingCollection);

        AnimationCollection.AddElements(animationInfoCollection, keyframeInfoToggleButton, KeyframeInfoCollection);
        KeyframeInfoCollection.SetVisibility(false);


        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (210, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (7.5f, 0.05f));

        CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (7.5f, 0.05f));

        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = Int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; CameraSpeedField.SetText("1").UpdateCharacters(); } });

        speedStacking.SetScale((45, 30f));
        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);

        MainPanelStacking.AddElements(cullingCollection, alphaCollection, boneInfoToggleButton, BoneInfoCollection, animationInfoToggleButton, AnimationCollection);

        mainPanelCollection.AddElements(mainPanel, MainPanelStacking, cameraSpeedStacking);


        // Add elements to ui
        ModelingUi.AddElement(mainPanelCollection);


        //-- Animation Saving UI --//
        AnimationSavingUI = new UIController("AnimationSavingUI");

        UICollection animationSavingPanelCollection = new("AnimationSavingPanelCollection", AnimationSavingUI, AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), (400, 300), (0, 0, 0, 0), 0);

        UIButton animationSavingMoveButton = new("AnimationMoveSavingButton", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (380, 20), (0, -20, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        animationSavingMoveButton.SetOnHold(() =>
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero)
                return;

            animationSavingPanelCollection.Offset.X += mouseDelta.X;
            animationSavingPanelCollection.Offset.Y += mouseDelta.Y;
            animationSavingPanelCollection.Align();
            animationSavingPanelCollection.UpdateTransformation();
        });
        
        UICollection closeButtonCollection = new UICollection("CloseButtonCollection", AnimationSavingUI, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (20, 20), (0, -20, 0, 0), 0);

        UIButton closeAnimationSavingButton = new("CloseAnimationSavingButton", AnimationSavingUI, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        closeAnimationSavingButton.SetOnClick(() => { SaveAnimation = false; });

        UIImage closeAnimationSavingImage = new("CloseAnimationSavingImage", AnimationSavingUI, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (28, 28), (0, 0, 0, 0), 0, 89, (-1, -1));
        closeAnimationSavingImage.Depth = 10;

        closeButtonCollection.AddElements(closeAnimationSavingButton, closeAnimationSavingImage);

        UIImage animationSavingBackground = new("AnimationSavingBackground", AnimationSavingUI, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        UICollection animationSavingStacking = new("AnimationSavingStacking", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (390, 290), (5, 5, 0, 0), 0);

        UICollection animationSaveNameCollection = new("AnimationNameCollection", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (390, 20), (0, 0, 0, 0), 0);

        UIImage animationSaveNamePanel = new("AnimationNamePanel", AnimationSavingUI, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (220, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIInputField animationSaveNameField = new("AnimationSaveNameField", AnimationSavingUI, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (7.5f, 0.05f));

        UITextButton animationSavingButton = new UITextButton("AnimationSavingButton", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (85, 20), (220, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        animationSavingButton.SetTextCharCount("Save", 1f);
        animationSavingButton.SetOnClick(() =>
        {
            if (Model == null || Model.Animation == null)
                return;

            AnimationManager.DisplayError = false;
            string name = animationSaveNameField.Text;
            if (string.IsNullOrEmpty(name))
            {
                PopUp.AddPopUp("Animation name cannot be empty");
                return;
            }
            if (!AnimationManager.TryGet(Model.Animation.Name, out Animation? animation))
            {
                AnimationManager.Add(Model.Animation);
            }
            AnimationManager.ChangeName(Model.Animation.Name, name);

            if (GenerateEveryKeyframe)
            {
                RegenerateBoneKeyframes();
            }
            else
            {
                ResetBoneKeyframes();
            }

            AnimationManager.DisplayError = true;
            AnimationManager.Save(name);
            AnimationManager.DisplayError = false;
        });

        UITextButton animationLoadingButton = new UITextButton("AnimationLoadingButton", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (85, 20), (305, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Static);
        animationLoadingButton.SetTextCharCount("Load", 1f);
        animationLoadingButton.SetOnClick(() =>
        {
            if (Model == null || Model.Animation == null)
                return;

            AnimationManager.DisplayError = false;

            string name = animationSaveNameField.Text;
            AnimationManager.Load(name);

            if (!AnimationManager.TryGet(name, out Animation? animation))
            {
                PopUp.AddPopUp("Animation not found or failed to load");
                return;
            }

            Model.Animation = animation;

            GenerateAnimationTimeline(Model);
            RegenerateBoneKeyframes();
        });

        animationSaveNameField.SetMaxCharCount(20).SetText("AnimationName", 1f).SetTextType(TextType.Alphanumeric);

        animationSaveNameCollection.AddElements(animationSaveNamePanel, animationSaveNameField);

        UIVerticalCollection animationSavingSettingsStacking = new("AnimationSavingSettingsStacking", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (390, 250), (0, 20, 0, 0), (0, 0, 0, 0), 0, 0);

        UICollection animationSaveAnimationWithAllKeyframesCollection = new("AnimationSaveAnimationWithAllKeyframesCollection", AnimationSavingUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (390, 20), (0, 0, 0, 0), 0);

        UIText animationSaveAnimationWithAllKeyframesButton = new("AnimationSaveAnimationWithAllKeyframesButton", AnimationSavingUI, AnchorType.MiddleLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (390, 20), (0, 0, 0, 0), 0);
        animationSaveAnimationWithAllKeyframesButton.SetTextCharCount("Generate every keyframe", 0.9f);

        _animationGenerateEveryKeyframeButton = new UIButton("AnimationSaveAnimationWithAllKeyframesToggle", AnimationSavingUI, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f), UIState.Static);
        _animationGenerateEveryKeyframeButton.SetOnClick(() =>
        {
            GenerateEveryKeyframe = !GenerateEveryKeyframe;
            if (!GenerateEveryKeyframe)
            {
                _animationGenerateEveryKeyframeButton.TextureIndex = 11;
                _animationGenerateEveryKeyframeButton.Color = (0.5f, 0.5f, 0.5f, 1f);
                _animationGenerateEveryKeyframeButton.UpdateTexture();
                _animationGenerateEveryKeyframeButton.UpdateColor();
            }
            else
            {
                _animationGenerateEveryKeyframeButton.TextureIndex = 10;
                _animationGenerateEveryKeyframeButton.Color = (1f, 1f, 1f, 1f);
                _animationGenerateEveryKeyframeButton.UpdateTexture();
                _animationGenerateEveryKeyframeButton.UpdateColor();
            }
        });

        animationSaveAnimationWithAllKeyframesCollection.AddElements(animationSaveAnimationWithAllKeyframesButton, _animationGenerateEveryKeyframeButton);

        animationSavingSettingsStacking.AddElements(animationSaveAnimationWithAllKeyframesCollection);

        animationSavingStacking.AddElements(animationSaveNameCollection, animationSavingButton.Collection, animationLoadingButton.Collection, animationSavingSettingsStacking);

        animationSavingPanelCollection.AddElements(animationSavingBackground, animationSavingStacking, animationSavingMoveButton, closeButtonCollection);

        AnimationSavingUI.AddElement(animationSavingPanelCollection);
        AnimationSavingUI.GenerateBuffers();

        

        TimelineUI = new UIController("TimelineUI");

        UICollection timelinePanelCollection = new("TimelinePanelCollection", TimelineUI, AnchorType.ScaleBottom, PositionType.Absolute, (0, 0, 0), (0, 250), (5, -5, 255, 5), 0);

        UIImage timelineBackground = new("TimelineBackground", TimelineUI, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width, 250), (0, 0, 0, 0), 0, 0, (7.5f, 0.05f));
        UIButton timelineMoveButton = new("TimelineMoveButton", TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0, 0), (0, 0, 0), (Game.Width - 385, 30), (7, 0, 0, 0), 0, 0, (0, 0), UIState.InvisibleInteractable);
        timelineMoveButton.SetOnHold(() =>
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero || !_moveTimeline)
                return;

            TimelinePosition.X = Mathf.Clamp(-1000, 0, TimelinePosition.X + mouseDelta.X);
        });

        UIVerticalCollection timelineSettingsStacking = new("TimelineSettingsStacking2", TimelineUI, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (100, 250), (0, 10, -10, 0), (0, 0, 5, 5), 0, 0);

        TimelineTimeField = new("TimelineTimeField", TimelineUI, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 11, 4, "0", TextType.Numeric);

        timelineSettingsStacking.AddElements(TimelineTimeField.Collection);

        UICollection timelineScrollViewCollection = new("TimelineScrollViewCollection", TimelineUI, AnchorType.ScaleBottom, PositionType.Relative, (0, 0, 0), (Game.Width - 10, 250 - 30), (5, -5, 110, 5), 0);

        UIImage timelineScrollViewBackground = new("TimelineScrollViewBackground", TimelineUI, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width - 10, 250 - 10), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
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
                ForSelectedModels(model => { model.SetAnimationFrame(_timerIndex); });
                timelineIndexButton.Align();
                timelineIndexButton.UpdateTransformation();
                _oldMousePos = mousePosition;
            }
        });
        timelineIndexButton.SetOnRelease(() => { _moveTimeline = true; });

        timerPanelCollection.AddElements(timelineIndexButton);

        TimerUI.AddElement(timerPanelCollection);
    }

    public override void Start() 
    { 
        Started = true;
        CameraSpeedField.SetText($"{Game.camera.SPEED}").UpdateCharacters();

        Console.WriteLine("Start Rigging Editor");
    }

    public override void Resize()
    {
        ModelingUi.Resize();
        AnimationSavingUI.Resize();
        TimelineUI.Resize();
        KeyframeUI.Resize();
        TimerUI.Resize();
    }

    public override void Awake()
    {
        ModelSettings.WireframeVisible = false;

        Console.WriteLine("Awake Animation Editor");

        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = true;
        }
            
        KeyframePanelCollection.Align();

        if (Model != null)
        {
            Model.Animation ??= new Animation($"{Model.Name}_Animation");
            Model.SetAnimationRig();
            Model.SetAnimation();
            GenerateAnimationTimeline(Model);
        }

        Editor.AfterNewSelectedModelAction = (model) =>
        {
            model.Animation ??= new Animation($"{model.Name}_Animation");
            model.SetAnimationRig();
            model.SetAnimation();
            GenerateAnimationTimeline(model);
        };

        Editor.AfterOtherSelectedModelAction = (model) =>
        {
            Console.WriteLine($"Selected model: {model.Name}");
            AddSideModel(model);
        };

        Editor.AfterUnSelectedModelAction = (model) =>
        {
            if (ModelManager.SelectedModels.Count == 0)
            {
                ClearTimeline();
            }
        };

        Editor.AfterUnSelectedOtherModelAction = (model) =>
        {
            Console.WriteLine($"Unselected model: {model.Name}");
            RemoveSideModel(model);
        };

        Handle_BoneMovement();

        Info.RenderInfo = false;
        regenerateVertexUi = true;
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

    public override void EndRender()
    {
        if (SaveAnimation)
            AnimationSavingUI.RenderNoDepthTest();
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
        AnimationSavingUI.Update();
        TimelineUI.Update();

        if (Input.IsKeyPressed(Keys.U))
        {
            ClearTimeline();
        }

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
            if (Input.IsKeyPressed(Keys.Space) && ModelManager.SelectedModels.Count > 0)
            {
                Playing = !Playing;
                if (Model != null) Model.Animate = Playing;
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


    public override void Exit()
    {
        TimelineScrollView.DeleteSubElements();
        TimelineUI.RemoveElements();
        TimelineUI.UpdateMeshes();

        foreach (var (_, model) in ModelManager.Models)
        {
            model.RenderBones = false;
        }

        if (Model != null)
        {
            Model.RenderBones = false;
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

        Editor.AfterNewSelectedModelAction = (model) => { };
        Editor.AfterUnSelectedModelAction = (model) => { };
    }

    public void ForSelectedModels(Action<Model> action)
    {
        foreach (var (_, modelData) in ModelManager.SelectedModels)
        {
            action(modelData.Model);
        }
    }


    public void RegenerateBoneKeyframes()
    {
        foreach (var (_, boneAnimation) in BoneAnimations)
        {
            boneAnimation.RegenerateBoneKeyframes();
        }
    }

    public void ResetBoneKeyframes()
    {
        foreach (var (_, boneAnimation) in BoneAnimations)
        {
            boneAnimation.ResetKeyframes();
        }
    }

    public void MoveBone(int i, string text)
    {
        if (SelectedBone == null)
            return;

        float value = Float.Parse(text);
        Vector3 position = SelectedBone.Position;
        position[i] = value;
        SelectedBone.Position = position;
        if (Model == null || Model.Rig == null)
            return;
        Model.Rig.RootBone.UpdateGlobalTransformation();
        Model.UpdateRig();
        foreach (var bone in Model.Rig.BonesList)
        {
            bone.UpdateEndTarget();
        }
    }

    public void RotateBone(int i, string text)
    {
        if (SelectedBone == null)
            return;

        float value = MathHelper.DegreesToRadians(Float.Parse(text));
        Vector3 rotation = SelectedBone.EulerRotation;
        rotation[i] = value;
        SelectedBone.EulerRotation = rotation;
        if (Model == null || Model.Rig == null)
            return;
        Model.Rig.RootBone.UpdateGlobalTransformation();
        Model.UpdateRig();
        foreach (var bone in Model.Rig.BonesList)
        {
            bone.UpdateEndTarget();
        }
    }

    public void ToggleElements(bool visible, params UIElement[] elemnts)
    {
        if (visible)
        {
            foreach (var element in elemnts)
            {
                element.SetVisibility(true);
            }
        }
        else
        {
            foreach (var element in elemnts)
            {
                element.SetVisibility(false);
            }
        }
        MainPanelStacking.ResetInit();
        MainPanelStacking.Align();
        MainPanelStacking.UpdateTransformation();
    }

    public void SetKeyframeInfo()
    {
        if (SelectedKeyframe == null)
            return;
        AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;

        if (!KeyframeInfoCollection.Visible)
            ToggleElements(true, KeyframeInfoCollection);

        PositionEasingPanel.TextureIndex = (int)keyframe.PositionEasing + 60;
        RotationEasingPanel.TextureIndex = (int)keyframe.RotationEasing + 60;
        PositionEasingPanel.UpdateTexture();
        RotationEasingPanel.UpdateTexture();
    }

    public void HideKeyframeInfo()
    {
        if (KeyframeInfoCollection.Visible)
            ToggleElements(false, KeyframeInfoCollection);
    }

    public void ToggleAnimationSavingUI()
    {
        if (!SaveAnimation)
        {
            GenerateEveryKeyframe = false;
            _animationGenerateEveryKeyframeButton.TextureIndex = 11;
            _animationGenerateEveryKeyframeButton.Color = (0.5f, 0.5f, 0.5f, 1f);
            _animationGenerateEveryKeyframeButton.UpdateTexture();
            _animationGenerateEveryKeyframeButton.UpdateColor();
        }
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



    public void ClearTimeline()
    {
        TimelineScrollView.DeleteSubElements();

        foreach (var boneAnimation in BoneAnimations)
        {
            boneAnimation.Value.ResetKeyframes();
            boneAnimation.Value.Clear();
        }

        foreach (var modelAnimations in SideBoneAnimations)
        {
            foreach (var boneAnimation in modelAnimations.Value.Item2)
            {
                boneAnimation.Value.ResetKeyframes();
                boneAnimation.Value.Clear();
            }
        }

        BoneAnimations = [];
        SideBoneAnimations = [];
    }

    public void GenerateAnimationTimeline(Model model)
    {
        Animation? animation = model.Animation;
        if (model.Rig == null || animation == null)
            return;

        ClearTimeline();

        for (int i = 0; i < model.Rig.BonesList.Count; i++)
        {
            Bone bone = model.Rig.BonesList[i];
            if (!animation.TryGetBoneAnimation(bone.Name, out var boneAnimation))
            {
                boneAnimation = new BoneAnimation(bone.Name);
                animation.AddBoneAnimation(boneAnimation);
            }

            TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
            timelineAnimation.Index = i;

            UITextButton boneTextButton = new UITextButton(bone.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
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
                var keyframeButton = CreateKeyElement(keyframe, timelineAnimation, bone, keyframe.Index);
                timelineAnimation.Add(keyframeButton, keyframe);
                KeyframeUI.AddElement(keyframeButton);
            }
            timelineAnimation.RegenerateBoneKeyframes();
        }

        foreach (var modelData in ModelManager.SelectedModels.Values)
        {
            AddSideModel(modelData.Model);
        }

        TimelineScrollView.ResetInit();
        KeyframePanelCollection.Align();
    }

    public void AddSideModel(Model model)
    {
        if (Model == model || model.Rig == null || model.Animation == null)
            return;

        Animation animation = model.Animation;

        UITextButton modelTextButton = new UITextButton(model.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
        modelTextButton.SetTextCharCount(model.Name, 1f);
        TimelineScrollView.AddElement(modelTextButton.Collection);
        TimelineUI.AddElement(modelTextButton.Collection);
        SideBoneAnimations.Add(model, (modelTextButton, new Dictionary<string, TimelineBoneAnimation>()));

        for (int i = 0; i < model.Rig.BonesList.Count; i++)
        {
            Bone bone = model.Rig.BonesList[i];
            if (!animation.TryGetBoneAnimation(bone.Name, out var boneAnimation))
            {
                boneAnimation = new BoneAnimation(bone.Name);
                animation.AddBoneAnimation(boneAnimation);
            }

            TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
            timelineAnimation.Index = i;

            SideBoneAnimations[model].Item2.Add(bone.Name, timelineAnimation);

            if (boneAnimation.Keyframes.Count == 0)
            {
                AnimationKeyframe keyframe = new AnimationKeyframe(0, bone);
                boneAnimation.AddOrUpdateKeyframe(keyframe);
            }
            foreach (var keyframe in boneAnimation.Keyframes)
            {
                var keyframeButton = new UIButton(); // can be empty because it isn't used anyways
                timelineAnimation.Add(keyframeButton, keyframe);
            }
            timelineAnimation.RegenerateBoneKeyframes();
        }
    }

    public void RemoveSideModel(Model model)
    {
        if (Model == model || !SideBoneAnimations.ContainsKey(model))
            return;

        foreach (var timelineAnimation in SideBoneAnimations[model].Item2.Values)
        {
            timelineAnimation.ResetKeyframes();
            timelineAnimation.Clear();
        }

        SideBoneAnimations[model].Item1.Collection.Delete();
        SideBoneAnimations.Remove(model);
    }

    void KeyframeSelection(UIButton button, AnimationKeyframe keyframe, TimelineBoneAnimation timelineAnimation, Bone bone)
    {
        if (button.ElementState == 0)
        {
            button.ElementState = 1;
            button.Color = (0.5f, 0.5f, 1f, 1f);
            if (SelectedKeyframe != null)
            {
                SelectedKeyframe.Value.Button.ElementState = 0;
                SelectedKeyframe.Value.Button.Color = (0.5f, 0.5f, 0.5f, 1f);
                SelectedKeyframe.Value.Button.UpdateColor();
            }
            SelectedKeyframe = new SelectedKeyframeStruct(button, keyframe, timelineAnimation, bone);
            SetKeyframeInfo();

        }
        else
        {
            button.ElementState = 0;
            button.Color = (0.5f, 0.5f, 0.5f, 1f);
            SelectedKeyframe = null;
            HideKeyframeInfo();
        }
        button.UpdateColor();
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

    public void TestAdd()
    {
        if (Model == null || Model.Animation == null)
            return;

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
                AddKeyframe(Model.Animation);
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

    public void AddKeyframe(Animation animation)
    {
        HashSet<Bone> selectedBones = GetSelectedBones();
        foreach (var bone in selectedBones)
        {
            AddKeyframe(animation, bone, CurrentFrame, true);
        }
    }

    /*
    animation.AddBoneAnimation(bone.Name, out var boneAnimation);
                TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
                timelineAnimation.Index = i;
                
                UITextButton boneTextButton = new UITextButton(bone.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
                boneTextButton.SetTextCharCount(bone.Name, 1f);
                TimelineScrollView.AddElement(boneTextButton.Collection);
                TimelineUI.AddElement(boneTextButton.Collection);
                BoneAnimations.Add(bone.Name, timelineAnimation);

                AnimationKeyframe keyframe = new AnimationKeyframe(0, bone);
                boneAnimation.AddOrUpdateKeyframe(keyframe);

                UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, i * 25, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
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
                        timelineAnimation.RegenerateBoneKeyframes();
                    }
                });

                timelineAnimation.Add(keyframeButton, keyframe);

                i++;
    */

    public void AddKeyframe(Animation animation, Bone bone, int frame, bool addToUIController)
    {
        if (!BoneAnimations.TryGetValue(bone.Name, out TimelineBoneAnimation? timelineAnimation))
            return;

        AnimationKeyframe keyframe = new AnimationKeyframe(frame, bone.Position, bone.Rotation, bone.Scale);

        if (animation.AddOrUpdateKeyframe(bone.Name, keyframe))
        {
            var keyframeButton = CreateKeyElement(keyframe, timelineAnimation, bone, frame);
            timelineAnimation.Add(keyframeButton, keyframe);
            if (addToUIController)
                KeyframeUI.AddElement(keyframeButton);
        }
    }

    public UIButton CreateKeyElement(AnimationKeyframe keyframe, TimelineBoneAnimation timelineAnimation, Bone bone, int frame)
    {
        UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (frame * 20, timelineAnimation.Index * 25, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        KeyframePanelCollection.AddElement(keyframeButton);
        KeyframePanelCollection.Align();
        keyframeButton.ElementState = 0;

        keyframeButton.SetOnClick(() =>
        {
            _oldMousePos = Input.GetMousePosition();
            KeyframeSelection(keyframeButton, keyframe, timelineAnimation, bone);
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
                while (timelineAnimation.ContainsIndex(index))
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
                timelineAnimation.RegenerateBoneKeyframes();
            }
        });
        return keyframeButton;
    }

    public void RemoveKeyframe()
    {
        if (Model == null || Model.Animation == null)
            return;

        if (SelectedKeyframe != null)
        {
            AnimationKeyframe keyframe = SelectedKeyframe.Value.Keyframe;
            UIButton button = SelectedKeyframe.Value.Button;
            TimelineBoneAnimation timelineAnimation = SelectedKeyframe.Value.Animation;
            Bone bone = SelectedKeyframe.Value.Bone;

            if (Model.Animation.RemoveKeyframe(bone.Name, keyframe))
            {
                button.Delete();
                timelineAnimation.Remove(keyframe);
                timelineAnimation.RegenerateBoneKeyframes();
            }
        }
        else
        {
            HashSet<Bone> selectedBones = GetSelectedBones();
            foreach (var bone in selectedBones)
            {
                if (Model.Animation.RemoveKeyframe(bone.Name, CurrentFrame, out var keyframe) &&
                    BoneAnimations.TryGetValue(bone.Name, out TimelineBoneAnimation? timelineAnimation) &&
                    timelineAnimation.Get(keyframe, out UIButton? button)
                )
                {
                    button.Delete();
                    timelineAnimation.Remove(keyframe);
                    timelineAnimation.RegenerateBoneKeyframes();
                }
            }
        }
    }

    public class TimelineBoneAnimation(string name, BoneAnimation animation)
    {
        public string Name = name;
        public int Index;
        public BoneAnimation Animation = animation;
        public Dictionary<UIButton, AnimationKeyframe> ButtonKeyframes = [];
        public Dictionary<AnimationKeyframe, UIButton> KeyframeButtons = [];

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

        public bool ContainsIndex(int index)
        {
            return ButtonKeyframes.Values.Any(k => k.Index == index);
        }

        public bool GetKeyframeAtIndex(int index, [NotNullWhen(true)] out AnimationKeyframe? keyframe)
        {
            keyframe = ButtonKeyframes.Values.FirstOrDefault(k => k.Index == index);
            return keyframe != null;
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

        public void RegenerateBoneKeyframes()
        {
            Animation.Clear();
            List<AnimationKeyframe> currentKeyframes = [.. ButtonKeyframes.Values];
            List<AnimationKeyframe> keyframes = [];
            OrderKeyframes(currentKeyframes);
            for (int i = 0; i < currentKeyframes.Count; i++)
            {
                AnimationKeyframe keyframe = currentKeyframes[i];
                if (keyframe.Index == 0)
                {
                    keyframes.Add(keyframe);
                }
                else if (i > 0)
                {
                    AnimationKeyframe before = currentKeyframes[i - 1];
                    int count = keyframe.Index - before.Index;
                    for (int j = 0; j < count; j++)
                    {
                        float t = (float)(j + 1) / count;
                        Vector3 position = Ease.Apply(keyframe.PositionEasing, before.Position, keyframe.Position, t);
                        Quaternion rotation = Ease.Apply(keyframe.RotationEasing, before.Rotation, keyframe.Rotation, t);
                        float scale = Ease.Apply(keyframe.ScaleEasing, before.Scale, keyframe.Scale, t);
                        AnimationKeyframe newKeyframe = new AnimationKeyframe(before.Index + j + 1, position, rotation, scale);
                        keyframes.Add(newKeyframe);
                    }
                    keyframes.Add(keyframe);
                }
                else
                {
                    for (int j = 0; j < keyframe.Index; j++)
                    {
                        AnimationKeyframe newKeyframe = new AnimationKeyframe(j, keyframe);
                        keyframes.Add(newKeyframe);
                    }
                    keyframes.Add(keyframe);
                }
            }
            Animation.SetKeyframes(keyframes);
        }

        public void ResetKeyframes()
        {
            List<AnimationKeyframe> keyframes = [.. ButtonKeyframes.Values];
            Animation.SetKeyframes(keyframes);
        }

        public void OrderKeyframes(List<AnimationKeyframe> keyframes)
        {
            if (keyframes.Count > 1)
            {
                keyframes = [.. keyframes.OrderBy(k => k.Time)];
            }
        }

        public void Clear()
        {
            foreach (var button in ButtonKeyframes.Keys)
            {
                button.Delete();
            }
            ButtonKeyframes = [];
            KeyframeButtons = [];
        }

        public void ClearFull()
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