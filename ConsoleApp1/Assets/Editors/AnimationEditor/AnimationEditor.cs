using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    private bool freeCamera = false;
    public OldModel model;
    public AnimationMesh Mesh;
    public UIController Timeline;
    
    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;

        Console.WriteLine("Start Animation Editor");

        editor.model.SwitchState("Animation");

        model = editor.model;
        Mesh = model.animationMesh;

        Timeline = new UIController();
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        Timeline.Resize();
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        Mesh.LoadModel(editor.currentModelName);
        editor.model.SwitchState("Animation");

        UICollection timelineCollection = new("TimelineCollection", Timeline, AnchorType.ScaleBottom, PositionType.Absolute, (0, 0, 0), (1000, 200), (5, -5, 255, 5), 0);
        
        UIImage timelineBackground = new("TimelineBackground", Timeline, AnchorType.ScaleBottom, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1), (0, 0, 0), (1000, 200), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIScrollView boneTimelineCollection = new("BoneTimelineScrollView", Timeline, AnchorType.ScaleTop, PositionType.Relative, CollectionType.Vertical, (1000, 186), (7, 7, 7, 7));
        boneTimelineCollection.SetSpacing(0);

        foreach (Bone bone in model.Bones)
        {
            // Main bone collection inside the timeline
            UIHorizontalCollection boneCollection = new($"{bone.Name}_Collection", Timeline, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 40), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

            // Bone name
            UIText boneText = new($"{bone.Name}_Text", Timeline, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1), (0, 0, 0), (10, 10), (5, 0, 0, 0), 0);
            boneText.SetMaxCharCount(20).SetText(bone.Name, 0.6f);

            // The collection holds the bone name for alignment purposes.
            // Text scale is calculated when calling SetText so we need to use it for the collection scale.
            // (Scale info in UICollection class)
            UIDepthCollection boneNameCollection = new($"{bone.Name}_NameCollection", Timeline, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (boneText.Scale.X, 40), (0, 0, 0, 0), 0);

            // Background for the bone timeline newt to the bone name
            UIImage boneTimelineBackground = new($"{bone.Name}_Background", Timeline, AnchorType.TopLeft, PositionType.Relative, (0.4f, 0.4f, 0.4f, 1), (0, 0, 0), (1000, 40), (0, 0, 0, 0), 0, 1, (10, 0.05f));

            boneCollection.AddElements(boneNameCollection.AddElement(boneText), boneTimelineBackground);

            boneTimelineCollection.AddElement(boneCollection);
        }

        timelineCollection.AddElements(timelineBackground, boneTimelineCollection);
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
        Shader.Error("before animation render: ");
        editor.RenderAnimation();
        Shader.Error("after animation render: ");
        Console.WriteLine("after animation render: ");
        Timeline.RenderNoDepthTest();
    }
    

    public override void Exit(GeneralModelingEditor editor)
    {
        //editor.model.Mesh.InitModel();
        //editor.model.Mesh.UpdateMesh();
        Timeline.Clear();
    }
}