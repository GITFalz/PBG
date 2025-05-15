using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RiggingEditor : BaseEditor
{
    public UIController ModelingUi;

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText AxisText;

    public List<Vertex> SelectedVertices = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    private bool regenerateVertexUi = true;

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

        mainPanelStacking.AddElements(cullingCollection, alphaCollection);

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
    }
    
    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();

        ModelingUi.RenderDepthTest();
    }

    public override void Update(GeneralModelingEditor editor)
    {
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
                Model?.UpdateVertexPosition();
            }
        }
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        if (Model == null)
            return;
            
        Model.RenderBones = false;
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
}