using OpenTK.Mathematics;

public class OldAnimation
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
        
        float t = (elapsedTime - index * OldAnimationController.frameTime) / OldAnimationController.frameTime;
        
        keyframe = keyframe1.Lerp(keyframe2, t);

        elapsedTime += GameTime.DeltaTime;
        index = (int)(elapsedTime / OldAnimationController.frameTime);

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
    public Vector3 Scale = Vector3.One;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Position = Vector3.Zero;
    
    public Vector3 Forward => Vector3.Transform((0, 0, 1), Rotation);
    public Vector3 Up => Vector3.Transform((0, 1, 0), Rotation);
    
    public AnimationKeyframe(Vector3 scale, Vector3 rotation, Vector3 position)
    {
        Scale = scale;
        
        Rotation = Mathf.RotateAround((1, 0, 0), Rotation, MathHelper.DegreesToRadians(rotation.X));
        Rotation = Mathf.RotateAround(Up, Rotation, MathHelper.DegreesToRadians(rotation.Y));
        Rotation = Mathf.RotateAround(Forward, Rotation, MathHelper.DegreesToRadians(rotation.Z));
        
        //Rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(rotation.X), MathHelper.DegreesToRadians(rotation.Y), MathHelper.DegreesToRadians(rotation.Z));
        Position = position;
    }
    
    public AnimationKeyframe(Vector3 scale, Quaternion rotation, Vector3 position)
    {
        Scale = scale;
        Rotation = rotation;
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