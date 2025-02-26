using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    private bool freeCamera = false;
    private bool regenerateUi = false;
    private bool _started = false;
    public Model model;
    public AnimationMesh Mesh;
    public UIController Timeline;
    
    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Animation Editor");

        editor.model.SwitchState("Animation");

        if (_started)
            return;
        
        model = editor.model;
        Mesh = model.animationMesh;

        Timeline = new UIController();

        _started = true;
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        Timeline.OnResize();
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        Mesh.LoadModel(editor.currentModelName);
        editor.model.SwitchState("Animation");

        UIMesh uiMesh = Timeline.uIMesh;
        UIMesh maskMesh = Timeline.maskMesh;
        UIMesh maskeduIMesh = Timeline.maskeduIMesh;

        TextMesh maskedTextMesh = Timeline.maskedTextMesh;

        UICollection timelineCollection = new("TimelineCollection", AnchorType.ScaleBottom, PositionType.Absolute, (0, 0, 0), (1000, 200), (5, -5, 255, 5), 0);
        
        UIImage timelineBackground = new("TimelineBackground", AnchorType.ScaleBottom, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (1000, 200), (0, 0, 0, 0), 0, 1, (10, 0.05f), uiMesh);
        UIScrollView boneTimelineCollection = new("BoneTimelineScrollView", AnchorType.ScaleTop, PositionType.Relative, CollectionType.Vertical, (1000, 186), (7, 7, 7, 7), maskMesh);
        boneTimelineCollection.SetSpacing(0);

        foreach (Bone bone in model.Bones)
        {
            // Main bone collection inside the timeline
            UIHorizontalCollection boneCollection = new($"{bone.Name}_Collection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 40), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

            // Bone name
            UIText boneText = new($"{bone.Name}_Text", AnchorType.MiddleLeft, PositionType.Relative, (0, 0, 0), (10, 10), (5, 0, 0, 0), 0, 1, (10, 0.05f), maskedTextMesh);
            boneText.SetMaxCharCount(20).SetText(bone.Name, 0.6f);

            // The collection holds the bone name for alignment purposes.
            // Text scale is calculated when calling SetText so we need to use it for the collection scale.
            // (Scale info in UICollection class)
            UIDepthCollection boneNameCollection = new($"{bone.Name}_NameCollection", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (boneText.Scale.X, 40), (0, 0, 0, 0), 0);

            // Background for the bone timeline newt to the bone name
            UIImage boneTimelineBackground = new($"{bone.Name}_Background", AnchorType.TopLeft, PositionType.Relative, (0.4f, 0.4f, 0.4f), (0, 0, 0), (1000, 40), (0, 0, 0, 0), 0, 1, (10, 0.05f), maskeduIMesh);

            boneCollection.AddElement(boneNameCollection.AddElement(boneText), boneTimelineBackground);

            boneTimelineCollection.AddElement(boneCollection);
        }

        timelineCollection.AddElement(timelineBackground, boneTimelineCollection);
        Timeline.AddElement(timelineCollection);
        Timeline.GenerateBuffers();
    }
    
    public override void Update(GeneralModelingEditor editor)
    {
        Timeline.Test();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            freeCamera = !freeCamera;
            
            if (freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.firstMove = true;
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
            }
        }

        if (freeCamera)
        {
            Game.camera.Update();
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderAnimation();
        Timeline.Render();
    }
    

    public override void Exit(GeneralModelingEditor editor)
    {
        //editor.model.Mesh.InitModel();
        //editor.model.Mesh.UpdateMesh();
        Timeline.Clear();
    }
}