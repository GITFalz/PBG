using OpenTK.Mathematics;

public class Bone
{
    public Bone RootBone;
    public readonly string Name;
    public List<Bone> Children = new List<Bone>();
    
    // Base values
    public Vector3 Pivot = Vector3.Zero;
    public Vector3 End = (0, 2, 0);

    public Matrix4 localTransform;
    public Matrix4 globalTransform;

    public Bone(Bone rootBone, string name)
    {
        RootBone = rootBone;
        Name = name;
    }

    /// <summary>
    /// Constructor for the root bone
    /// </summary>
    /// <param name="name"></param>
    public Bone(string name)
    {
        RootBone = this;
        Name = name;
    }

    public Bone AddChild(Bone child)
    {
        var names = RootBone.GetChildNames(); 
        string newName = child.Name;
        while (names.Contains(newName))
        {
            newName += $"{names.Count}";
        }
        child = child.Copy(RootBone, newName);
        Children.Add(child);
        return child;
    }

    public bool IsRoot()
    {
        return RootBone == this;
    }

    public List<string> GetChildNames()
    {
        List<string> names = [Name];
        foreach (var child in Children)
        {
            names.AddRange(child.GetChildNames());
        }
        return names;
    }

    public void SetPivot(Vector3 pivot)
    {
        Pivot = pivot;
    }

    public void CalculateGlobalTransform()
    {
        globalTransform = IsRoot() ? localTransform : localTransform * RootBone.globalTransform;
        foreach (var child in Children) {
            child.CalculateGlobalTransform();
        }
    }

    public List<Bone> GetBones()
    {
        List<Bone> bones = [this];
        foreach (var child in Children)
        {
            bones.AddRange(child.GetBones());
        }
        return bones;
    }

    private Bone Copy(Bone rootBone, string name)
    {
        Bone bone = new(rootBone, name);

        bone.Pivot = Pivot;
        bone.End = End;
        bone.localTransform = localTransform;
        bone.globalTransform = globalTransform;

        foreach (var child in Children)
        {
            bone.Children.Add(child.Copy(rootBone, child.Name));
        }

        return bone;
    }


    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Bone bone = (Bone) obj;
        return Name == bone.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}

public struct BoneSelection
{
    public bool PivotSelected;
    public bool EndSelected;

    public BoneSelection(bool pivotSelected, bool endSelected)
    {
        PivotSelected = pivotSelected;
        EndSelected = endSelected;
    }
}