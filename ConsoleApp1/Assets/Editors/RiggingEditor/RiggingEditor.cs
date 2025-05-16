using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RiggingEditor : BaseEditor
{
    public UIController ModelingUi;
    public Camera Camera => Game.camera;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public static UIInputFieldPrefab BoneNameText;


    public List<Vertex> SelectedVertices = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public List<BonePivot> SelectedBonePivots = new();
    public Dictionary<BonePivot, (Vector2, float)> BonePivots = new Dictionary<BonePivot, (Vector2, float)>();

    private bool regenerateVertexUi = true;

    
    private Vector3 _endPosition = Vector3.Zero;
    

    public RiggingEditor(GeneralModelingEditor editor) : base(editor)
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
        alphaButton.SetOnClick(() => { blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);


        // Bone info panel collection
        UICollection boneInfoCollection = new("BoneInfoCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 45), (0, 0, 0, 0), 0);

        UICollection boneTextCollection = new("BoneTextCollection", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);
        
        UIText boneNameText = new("BoneNameText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 45), (0, 0, 0, 0), 0);
        boneNameText.SetTextCharCount("Name:", 1.2f);

        UITextButton boneSetNameButton = new("BoneSetNameButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        boneSetNameButton.SetTextCharCount("Set", 1f);
        boneSetNameButton.SetOnClick(() => {
            var bones = GetSelectedBones();
            if (bones.Count != 1)
                return;

            Console.WriteLine("Set bone name");
            var bone = bones.First();
            bone.SetName(BoneNameText.InputField.Text);
            BoneNameText.SetText(bone.Name, 1.2f).UpdateCharacters();
        });

        boneTextCollection.AddElements(boneNameText, boneSetNameButton.Collection);

        BoneNameText = new UIInputFieldPrefab("BoneNameText", ModelingUi, (0, 0, 0, 0), (0.5f, 0.5f, 0.5f, 1f), 1, 21, "BoneName", TextType.Alphanumeric);
        BoneNameText.SetText("BoneName", 1f);
        BoneNameText.UpdateScaling();
        BoneNameText.Collection.SetAnchorType(AnchorType.BottomLeft);

        boneInfoCollection.AddElements(boneTextCollection, BoneNameText.Collection);


        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);
        
        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        
        UIInputField CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));
        
        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; } }); 

        speedStacking.SetScale((45, 30f));
        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);

        mainPanelStacking.AddElements(cullingCollection, alphaCollection, boneInfoCollection);

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

    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;


        Console.WriteLine("Start Rigging Editor");
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        ModelingUi.Resize();
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        if (Model == null)
            return;

        Model.RenderBones = true;
        Model.SetStaticRig("Test");

        Model.SetAnimation();
        UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
    }
    
    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();

        ModelingUi.RenderDepthTest();
    }

    public override void Update(GeneralModelingEditor editor)
    {
        if (Model == null)
            return;

        ModelingUi.Update();

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
                UpdateBonePosition(Game.camera.ProjectionMatrix, Game.camera.ViewMatrix);
                Model.UpdateVertexPosition();
            }
        }

        RigUpdate();
    }

    private void RigUpdate()
    {
        if (Input.IsMousePressed(MouseButton.Left))
            TestBonePosition();

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
            UpdateBonePosition(Camera.ProjectionMatrix, Camera.ViewMatrix);
        }
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        if (Model == null)
            return;

        Model.RenderBones = false;
        Model.SetStaticRig(null);
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
        Model.Mesh.UpdateRigVertexPosition();

        foreach (var bone in Model.Rig.BonesList)
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