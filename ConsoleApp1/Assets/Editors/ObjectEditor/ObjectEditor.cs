public class ObjectEditor : BaseEditor
{
    public ObjectEditor(GeneralModelingEditor editor) : base(editor)
    {
        // Constructor logic here
    }
    
    public override void Awake(GeneralModelingEditor editor)
    {
        throw new NotImplementedException();
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        throw new NotImplementedException();
    }

    public override void Render(GeneralModelingEditor editor)
    {
        throw new NotImplementedException();
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        throw new NotImplementedException();
    }

    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;
        /*
        Console.WriteLine("Start Modeling Editor");

        if (_started)
        {
            return;
        }

        Editor = editor;
        Mesh = editor.model.modelMesh;

        _started = true;
        */
    }

    public override void Update(GeneralModelingEditor editor)
    {
        throw new NotImplementedException();
    }
}