using OpenTK.Mathematics;

public abstract class Bone
{
    public readonly string Name;
    public List<Bone> Children = [];
    
    // Base values
    public BoneVertex Pivot;
    public BoneVertex End;

    public Matrix4 localTransform = Matrix4.Identity;
    public Matrix4 globalTransform = Matrix4.Identity;

    public Bone(string name)
    {
        Pivot = new(this, (0, 0, 0));
        End = new(this, (0, 1, 0));
        
        Name = name;
    }

    public abstract void CalculateGlobalTransform();

    public void GetBones(List<Bone> bones)
    {
        bones.Add(this);
        foreach (var child in Children) {
            child.GetBones(bones);
        }
    }
}

public class RootBone : Bone
{
    public RootBone(string name) : base(name) {}

    public override void CalculateGlobalTransform()
    {
        globalTransform = localTransform;
        foreach (var child in Children) {
            child.CalculateGlobalTransform();
        }
    }
}

public class ChildBone : Bone
{
    public RootBone RootBone;
    public Bone Parent;

    /// <summary>
    /// Constructor for the root bone
    /// </summary>
    /// <param name="name"></param>
    public ChildBone(RootBone rootBone, Bone parentBone, string name) : base(name)
    {
        RootBone = rootBone;
        Parent = parentBone;

        Parent.Children.Add(this);
    }

    public override void CalculateGlobalTransform()
    {
        globalTransform = localTransform * Parent.globalTransform;
        foreach (var child in Children) {
            child.CalculateGlobalTransform();
        }
    }
}

public class BoneVertex
{
    public Bone Parent;
    public Vector3 Position;
    public bool Selected;

    public BoneVertex(Bone parent, Vector3 position)
    {
        Parent = parent;
        Position = position;
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