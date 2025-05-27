using OpenTK.Mathematics;

public abstract class BaseEditor(GeneralModelingEditor editor)
{ 
    public GeneralModelingEditor Editor = editor;
    public Model? Model => ModelManager.SelectedModel;

    public bool Started = false;
    public bool blocked = false;
    public string FileName => GeneralModelingEditor.FileName.Text;

    public abstract void Start();
    public abstract void Resize();
    public abstract void Awake();
    public abstract void Update();
    public abstract void Render();
    public abstract void Exit();
}