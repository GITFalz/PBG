using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    // === UI COMPONENTS ===
    public UIController ModelingUi;
    public UIController TimelineUI;
    public UIController KeyframeUI;
    public UIController TimerUI;

    public UIInputFieldPrefab TimelineTimeField;
    public UIScrollView TimelineScrollView;

    public UICollection KeyframePanelCollection;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    // === DATA CONTAINERS ===
    public List<Vertex> SelectedVertices = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public List<BonePivot> SelectedBonePivots = new();
    public Dictionary<BonePivot, (Vector2, float)> BonePivots = new Dictionary<BonePivot, (Vector2, float)>();

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
    private bool _generate = false;

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
        alphaButton.SetOnClick(() => { blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);



        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));

        UIInputField CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));

        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = Int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; } });

        speedStacking.SetScale((45, 30f));
        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);

        mainPanelStacking.AddElements(cullingCollection, alphaCollection);

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


        KeyframeUI = new UIController("KeyframeUI");

        UICollection mainKeyframePanelCollection = new("MainKeyframePanelCollection", KeyframeUI, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        KeyframePanelCollection = new("KeyframePanelCollection", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        KeyframeUI.AddElement(KeyframePanelCollection);



        TimerUI = new UIController("TimerCollection");

        UICollection timerPanelCollection = new("TimerPanelCollection", TimerUI, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (5, 10, 0, 0), 0);

        UIButton timelineIndexButton = new("TimelineIndexButton", TimerUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (12, 12), (0, 0, 0, 0), 0, 10, (5, 0.05f), UIState.Interactable) { Depth = 3f };
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

    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;

        Console.WriteLine("Start Rigging Editor");

        if (Model == null)
            return;

        Model.Animation = Animation;
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        ModelingUi.Resize();
        TimelineUI.Resize();
        KeyframeUI.Resize();
        TimerUI.Resize();
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        ModelSettings.WireframeVisible = false;
        if (Model == null)
            return;

        Console.WriteLine("Awake Animation Editor");

        Model.RenderBones = true;
        Model.SetAnimationRig();

        Model.SetAnimation();

        if (Model.Animation == null)
            Model.Animation = new Animation("Test");

        Animation = Model.Animation;
        Animation.Clear();

        TimelineScrollView.ScrollPosition = 0;
        BoneAnimations = [];

        if (Model.Rig != null)
        {
            int i = 0;
            foreach (var bone in Model.Rig.BonesList)
            {
                Animation.AddBoneAnimation(bone.Name, out var boneAnimation);
                TimelineBoneAnimation timelineAnimation = new TimelineBoneAnimation(bone.Name, boneAnimation);
                timelineAnimation.Index = i;
                
                UITextButton boneTextButton = new UITextButton(bone.Name, TimelineUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (200, 20), (0, 0, 0, 0), 0, 10, (10, 0.05f));
                boneTextButton.SetTextCharCount(bone.Name, 1f);
                TimelineScrollView.AddElement(boneTextButton.Collection);
                BoneAnimations.Add(bone.Name, timelineAnimation);

                AnimationKeyframe keyframe = new AnimationKeyframe(i, bone);
                boneAnimation.AddOrUpdateKeyframe(keyframe);

                UIButton keyframeButton = new UIButton("KeyframeButton", KeyframeUI, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (0, i * 25, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
                KeyframePanelCollection.AddElement(keyframeButton);

                if (_generate)
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

        if (_generate)
        {
            TimelineUI.AddElement(TimelineScrollView.SubElements);
            TimelineScrollView.ResetInit();
            TimelineScrollView.Align();
            TimelineScrollView.UpdateScale();
            KeyframePanelCollection.Align();
        }

        Info.RenderInfo = false;
        Handle_BoneMovement();
        regenerateVertexUi = true;
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();

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


    public override void Update(GeneralModelingEditor editor)
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
            editor.freeCamera = !editor.freeCamera;

            if (editor.freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.Unlock();
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
                Game.camera.Lock();
                Model?.UpdateVertexPosition();
                UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
            }
        }

        if (!editor.freeCamera)
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

        if (Input.IsKeyDown(Keys.G))
        {
            Handle_BoneMovement();
        }

        if (Input.IsKeyReleased(Keys.G))
        {
            Game.SetCursorState(CursorState.Normal);
            regenerateVertexUi = true;
        }

        if (regenerateVertexUi)
        {
            UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
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

    public void UpdateBonePosition(Matrix4 projection, Matrix4 view)
    {
        if (Model == null || Model.Rig == null)
            return;

        BonePivots.Clear();

        foreach (var (_, bone) in Model.Rig.Bones)
        {
            Vector3 pivot = bone.Pivot.Get;
            Vector3 end = bone.End.Get;

            Vector2? screenPos1 = Mathf.WorldToScreen(pivot, Mathf.ToNumerics(projection), Mathf.ToNumerics(view));
            Vector2? screenPos1Side = Mathf.WorldToScreen(pivot + Game.camera.right.Normalized() * 0.3f, Mathf.ToNumerics(projection), Mathf.ToNumerics(view));
            Vector2? screenPos2 = Mathf.WorldToScreen(end, Mathf.ToNumerics(projection), Mathf.ToNumerics(view));
            Vector2? screenPos2Side = Mathf.WorldToScreen(end + Game.camera.right.Normalized() * 0.2f, Mathf.ToNumerics(projection), Mathf.ToNumerics(view));
            if (screenPos1 != null && screenPos1Side != null)
                BonePivots.Add(bone.Pivot, (screenPos1.Value, Vector2.Distance(screenPos1.Value, screenPos1Side.Value)));

            if (screenPos2 != null && screenPos2Side != null)
                BonePivots.Add(bone.End, (screenPos2.Value, Vector2.Distance(screenPos2.Value, screenPos2Side.Value)));
        }
    }

    public void TestBonePosition()
    {
        if (Model == null)
            return;

        if (!Input.IsKeyDown(Keys.LeftShift))
            SelectedBonePivots.Clear();

        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        BonePivot? closestBone = null;

        foreach (var (pivot, (position, radius)) in BonePivots)
        {
            float distance = Vector2.Distance(mousePos, position);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);

            if (distance < distanceClosest && distance < radius)
            {
                closest = position;
                closestBone = pivot;
            }
        }

        if (closestBone != null && !SelectedBonePivots.Remove(closestBone))
            SelectedBonePivots.Add(closestBone);

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
        HashSet<Bone> seenBones = [];
        foreach (var (pivot, _) in BonePivots)
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

    public override void Exit(GeneralModelingEditor editor)
    {
        TimelineScrollView.DeleteSubElements();
        TimelineUI.Update();

        if (Model == null)
            return;

        Model.RenderBones = false;
        Model.Rig?.Delete();
        Model.SetAnimationRig();

        foreach (var boneAnimation in BoneAnimations)
        {
            boneAnimation.Value.Clear();
        }
        KeyframeUI.Update();

        Info.RenderInfo = true;

        Playing = false;
        _generate = true;
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
            UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
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