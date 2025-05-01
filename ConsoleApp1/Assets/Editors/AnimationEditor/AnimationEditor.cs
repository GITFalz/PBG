using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    private bool freeCamera = false;
    public UIController Timeline;

    public AnimationEditor(GeneralModelingEditor editor) : base(editor)
    {
        // Constructor logic here
    }
    
    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;

        Console.WriteLine("Start Animation Editor");

        Timeline = new UIController();
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        Timeline.Resize();
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        UICollection timelineCollection = new("TimelineCollection", Timeline, AnchorType.ScaleBottom, PositionType.Absolute, (0, 0, 0), (1000, 200), (5, -5, 255, 5), 0);
        
        UIImage timelineBackground = new("TimelineBackground", Timeline, AnchorType.ScaleBottom, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1), (0, 0, 0), (1000, 200), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        UIScrollView boneTimelineCollection = new("BoneTimelineScrollView", Timeline, AnchorType.ScaleTop, PositionType.Relative, CollectionType.Vertical, (1000, 186), (7, 7, 7, 7));
        boneTimelineCollection.SetSpacing(0);

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
                Game.camera.Unlock();
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
                Game.camera.Lock();
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