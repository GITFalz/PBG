using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class Animation
{
    public const int FRAMES = 24;

    public string Name;
    public Dictionary<string, BoneAnimation> BoneAnimations = new Dictionary<string, BoneAnimation>();

    public Animation(string name)
    {
        Name = name;
    }

    public Matrix4? GetFrame(string boneName)
    {
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
            return boneAnimation.GetFrame();
        return null;
    }

    public void AddOrUpdateKeyframe(string boneName, AnimationKeyframe keyframe)
    {
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
        {
            boneAnimation.AddOrUpdateKeyframe(keyframe);
        }
        else
        {
            BoneAnimation newBoneAnimation = new BoneAnimation();
            newBoneAnimation.AddOrUpdateKeyframe(keyframe);
            BoneAnimations.Add(boneName, newBoneAnimation);
        }
    }

    public void RemoveKeyframe(string boneName, int index)
    {
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
        {
            boneAnimation.RemoveKeyframe(index);
        }
    }
}

public class BoneAnimation
{
    public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();
    public Func<Matrix4?> GetFrame;
    public float elapsedTime = 0;
    int index = 0;


    public BoneAnimation() { GetFrame = GetNullFrame; }
    public Matrix4? GetNullFrame() { return null; }
    public Matrix4? GetFrameSingle() { return Keyframes[0].GetLocalTransform(); }

    public Matrix4? GetFrameMultiple()
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
        return Keyframes[index].Lerp(Keyframes[index + 1], t).GetLocalTransform();
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

    public void RemoveKeyframe(int index)
    {
        if (Keyframes.Count > 0)
        {
            foreach (var keyframe in Keyframes)
            {
                if (keyframe.Index == index)
                {
                    Keyframes.Remove(keyframe);
                    break;
                }
            }

            if (Keyframes.Count == 0)
            {
                GetFrame = GetNullFrame;
                elapsedTime = 0;
                index = 0;
            }
            else
            {
                Keyframes = Keyframes.OrderBy(k => k.Time).ToList();
                GetFrame = Keyframes.Count > 1 ? GetFrameMultiple : GetFrameSingle;
            }
        }
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
    public Vector3 Position;
    public Quaternion Rotation;
    public float Scale;

    public AnimationKeyframe(int index, Vector3 position, Quaternion rotation, float scale)
    {
        Index = index;
        Time = (float)index / (float)Animation.FRAMES;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public AnimationKeyframe(Vector3 position, Quaternion rotation, float scale)
    {
        Index = 0;
        Time = 0;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public void SetIndex(int index)
    {
        Index = index;
        Time = (float)index / (float)Animation.FRAMES;
    }

    public AnimationKeyframe Lerp(Vector3 position, Quaternion rotation, float scale, float t)
    {
        return new AnimationKeyframe(Mathf.Lerp(Position, position, t), Quaternion.Slerp(Rotation, rotation, t), Mathf.Lerp(Scale, scale, t));
    }

    public AnimationKeyframe Lerp(AnimationKeyframe keyframe, float t)
    {
        return Lerp(keyframe.Position, keyframe.Rotation, keyframe.Scale, t);
    }

    public Matrix4 GetLocalTransform()
    {
        return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
    }
}