using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : BaseEditor
{
    public UIController BoneUi = new UIController();

    private bool freeCamera = false;
    private bool regenerateUi = false;
    private bool _started = false;
    public Model model;
    
    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Animation Editor");

        editor.model.SwitchState("Animation");
        
        if (!_started)
        {
            model = editor.model;
            BoneUi.Generate();
            _started = true;
        }
    }

    public override void Awake(GeneralModelingEditor editor)
    {

    }
    
    public override void Update(GeneralModelingEditor editor)
    {
        var links = editor.GetLinkPositions(model.Bones);

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
            
            if (!regenerateUi)
            {
                BoneUi.Clear();
                regenerateUi = true;
            }
        }
        else if (regenerateUi)
        {
            foreach (var pos in links)
            {
                var panel = editor.GeneratePanelLink(pos.A, pos.B, 12);
                BoneUi.AddElement(panel);
            }
        
            BoneUi.Generate();
            regenerateUi = false;
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderAnimation();
        BoneUi.Render();
    }
    

    public override void Exit(GeneralModelingEditor editor)
    {
        //editor.model.Mesh.InitModel();
        //editor.model.Mesh.UpdateMesh();
    }
}