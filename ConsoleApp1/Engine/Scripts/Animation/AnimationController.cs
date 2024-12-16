using OpenTK.Mathematics;

public class AnimationController
{
    public const float framesPerSecond = 30;
    public const float frameTime = 1 / framesPerSecond;
    
    public static AnimationController Instance;
    
    public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();

    private int oldIndex = 0;
    int index = 0;
    float elapsedTime = 0;
    
    public void Start()
    {
        Instance = this;
        
        Keyframes.Add(new AnimationKeyframe(Vector3.One, Quaternion.Identity, Vector3.Zero));
        SetKeyframe(5, new AnimationKeyframe(Vector3.One, (0, 110, 0), Vector3.Zero));
        SetKeyframe(10, new AnimationKeyframe(Vector3.One, (0, 220, 0), Vector3.Zero));
    }
    
    public bool GetFrame(out AnimationKeyframe? keyframe)
    {
        keyframe = null;

        if (index >= Keyframes.Count - 1)
            return false;
        
        AnimationKeyframe keyframe1 = Keyframes[index];
        AnimationKeyframe keyframe2 = Keyframes[index + 1];

        Vector3 scale1 = keyframe1.Scale;
        Quaternion rotation1 = keyframe1.Rotation;
        Vector3 position1 = keyframe1.Position;
        
        Vector3 scale2 = keyframe2.Scale;
        Quaternion rotation2 = keyframe2.Rotation;
        Vector3 position2 = keyframe2.Position;
        
        float t = (elapsedTime - index * frameTime) / frameTime;
        
        keyframe = keyframe1.Lerp(keyframe2, t);

        elapsedTime += GameTime.DeltaTime;
        index = (int)(elapsedTime / frameTime);

        return true;
    }

    public void SetKeyframe(int index, AnimationKeyframe keyframe)
    {
        if (index < 0 || index >= Keyframes.Count)
        {
            int last = Keyframes.Count - 1;
            float t = index - last;
            
            for (int i = Keyframes.Count; i <= index; i++)
            {
                int current = i - last;
                Keyframes.Add(Keyframes[last].Lerp(keyframe, current / t));
            }
        }
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