using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class ModelManager
{
    public static Dictionary<string, Model> Models = [];

    public static Model? SelectedModel = null;
    public static Dictionary<string, SelectedModelData> SelectedModels = [];
    public struct SelectedModelData
    {
        public UIButton Button;
        public Model Model;

        public SelectedModelData(UIButton button, Model model)
        {
            Button = button;
            Model = model;
        }
    }

    public static void Update()
    {
        if (Input.IsMousePressed(MouseButton.Left))
        {
            if (SelectedModel != null)
            {
                SelectedModel.IsSelected = false;
                SelectedModel.SelectedVertices.Clear();
                SelectedModel.GenerateVertexColor();
            }
            else
            {

            }
        }

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
        GL.CullFace(TriangleFace.Back);

        foreach (var model in Models.Values)
        {
            if (model.IsShown)
            {
                model.Render();
            }
        }
        
        GL.DepthFunc(DepthFunction.Lequal);
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
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string newFile = fileName;

        while (Models.ContainsKey(newFile))
        {
            newFile = $"{newFile}_{Models.Count}";
        }

        Model model = new Model();
        if (!model.LoadModel(name))
            return;

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
    }

    public static void LoadModelFromPath(string path)
    {
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("File does not exist.");
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(path);
        string newFile = fileName;

        while (Models.ContainsKey(newFile))
        {
            newFile = $"{newFile}_{Models.Count}";
        }

        Model model = new Model();
        if (!model.LoadModelFromPath(path))
            return;

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
    }

    public static void Select(Model model)
    {
        if (SelectedModel != null)
        {
            SelectedModel.IsSelected = false;
            SelectedModel.SelectedVertices.Clear();
            SelectedModel.GenerateVertexColor();
        }

        SelectedModel = model;
        if (SelectedModel != null)
        {
            SelectedModel.IsSelected = true;
            SelectedModel.UpdateVertexPosition();
        }
    }

    public static void UnSelect(Model model)
    {
        if (SelectedModel == model)
        {
            SelectedModel.IsSelected = false;
            SelectedModel.SelectedVertices.Clear();
            SelectedModel.GenerateVertexColor();
            SelectedModel = null;
        }
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