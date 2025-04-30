using OpenTK.Mathematics;

public abstract class BaseEditor 
{ 
    public bool Started = false;
    public GeneralModelingEditor Editor;
    public string FileName => GeneralModelingEditor.FileName.Text;

    public bool blocked = false;

    public BaseEditor(GeneralModelingEditor editor)
    {
        Editor = editor;
    }

    public abstract void Start(GeneralModelingEditor editor);
    public abstract void Resize(GeneralModelingEditor editor);
    public abstract void Awake(GeneralModelingEditor editor);
    public abstract void Update(GeneralModelingEditor editor);
    public abstract void Render(GeneralModelingEditor editor);
    public abstract void Exit(GeneralModelingEditor editor);
}

public class Link<T>(T a, T b) where T : struct
{
    public T A = a;
    public T B = b;
}

public class VertexPanel
{
    public UIPanel Panel;
    public Vector2 ScreenPosition;

    public VertexPanel(UIPanel panel, Vector2 screenPosition)
    {
        Panel = panel;
        ScreenPosition = screenPosition;
    }
}