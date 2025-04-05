public static class ModelRenderer
{
    public static List<Model> Models = [];
    public static Model? SelectedModel = null;

    public static EditMode CurrentEditMode = EditMode.Object;

    public static void Render()
    {

    }

    public static void RenderModels()
    {
        foreach (var model in Models)
        {
            model.Render();
        }
    }
}

public enum EditMode
{
    Object,
    Vertex
}