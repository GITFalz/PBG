using OpenTK.Mathematics;

public class Animation
{
    public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();
    
    int index = 0;
    float elapsedTime = 0;
    
    public bool GetFrame(out AnimationKeyframe? keyframe)
    {
        keyframe = null;

        if (index >= Keyframes.Count - 1)
            return false;
        
        AnimationKeyframe keyframe1 = Keyframes[index];
        AnimationKeyframe keyframe2 = Keyframes[index + 1];
        
        float t = (elapsedTime - index * AnimationController.frameTime) / AnimationController.frameTime;
        
        keyframe = keyframe1.Lerp(keyframe2, t);

        elapsedTime += GameTime.DeltaTime;
        index = (int)(elapsedTime / AnimationController.frameTime);

        return true;
    }
    
    public bool IsEnd()
    {
        return index >= Keyframes.Count - 1;
    }

    public void Reset()
    {
        index = 0;
        elapsedTime = 0;
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