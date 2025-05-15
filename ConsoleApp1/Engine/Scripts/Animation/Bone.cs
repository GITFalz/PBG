using OpenTK.Mathematics;

public abstract class Bone
{
    public string Name;
    public List<ChildBone> Children = [];


    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public float Scale = 1;

    // Computed at bind time (static)
    public Matrix4 BindPoseMatrix;
    public Matrix4 InverseBindMatrix;

    // Computed at runtime (updated each frame)
    public Matrix4 LocalAnimatedMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);   

    public Matrix4 GlobalAnimatedMatrix;

    public Matrix4 FinalMatrix => GlobalAnimatedMatrix;

    public Bone(string name) { Name = name; }
    
    public abstract void UpdateGlobalTransformation();
    public abstract string GetFullName();

    public void SetBindPose()
    {
        BindPoseMatrix = GlobalAnimatedMatrix;
        InverseBindMatrix = Matrix4.Invert(BindPoseMatrix);
    }

    public bool Add(ChildBone child)
    {
        if (Children.Contains(child))
            return false;

        string fullName = child.GetFullName();
        string newName = fullName;
        bool sameName = false;
        int cycle = 1;
        while (!sameName)
        {
            sameName = true;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].GetFullName() == fullName)
                {
                    newName = $"{fullName}_{cycle}";
                    sameName = false;
                    break;
                }
            }
            cycle++;
        }
        child.Name = newName;
        Children.Add(child);
        return true;
    }

    public void GetBones(List<Bone> bones)
    {
        bones.Add(this);
        foreach (var child in Children)
            child.GetBones(bones);
    }
}

public class RootBone : Bone
{
    public RootBone(string name) : base(name) {}

    public override void UpdateGlobalTransformation()
    {
        GlobalAnimatedMatrix = LocalAnimatedMatrix;
        foreach (var child in Children)
            child.UpdateGlobalTransformation();
    }

    public override string GetFullName()
    {
        return Name;
    }

    public RootBone Copy()
    {
        var copy = new RootBone(Name);
        foreach (var child in Children)
            copy.Children.Add(child.Copy(this));
        return copy;
    }
}

public class ChildBone : Bone
{   
    public Bone Parent;

    public ChildBone(string name, Bone parent) : base(name)
    {
        Parent = parent;
        Parent.Add(this);
    }

    public override void UpdateGlobalTransformation() {
        
        GlobalAnimatedMatrix = LocalAnimatedMatrix * Parent.GlobalAnimatedMatrix;
        foreach (var child in Children) {
            child.UpdateGlobalTransformation();
        }
    }

    public override string GetFullName()
    {
        return $"{Parent.GetFullName()}.{Name}";
    }

    public ChildBone Copy(Bone parent)
    {
        var copy = new ChildBone(Name, parent);
        foreach (var child in Children)
            copy.Children.Add(child.Copy(this));
        return copy;
    }
}