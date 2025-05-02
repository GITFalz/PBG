using OpenTK.Mathematics;

public abstract class Bone
{
    public readonly string Name;
    public List<Bone> Children = [];
    
    // Base values
    public BoneVertex Pivot;
    public BoneVertex End;

    public Matrix4 LocalTransform = Matrix4.Identity;
    public Matrix4 GlobalTransform = Matrix4.Identity;

    public Vector3 OriginalPosition = Vector3.Zero;
    public Vector3 OriginalDirection = Vector3.UnitY;
    public float OriginalLength = 1f;

    public Bone(string name)
    {
        Pivot = new(this, (0, 0, 0));
        End = new(this, (0, 1, 0));
        
        Name = name;
    }
    
    public abstract void CalculateLocalTransformPropagate();
    public abstract void CalculateGlobalTransform();

    public void CalculateLocalTransform()
    {
        Matrix4 scale = Matrix4.CreateScale(GetDistance());
        Matrix4 rotation = Mathf.GetRotationMatrix((0, 1, 0), GetDirection());
        Matrix4 translation = Matrix4.CreateTranslation(Pivot.Position);
        LocalTransform = scale * rotation * translation;
    }

    public void GetBones(List<Bone> bones)
    {
        bones.Add(this);
        foreach (var child in Children) {
            child.GetBones(bones);
        }
    }

    public void Move(Vector3 offset)
    {
        Pivot.Position += offset;
        End.Position += offset;
    }

    public void SetDirection(Vector3 direction)
    {
        End.Position = Pivot.Position + direction.Normalized() * GetDistance();
    }

    public void SetLength(float length)
    {
        Vector3 direction = GetDirection();
        End.Position = Pivot.Position + direction.Normalized() * length;
    }

    private float GetDistance()
    {
        return (End.Position - Pivot.Position).Length;
    }

    private Vector3 GetDirection()
    {
        return (End.Position - Pivot.Position).Normalized();
    }
}

public class RootBone : Bone
{
    public RootBone(string name) : base(name) {}

    public override void CalculateLocalTransformPropagate()
    {
        CalculateLocalTransform();
        foreach (var child in Children) {
            child.CalculateLocalTransformPropagate();
        }
    }

    public override void CalculateGlobalTransform()
    {
        GlobalTransform = LocalTransform;
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

    public override void CalculateLocalTransformPropagate()
    {
        CalculateLocalTransform();
        foreach (var child in Children) {
            child.CalculateLocalTransformPropagate();
        }
    }

    public override void CalculateGlobalTransform()
    {
        GlobalTransform = LocalTransform * Parent.GlobalTransform;
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