using OpenTK.Mathematics;

public class AnimationController
{
    public const float framesPerSecond = 30;
    public const float frameTime = 1 / framesPerSecond;
    
    public static AnimationController Instance;
    
    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

    private int oldIndex = 0;
    int index = 0;
    float elapsedTime = 0;

    public Animation currentAnimation = new Animation();
    
    public void Start()
    {
        Instance = this;

        Vector3 testPos = new Vector3(0.5f, 0, -1);
        
        Animations.Add("test", new Animation());
        Animation test = Animations["test"];
        
        test.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, 0, -90), testPos));
        test.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (-90, 0, -41), (-1, 0, -1)));
        test.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-90, 0, 129), (-1, 0, 1)));
        test.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (-90, 0, 170), (-1, 0, 1)));
    }

    public void Update(AnimationMesh mesh, float angle)
    {
        if (!currentAnimation.IsEnd() && currentAnimation.GetFrame(out var keyframe))
        {
            if (keyframe != null)
            {
                mesh.UpdateRotation(keyframe.Rotation);
                mesh.UpdatePosition(keyframe.Position);
                mesh.UpdateRotation((0, 1, 0), angle + 180);
            }
        }
    }

    public Animation GetAnimation(string name)
    {
        return Animations.TryGetValue(name, out var animation) ? animation : new Animation();
    }

    public static AnimationKeyframe? PlayAnimation(Animation animation)
    {
        animation.GetFrame(out var keyframe);
        return keyframe;
    }
}

public class AnimationKeyframe
{
    public Vector3 Scale;
    public Quaternion Rotation;
    public Vector3 Position;

    public AnimationKeyframe(Vector3 scale, Quaternion rotation, Vector3 position)
    {
        Scale = scale;
        Rotation = rotation;
        Position = position;
    }
    
    public AnimationKeyframe(Vector3 scale, Vector3 rotation, Vector3 position)
    {
        Scale = scale;
        Rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(rotation.X), MathHelper.DegreesToRadians(rotation.Y), MathHelper.DegreesToRadians(rotation.Z));
        Position = position;
    }

    public AnimationKeyframe Lerp(AnimationKeyframe keyframe, float t)
    {
        return new AnimationKeyframe(
            Vector3.Lerp(Scale, keyframe.Scale, t),
            Quaternion.Slerp(Rotation, keyframe.Rotation, t),
            Vector3.Lerp(Position, keyframe.Position, t)
        );
    }

    public override string ToString()
    {
        return $"Scale: {Scale}, Rotation: {Rotation}, Position: {Position}";
    }
}