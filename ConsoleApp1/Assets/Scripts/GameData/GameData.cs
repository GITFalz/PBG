public static class GameData
{
    public static Dictionary<string, ModelSaveData> Models = [];

    public static void Add(ModelSaveData model)
    {
        if (Models.ContainsKey(model.Name))
            return;

        Models.Add(model.Name, model);
    }

    public static bool ContainsKey(string key)
    {
        return Models.ContainsKey(key);
    }

    public struct ModelSaveData
    {
        public string Name => Model.Name;
        public Model Model;
        public string Path;

        public ModelSaveData(Model model, string path)
        {
            Model = model;
            Path = path;
        }
    }
}