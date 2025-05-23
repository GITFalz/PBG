public static class GameData
{
    public static Dictionary<string, Model> Models = [];

    public static void Add(Model model)
    {
        if (Models.ContainsKey(model.Name))
            return;
            
        Models.Add(model.Name, model);
    }
}