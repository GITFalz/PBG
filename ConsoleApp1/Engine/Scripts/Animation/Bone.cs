using OpenTK.Mathematics;

public abstract class Bone
{
    public string Name;
    public List<ChildBone> Children = [];
    public BoneSelection Selection = BoneSelection.None;


    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public float Scale = 1;

    // Computed at bind time (static)
    public Matrix4 BindPoseMatrix;

    // Computed at runtime (updated each frame)
    public Matrix4 LocalAnimatedMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);

    public Matrix4 GlobalAnimatedMatrix;

    public Matrix4 FinalMatrix => GlobalAnimatedMatrix;
    public int Index = 0;

    public Bone(string name) { Name = name; }

    public abstract void UpdateGlobalTransformation();
    public abstract string GetBonePath();
    public abstract RootBone GetRootBone();

    public void SetBindPose()
    {
        BindPoseMatrix = GlobalAnimatedMatrix;
    }

    public Matrix4 GetInverse()
    {
        return GlobalAnimatedMatrix.Inverted();
    }

    public bool Add(ChildBone child)
    {
        if (Children.Contains(child))
            return false;

        RootBone root = GetRootBone();
        List<string> names = [];
        root.GetBoneNames(names);
        string name = child.Name;
        int cycle = 0;
        while (names.Contains(child.Name))
        {
            child.Name = $"{name}_{cycle}";
            cycle++;
        }
        Children.Add(child);
        return true;
    }

    public void GetBones(Dictionary<string, Bone> bones)
    {
        bones.Add(Name, this);
        foreach (var child in Children)
            child.GetBones(bones);
    }
    
    public void GetBoneNames(List<string> names)
    {
        names.Add(Name);
        foreach (var child in Children)
            child.GetBoneNames(names);
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

    public override string GetBonePath()
    {
        return Name;
    }

    public override RootBone GetRootBone()
    {
        return this;
    }

    public RootBone Copy()
    {
        var copy = new RootBone(Name);
        foreach (var child in Children)
            child.Copy(copy);

        copy.Position = Position;
        copy.Rotation = Rotation;
        copy.Scale = Scale;
        copy.UpdateGlobalTransformation();

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

    public override void UpdateGlobalTransformation()
    {

        GlobalAnimatedMatrix = LocalAnimatedMatrix * Parent.GlobalAnimatedMatrix;
        foreach (var child in Children)
        {
            child.UpdateGlobalTransformation();
        }
    }

    public override string GetBonePath()
    {
        return $"{Parent.GetBonePath()}.{Name}";
    }

    public override RootBone GetRootBone()
    {
        return Parent.GetRootBone();
    }

    public ChildBone Copy(Bone parent)
    {
        var copy = new ChildBone(Name, parent);
        foreach (var child in Children)
            child.Copy(copy);

        copy.Position = Position;
        copy.Rotation = Rotation;
        copy.Scale = Scale;
        return copy;
    }
}

public enum BoneSelection
{
    None = 0,
    Pivot = 1,
    End = 2,
    Both = 7
}