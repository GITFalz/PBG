using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public static class RigManager
{
    public static Dictionary<string, Rig> Rigs = [];
    public static bool DisplayError = true;

    public static bool Add(Rig rig)
    {
        if (Rigs.ContainsKey(rig.Name))
        {
            if (DisplayError) PopUp.AddPopUp("Rig already exists");
            return false;
        }

        Rigs.Add(rig.Name, rig);
        if (DisplayError) PopUp.AddPopUp("Rig added");
        return true;
    }

    public static void Update(Rig rig)
    {
        if (Rigs.ContainsKey(rig.Name))
        {
            Rigs[rig.Name] = rig;
        }
        else
        {
            PopUp.AddPopUp("Rig not found");
        }
    }

    public static bool Remove(string name)
    {
        if (!Rigs.ContainsKey(name))
        {
            if (DisplayError) PopUp.AddPopUp("Rig not found");
            return false;
        }

        Rigs.Remove(name);
        if (DisplayError) PopUp.AddPopUp("Rig removed");
        return true;
    }

    public static bool TryGet(string name, [NotNullWhen(true)] out Rig? rig)
    {
        if (Rigs.TryGetValue(name, out rig))
            return true;

        if (DisplayError) PopUp.AddPopUp("Rig not found");
        return false;
    }

    public static bool ChangeName(string oldName, string newName)
    {
        if (!Rigs.ContainsKey(oldName))
        {
            if (DisplayError) PopUp.AddPopUp("Old rig name not found");
            return false;
        }

        if (Rigs.ContainsKey(newName))
        {
            if (DisplayError) PopUp.AddPopUp("Rig name already exists");
            return false;
        }

        Rigs.Add(newName, Rigs[oldName]);
        Rigs.Remove(oldName);
        if (DisplayError) PopUp.AddPopUp("Rig name changed");
        return true;
    }

    #region Load
    public static bool Load(string name)
    {
        return Load(name, Game.rigPath);
    }

    public static bool Load(string name, string path)
    {
        path = Path.Combine(path, name + ".rig");
        return LoadFromPath(path);
    }
    
    public static bool LoadFromPath(string path)
    {
        return LoadFromPath(path, out _);
    }
    
    public static bool LoadFromPath(string path, [NotNullWhen(true)] out Rig? rig)
    {
        rig = null;
        if (!File.Exists(path))
        {
            if (DisplayError)
            {   
                Console.WriteLine("Rig file does not exist: " + path);
                PopUp.AddPopUp("Rig file does not exist");
            }
            return false;
        }

        string name = Path.GetFileNameWithoutExtension(path);
        if (Rigs.ContainsKey(name)) // Quietly ignore if the rig already exists
        {
            DisplayError = false; // Disable error display for this operation
            Remove(name);
            DisplayError = true; // Re-enable error display
        }

        if (!LoadRig(name, path, out rig))
        {
            if (DisplayError)
            {
                Console.WriteLine("Failed to load rig from path: " + path);
                PopUp.AddPopUp("Rig failed to load");
            }
            return false;
        }

        Add(rig);
        if (DisplayError)
        {
            PopUp.AddPopUp("Rig loaded from path");
        }
        return true;
    }

    private static bool LoadRig(string name, string path, [NotNullWhen(true)] out Rig? rig)
    {
        rig = null;
        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 0)
            return false;

        int count = Int.Parse(lines[0]);
        if (count == 0)
            return false;

        rig = new Rig(name);
        Dictionary<string, Bone> bones = [];

        for (int i = 1; i < lines.Length; i += 5)
        {
            string boneName;
            string parentName;
            Vector3 position;
            Quaternion rotation;
            float scale;

            string[] values = lines[i].Split(' ');
            if (values.Length != 2)
            {
                PopUp.AddPopUp("Invalid bone name at line " + i);
                return false;
            }
            boneName = values[1];

            values = lines[i + 1].Split(' ');
            if (values.Length != 2)
            {
                PopUp.AddPopUp("Invalid parent name at line " + (i + 1));
                return false;
            }
            parentName = values[1];

            values = lines[i + 2].Split(' ');
            if (values.Length != 4)
            {
                PopUp.AddPopUp("Invalid position at line " + (i + 2));
                return false;
            }
            position = new Vector3(
                Float.Parse(values[1].Trim(',')),
                Float.Parse(values[2].Trim(',')),
                Float.Parse(values[3].Trim(','))
            );

            values = lines[i + 3].Split(' ');
            if (values.Length != 5)
            {
                PopUp.AddPopUp("Invalid rotation at line " + (i + 3));
                return false;
            }
            rotation = new Quaternion(
                Float.Parse(values[1].Trim(',')),
                Float.Parse(values[2].Trim(',')),
                Float.Parse(values[3].Trim(',')),
                Float.Parse(values[4].Trim(','))
            );

            values = lines[i + 4].Split(' ');
            if (values.Length != 2)
            {
                PopUp.AddPopUp("Invalid scale at line " + (i + 4));
                return false;
            }
            scale = Float.Parse(values[1].Trim(','));

            if (boneName == parentName) // This means it is the root bone
            {
                RootBone root = new RootBone(boneName);
                root.Position = position;
                root.Rotation = rotation;
                root.Scale = scale;
                rig.RootBone = root;
                bones.Add(boneName, rig.RootBone);
            }
            else
            {
                if (!bones.TryGetValue(parentName, out Bone? parent))
                {
                    PopUp.AddPopUp("Parent bone not found: " + parentName + " at line " + (i + 1));
                    return false;
                }

                ChildBone child = new ChildBone(boneName, parent);
                child.Position = position;
                child.Rotation = rotation;
                child.Scale = scale;
                bones.Add(boneName, child);
            }
        }

        rig.Create();
        rig.Initialize();
        rig.RootBone.UpdateGlobalTransformation();
        return true;
    }
    #endregion
}