using OpenTK.Mathematics;

public class Bone
{
    public Bone RootBone;
    public Bone Parent;
    public readonly string Name;
    public List<Bone> Children = new List<Bone>();
    
    // Base values
    public BoneVertex Pivot;
    public BoneVertex End;

    public Matrix4 localTransform = Matrix4.Identity;
    public Matrix4 globalTransform = Matrix4.Identity;

    public Bone(Bone rootBone, string name)
    {
        RootBone = rootBone;
        Parent = rootBone;
        Pivot = new(this, (0, 0, 0));
        End = new(this, (0, 1, 0));
        Name = name;
    }

    /// <summary>
    /// Constructor for the root bone
    /// </summary>
    /// <param name="name"></param>
    public Bone(string name)
    {
        RootBone = this;
        Parent = this;
        Pivot = new(this, (0, 0, 0));
        End = new(this, (0, 1, 0));
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
        child.Parent = this;
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
        Pivot.Position = pivot;
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

        bone.Pivot = new(bone, Pivot.Position);
        bone.End = new(bone, End.Position);
        bone.localTransform = localTransform;
        bone.globalTransform = globalTransform;

        foreach (var child in Children)
        {
            bone.Children.Add(child.Copy(rootBone, child.Name));
        }

        return bone;
    }
}

public struct BoneSelection
{
    public bool PivotSelected;
    public bool EndSelected;
    public Vector2 PivotScreenPosition;
    public Vector2 EndScreenPosition;

    public BoneSelection(bool pivotSelected, bool endSelected, Vector2 pivotScreenPosition, Vector2 endScreenPosition)
    {
        PivotSelected = pivotSelected;
        EndSelected = endSelected;
        PivotScreenPosition = pivotScreenPosition;
        EndScreenPosition = endScreenPosition;
    }
}

public class BoneVertex
{
    public Bone Parent;
    public Vector3 Position;
    public Vector2 ScreenPosition;
    public bool Selected;

    public BoneVertex(Bone parent, Vector3 position)
    {
        Parent = parent;
        Position = position;
        ScreenPosition = (0, 0);
        Selected = false;
    }

    public bool IsEnd()
    {
        return Parent.End == this;
    }

    public bool IsPivot()
    {
        return Parent.Pivot == this;
    }
}