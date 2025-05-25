public static class GameData
{
    public static Dictionary<string, ModelSaveData> Models = [];
    public static Dictionary<string, RigSaveData> Rigs = [];
    public static Dictionary<string, AnimationSaveData> Animations = [];

    public static void Add(ModelSaveData model)
    {
        if (Models.ContainsKey(model.Name))
            return;

        Models.Add(model.Name, model);
    }

    public static void Add(RigSaveData rig)
    {
        if (Rigs.ContainsKey(rig.Name))
            return;

        Rigs.Add(rig.Name, rig);
    }

    public static void Add(AnimationSaveData animation)
    {
        if (Animations.ContainsKey(animation.Name))
            return;

        Animations.Add(animation.Name, animation);
    }

    public static bool ContainsModel(string key)
    {
        return Models.ContainsKey(key);
    }

    public static bool ContainsRig(string key)
    {
        return Rigs.ContainsKey(key);
    }

    public static bool ContainsAnimation(string key)
    {
        return Animations.ContainsKey(key);
    }

    public static Model? GetModel(string name)
    {
        if (Models.TryGetValue(name, out ModelSaveData? modelData))
        {
            return modelData.Model;
        }
        return null;
    }

    public static Rig? GetRig(string name)
    {
        if (Rigs.TryGetValue(name, out RigSaveData? rigData))
        {
            return rigData.Rig;
        }
        return null;
    }

    public static Animation? GetAnimation(string name)
    {
        if (Animations.TryGetValue(name, out AnimationSaveData? animationData))
        {
            return animationData.Animation;
        }
        return null;
    }

    public static List<string> SaveLines()
    {
        List<string> lines = [];
        foreach (var animation in Animations.Values)
        {
            lines.AddRange(animation.GetSaveLines());
        }
        foreach (var rig in Rigs.Values)
        {
            lines.AddRange(rig.GetSaveLines());
        }
        foreach (var model in Models.Values)
        {
            lines.AddRange(model.GetSaveLines());
        }
        return lines;
    }

    public abstract class SaveData
    {
        public string Path;
        public SaveData(string path) { Path = path; }
        public abstract string Name { get; }
        public abstract List<string> GetSaveLines();
    }

    public class ModelSaveData : SaveData
    {
        public override string Name => Model.Name;
        public Model Model;
        public ModelSaveData(Model model, string path) : base(path) { Model = model; }
        public override List<string> GetSaveLines()
        {
            List<string> lines = [];
            lines.Add("Model");
            lines.Add("{");
            lines.Add($"    Name: {Model.Name}");
            lines.Add($"    Path: {Path}");
            lines.Add($"    Position: {Model.Position.X} {Model.Position.Y} {Model.Position.Z}");
            lines.Add($"    Rotation: {Model.Rotation.X} {Model.Rotation.Y} {Model.Rotation.Z}");
            lines.Add($"    Rig: {Model.RigName ?? "null"}");
            if (Model.AnimationManager != null) // for now there is only one animation
                lines.AddRange(Model.AnimationManager.GetAnimations());
            else
                lines.Add("    Animation: null");
            lines.Add("}");
            return lines;
        }
    }

    public class RigSaveData : SaveData
    {
        public override string Name => Rig.Name;
        public Rig Rig;
        public RigSaveData(Rig rig, string path) : base(path) { Rig = rig; }
        public override List<string> GetSaveLines()
        {
            List<string> lines = [];
            lines.Add("Rig");
            lines.Add("{");
            lines.Add($"    Name: {Rig.Name}");
            lines.Add($"    Path: {Path}");
            lines.Add("}");
            return lines;
        }
    }

    public class AnimationSaveData : SaveData
    {
        public override string Name => Animation.Name;
        public Animation Animation;
        public AnimationSaveData(Animation animation, string path) : base(path) { Animation = animation; }
        public override List<string> GetSaveLines()
        {
            List<string> lines = [];
            lines.Add("Animation");
            lines.Add("{");
            lines.Add($"    Name: {Animation.Name}");
            lines.Add($"    Path: {Path}");
            lines.Add("}");
            return lines;
        }
    }
}