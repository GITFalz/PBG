using OpenTK.Windowing.GraphicsLibraryFramework;

public static class ModelManager
{
    public static Dictionary<string, Model> Models = [];

    public static void Update()
    {
        foreach (var (name, model) in Models)
        {
            if (model.IsShown)
            {
                model.Update(); 
            }
        }
    }

    public static void Render()
    {
        foreach (var (name, model) in Models)
        {
            if (model.IsShown)
            {
                model.Render(); 
            }
        }
    }

    public static void LoadModel(string name)
    {
        string fileName = name;
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a valid model name.");
            return;
        }

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        SelectedModel?.Unload();

        if (SelectedModel == null)
        {
            SelectedModel = new Model();
            Models.Add(name, SelectedModel);
        }

        SelectedModel.LoadModel(name);
        SelectedModel.Name = name;
        SelectedModel.IsSelected = true;
        SelectedModel.UpdateVertexPosition();

        /*
        Model model = new Model();
        model.LoadModel(name);
        model.SaveModel(newFile);
        model.Name = newFile;

        Models.Add(newFile, model);
        if (SelectedModel != null)
        {
            SelectedModel.IsSelected = false;
            SelectedModel.SelectedVertices.Clear();
            SelectedModel.GenerateVertexColor();
        }
        SelectedModel = model;
        SelectedModel.IsSelected = true;
        SelectedModel.UpdateVertexPosition();
        */
    }

    public static void SaveModel(string fileName)
    {
        SelectedModel?.SaveModel(fileName);
    }

    public static void DeleteModel(Model model)
    {
        Models.Remove(model.Name);
        if (SelectedModel == model)
        {
            SelectedModel = null;
        }
    }

    public static void Unload()
    {
        foreach (var (name, model) in Models)
        {
            model.Unload();
        }
    }
}