using OpenTK.Mathematics;

public abstract class Bone
{
    public readonly string Name;
    public List<Bone> Children = [];


    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    // Computed at bind time (static)
    public Matrix4 BindPoseMatrix;
    public Matrix4 InverseBindMatrix;

    // Computed at runtime (updated each frame)
    public Matrix4 LocalAnimatedMatrix => Matrix4.CreateTranslation(-Position) * Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);   

    public Matrix4 GlobalAnimatedMatrix;

    public Matrix4 FinalMatrix => InverseBindMatrix * GlobalAnimatedMatrix;

    public Bone(string name) { Name = name; }
    
    public abstract void UpdateGlobalTransformation();

    public void SetBindPose()
    {
        BindPoseMatrix = GlobalAnimatedMatrix;
        InverseBindMatrix = Matrix4.Invert(BindPoseMatrix);
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
}

public class ChildBone : Bone
{
    public Bone Parent;

    public ChildBone(string name, Bone parent) : base(name)
    {
        Parent = parent;
        Parent.Children.Add(this);
    }

    public override void UpdateGlobalTransformation() {
        GlobalAnimatedMatrix = Parent.GlobalAnimatedMatrix * LocalAnimatedMatrix;
        
        foreach (var child in Children) {
            child.UpdateGlobalTransformation();
        }
    }
}