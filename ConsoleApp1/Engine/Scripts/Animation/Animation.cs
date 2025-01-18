using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class Animation
{
    public const int FRAMES = 24;

    public string Name;
    public Dictionary<int, BoneAnimation> BoneAnimations = new Dictionary<int, BoneAnimation>();

    public Animation(string name)
    {
        Name = name;
    }

    public Quaternion? GetFrame(int boneIndex)
    {
        if (BoneAnimations.TryGetValue(boneIndex, out var boneAnimation))
            return boneAnimation.GetFrame();
        return null;
    }

    public void AddOrUpdateKeyframe(int boneIndex, AnimationKeyframe keyframe)
    {
        if (BoneAnimations.TryGetValue(boneIndex, out var boneAnimation))
        {
            boneAnimation.AddOrUpdateKeyframe(keyframe);
        }
        else
        {
            BoneAnimation newBoneAnimation = new BoneAnimation();
            newBoneAnimation.AddOrUpdateKeyframe(keyframe);
            BoneAnimations.Add(boneIndex, newBoneAnimation);
        }
    }
}

public class BoneAnimation
{
    public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();
    public Func<Quaternion?> GetFrame;
    public float elapsedTime = 0;
    int index = 0;


    public BoneAnimation() { GetFrame = GetNullFrame; }
    public Quaternion? GetNullFrame() { return null; }
    public Quaternion? GetFrameSingle() { return Keyframes[0].Rotation; }

    public Quaternion? GetFrameMultiple()
    {
        ResetIndexCheck();

        float t1 = Keyframes[index].Time;
        float t2 = Keyframes[index + 1].Time;

        if (elapsedTime >= t2)
        {
            index++;

            ResetIndexCheck();

            t1 = Keyframes[index].Time;
            t2 = Keyframes[index + 1].Time;
        }

        float t = Mathf.LerpI(t1, t2, elapsedTime);

        elapsedTime += GameTime.DeltaTime;
        return Keyframes[index].Lerp(Keyframes[index + 1], t).Rotation;
    }

    /// <summary>
    /// Add or updates a keyframe to the animation and sort the keyframes by time
    /// </summary>
    /// <param name="keyframe"></param>
    public void AddOrUpdateKeyframe(AnimationKeyframe keyframe)
    {
        var existing = Keyframes.FirstOrDefault(k => k.Index == keyframe.Index);

        if (existing != null)
        {
            existing.Rotation = keyframe.Rotation;
            return;
        }
        else
        {
            Keyframes.Add(keyframe);
            Keyframes = Keyframes.OrderBy(k => k.Time).ToList();
        }

        GetFrame = Keyframes.Count > 1 ? GetFrameMultiple : GetFrameSingle;
    }

    private bool ResetIndexCheck()
    {
        if (index >= Keyframes.Count - 1)
        {
            index = 0;
            elapsedTime = 0;
            return true;
        }
        return false;
    }
}

public class AnimationKeyframe
{
    public float Time;
    public int Index;
    public Quaternion Rotation;

    public AnimationKeyframe(int index, Quaternion rotation)
    {
        Index = index;
        Time = (float)index / (float)Animation.FRAMES;
        Rotation = rotation;
    }

    private AnimationKeyframe(Quaternion rotation)
    {
        Rotation = rotation;
    }

    public AnimationKeyframe Lerp(AnimationKeyframe keyframe, float t)
    {
        return new AnimationKeyframe(Quaternion.Slerp(Rotation, keyframe.Rotation, t));
    }
}