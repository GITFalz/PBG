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