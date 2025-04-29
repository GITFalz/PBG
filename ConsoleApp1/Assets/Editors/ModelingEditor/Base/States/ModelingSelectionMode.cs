using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingSelectionMode : ModelingBase
{   
    public ModelingSelectionMode(ModelingEditor editor) : base(editor) {}

    public override void Start()
    {
        
    }

    public override void Update()
    {
        if (Model == null)
            return;

        if (Input.IsKeyPressed(Keys.Delete))
        {
            Model.Delete();
        }
    }

    public override void Render()
    {
        
    }

    public override void Exit()
    {
        
    }
}