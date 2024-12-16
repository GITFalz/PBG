using OpenTK.Mathematics;

public class AnimationController
{
    public string Name;
    
    public const float framesPerSecond = 30;
    public const float frameTime = 1 / framesPerSecond;
    
    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

    private int oldIndex = 0;
    int index = 0;
    float elapsedTime = 0;
    
    public Animation baseAnimation;
    public Animation? currentAnimation = null;
    
    public AnimationMesh mesh;
    
    public bool loop = false;
    
    public AnimationController(string name)
    {
        Name = name;
    }
    

    public bool Update(float angle)
    {
        if (currentAnimation == null)
            return false;
        
        if (!currentAnimation.IsEnd() && currentAnimation.GetFrame(out var keyframe))
        {
            if (keyframe != null)
            {
                mesh.UpdateRotation(keyframe.Rotation);
                mesh.UpdatePosition(keyframe.Position);
                mesh.UpdateRotation((0, 1, 0), angle + 180);
            }

            return true;
        }
        
        currentAnimation.Reset();
        
        if (loop)
            return true;

        BaseAnimation();

        return false;
    }

    public Animation GetAnimation(string name)
    {
        return Animations.TryGetValue(name, out var animation) ? animation : new Animation();
    }
    
    public void SetAnimation(Animation animation)
    {
        currentAnimation?.Reset();
        currentAnimation = animation;
    }
    
    public void SetAnimation(string name)
    {
        if (Animations.TryGetValue(name, out var animation))
        {
            currentAnimation?.Reset();
            currentAnimation = animation;
        }
    }

    public static AnimationKeyframe? PlayAnimation(Animation animation)
    {
        animation.GetFrame(out var keyframe);
        return keyframe;
    }
    
    public void BaseAnimation()
    {
        currentAnimation = baseAnimation;
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