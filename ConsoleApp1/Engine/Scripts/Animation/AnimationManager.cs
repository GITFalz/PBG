using System.Diagnostics.CodeAnalysis;

public class AnimationManager
{
    public static Dictionary<string, Animation> Animations = [];

    public static bool Add(Animation animation, bool displayError = true)
    {
        if (Animations.ContainsKey(animation.Name))
        {
            if (displayError) PopUp.AddPopUp("Animation already exists");
            return false;
        }

        Animations.Add(animation.Name, animation);
        if (displayError) PopUp.AddPopUp("Animation added");
        return true;
    }

    public static void Update(Animation animation)
    {
        if (Animations.ContainsKey(animation.Name))
        {
            Animations[animation.Name] = animation;
        }
        else
        {
            PopUp.AddPopUp("Animation not found");
        }
    }

    public static bool Remove(string name, bool displayError = true)
    {
        if (!Animations.ContainsKey(name))
        {
            if (displayError) PopUp.AddPopUp("Animation not found");
            return false;
        }

        Animations.Remove(name);
        if (displayError) PopUp.AddPopUp("Animation removed");
        return true;
    }

    public static bool TryGet(string name, [NotNullWhen(true)] out Animation? animation, bool displayError = true)
    {
        if (Animations.TryGetValue(name, out animation))
            return true;

        if (displayError) PopUp.AddPopUp("Animation not found");
        return false;
    }

    public static bool ChangeName(string oldName, string newName, bool displayError = true)
    {
        if (!Animations.ContainsKey(oldName))
        {
            if (displayError) PopUp.AddPopUp("Old Animation name not found");
            return false;
        }

        if (Animations.ContainsKey(newName))
        {
            if (displayError) PopUp.AddPopUp("Animation name already exists");
            return false;
        }

        Animations.Add(newName, Animations[oldName]);
        Animations.Remove(oldName);
        if (displayError) PopUp.AddPopUp("Animation name changed");
        return true;
    }

    #region Load
    public static bool Load(string name)
    {
        return Load(name, Game.animationPath);
    }

    public static bool Load(string name, string path)
    {
        path = Path.Combine(path, name + ".anim");
        return LoadFromPath(path);
    }

    public static bool LoadFromPath(string path)
    {
        return LoadFromPath(path, out _);
    }

    public static bool LoadFromPath(string path, [NotNullWhen(true)] out Animation? animation)
    {
        animation = null;
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("Animation file does not exist");
            return false;
        }

        string name = Path.GetFileNameWithoutExtension(path);
        if (Animations.ContainsKey(name)) // Quietly ignore if the rig already exists
        {
            Remove(name, false);
        }

        if (!LoadAnimation(name, path, out animation))
        {
            PopUp.AddPopUp("Animation failed to load");
            return false;
        }

        Add(animation);
        PopUp.AddPopUp("Animation loaded from path");
        return true;
    }

    private static bool LoadAnimation(string name, string path, [NotNullWhen(true)] out Animation? animation)
    {
        if (!AnimationParser.Parse(name, File.ReadAllLines(path), out animation))
        {
            animation = null;
            return false;
        }
        return true;
    }
    #endregion
}