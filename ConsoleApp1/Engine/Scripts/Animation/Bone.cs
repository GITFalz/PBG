using OpenTK.Mathematics;

public abstract class Bone
{
    public string Name;
    public List<ChildBone> Children = [];
    public BoneSelection Selection = BoneSelection.None;


    public Vector3 Position {
        get => _position;
        set
        {
            _position = value;
            LocalAnimatedMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }
    }
    private Vector3 _position = Vector3.Zero;
    public Quaternion Rotation {
        get => _rotation;
        set
        {
            _rotation = value;
            _eulerRotation = _rotation.ToEulerAngles();
            LocalAnimatedMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }
    }
    private Quaternion _rotation = Quaternion.Identity;
    public Vector3 EulerRotation
    {
        get => _eulerRotation;
        set
        {
            _eulerRotation = value;
            _rotation = Quaternion.FromEulerAngles(_eulerRotation);
            LocalAnimatedMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }
    }
    private Vector3 _eulerRotation = Vector3.Zero;
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            LocalAnimatedMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }
    }
    private float _scale = 1;

    // Computed at runtime (updated each frame)
    public Matrix4 LocalAnimatedMatrix = Matrix4.Identity;

    public Matrix4 GlobalAnimatedMatrix = Matrix4.Identity;
    public Matrix4 InverseGlobalAnimatedMatrix;
    public Matrix4 TransposedInverseGlobalAnimatedMatrix;

    public Matrix4 FinalMatrix => GlobalAnimatedMatrix;
    public int Index = 0;

    public BonePivot Pivot;
    public BonePivot End;
    public Vector3 EndTarget;
    public Vector3 LocalEnd => Position + Vector3.Transform(new Vector3(0, 2, 0) * 0.1f * Scale, Rotation);

    public Bone(string name) 
    {
        Name = name;
        Pivot = new(GetPivot, this);
        End = new(GetEnd, this);
        LocalAnimatedMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
    }

    public abstract void UpdateGlobalTransformation();
    public abstract string GetBonePath();
    public abstract RootBone GetRootBone();
    public abstract Vector3 GetPivot();
    public abstract Vector3 GetEnd();
    public abstract void Rotate();
    public abstract void Rotate(Vector3 axis, float angle);
    public abstract void Move();

    public void UpdateEndTarget()
    {
        EndTarget = End.Get - Pivot.Get;
    }

    public Matrix4 GetInverse()
    {
        return GlobalAnimatedMatrix.Inverted();
    }

    public bool Add(ChildBone child)
    {
        if (Children.Contains(child))
            return false;
        
        child.SetName(child.Name);
        Children.Add(child);
        return true;
    }

    public void SetName(string newName)
    {
        RootBone root = GetRootBone();
        List<string> names = [];
        root.GetBoneNames(names);
        string name = newName;
        int cycle = 0;
        while (names.Contains(newName))
        {
            newName = $"{name}_{cycle}";
            cycle++;
        }
        Name = newName;
    }

    public BonePivot Not(BonePivot pivot)
    {
        if (pivot == Pivot)
            return End;
        else
            return Pivot;
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
    public RootBone(string name) : base(name) { EndTarget = End.Get + Vector3.UnitY; }

    public override void UpdateGlobalTransformation()
    {
        GlobalAnimatedMatrix = LocalAnimatedMatrix;
        InverseGlobalAnimatedMatrix = GlobalAnimatedMatrix.Inverted();
        TransposedInverseGlobalAnimatedMatrix = InverseGlobalAnimatedMatrix.Transposed();

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

    public override Vector3 GetPivot()
    {
        return Position;
    }

    public override Vector3 GetEnd()
    {
        return Position + Vector3.Transform(new Vector3(0, 2, 0) * 0.1f * Scale, Rotation);
    }

    public override void Rotate()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        
        Vector3 axisY = Vector3.Normalize(Game.camera.front);
        Vector3 axisX = Vector3.Normalize(Game.camera.right);

        Rotation *= Quaternion.FromAxisAngle(axisY, MathHelper.DegreesToRadians(mouseDelta.X * GameTime.DeltaTime * 50f));
        Rotation *= Quaternion.FromAxisAngle(axisX, MathHelper.DegreesToRadians(mouseDelta.Y * GameTime.DeltaTime * 50f));
    }
    
    public override void Rotate(Vector3 axis, float angle)
    {
        Rotation *= Quaternion.FromAxisAngle(axis, MathHelper.DegreesToRadians(angle));
    }

    public override void Move()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();

        Vector3 axisY = Vector3.Normalize(Game.camera.up);
        Vector3 axisX = Vector3.Normalize(Game.camera.right);

        Position += axisY * -mouseDelta.Y * GameTime.DeltaTime * 5f;
        Position += axisX * mouseDelta.X * GameTime.DeltaTime * 5f;
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
        EndTarget = End.Get + Vector3.UnitY;
    }

    public override void UpdateGlobalTransformation()
    {

        GlobalAnimatedMatrix = LocalAnimatedMatrix * Parent.GlobalAnimatedMatrix;
        InverseGlobalAnimatedMatrix = GlobalAnimatedMatrix.Inverted();
        TransposedInverseGlobalAnimatedMatrix = InverseGlobalAnimatedMatrix.Transposed();

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

    public override Vector3 GetPivot()
    {
        var v4Position = new Vector4(Position, 1f);
        var v4Transformed = Parent.GlobalAnimatedMatrix.Transposed() * v4Position;
        return v4Transformed.Xyz;
    }

    public override Vector3 GetEnd()
    {
        var v4Position = new Vector4(Position + Vector3.Transform(new Vector3(0, 2, 0) * 0.1f * Scale, Rotation), 1f);
        var v4Transformed = Parent.GlobalAnimatedMatrix.Transposed() * v4Position;
        return v4Transformed.Xyz;
    }

    public override void Rotate()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();

        Vector3 axisY = Vector3.Normalize(Game.camera.front);
        Vector3 axisX = Vector3.Normalize(Game.camera.right);

        Matrix4 invParent = GlobalAnimatedMatrix.Inverted();
        Vector3 localAxisY = Vector3.Normalize(Vector3.TransformNormal(axisY, invParent));
        Vector3 localAxisX = Vector3.Normalize(Vector3.TransformNormal(axisX, invParent));

        Rotation *= Quaternion.FromAxisAngle(localAxisY, MathHelper.DegreesToRadians(mouseDelta.X * GameTime.DeltaTime * 50f));
        Rotation *= Quaternion.FromAxisAngle(localAxisX, MathHelper.DegreesToRadians(mouseDelta.Y * GameTime.DeltaTime * 50f));
    }
    
    public override void Rotate(Vector3 axis, float angle)
    {
        Matrix4 invParent = GlobalAnimatedMatrix.Inverted();
        Vector3 localAxis = Vector3.Normalize(Vector3.TransformNormal(axis, invParent));

        Rotation *= Quaternion.FromAxisAngle(localAxis, MathHelper.DegreesToRadians(angle));
    }

    public override void Move()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();

        Vector3 axisY = Vector3.Normalize(Game.camera.up);
        Vector3 axisX = Vector3.Normalize(Game.camera.right);

        Matrix4 invParent = GlobalAnimatedMatrix.Inverted();
        Vector3 localAxisY = Vector3.Normalize(Vector3.TransformNormal(axisY, invParent));
        Vector3 localAxisX = Vector3.Normalize(Vector3.TransformNormal(axisX, invParent));

        Position += localAxisY * -mouseDelta.Y * GameTime.DeltaTime * 5f;
        Position += localAxisX * mouseDelta.X * GameTime.DeltaTime * 5f;
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

public class BonePivot
{
    public Bone Bone;
    public Func<Vector3> PositionFunc;

    public BonePivot(Func<Vector3> positionFunc, Bone bone)
    {
        Bone = bone;
        PositionFunc = positionFunc;
    }

    public Vector3 Get => PositionFunc();

    public bool IsEnd()
    {
        return this == Bone.End;
    }

    public bool IsPivot()
    {
        return this == Bone.Pivot;
    }
}

public enum BoneSelection
{
    None = 0,
    Pivot = 1,
    End = 2,
    Both = 7
}