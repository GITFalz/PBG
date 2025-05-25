using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public AnimationKeyframe? GetFrame(string boneName)
    {
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
            return boneAnimation.GetFrame();
        return null;
    }

    public void AddBoneAnimation(string boneName)
    {
        if (!BoneAnimations.ContainsKey(boneName))
        {
            BoneAnimations.Add(boneName, new BoneAnimation());
        }
    }

    public void AddBoneAnimation(BoneAnimation boneAnimation)
    {
        if (!BoneAnimations.ContainsKey(boneAnimation.Name))
        {
            BoneAnimations.Add(boneAnimation.Name, boneAnimation);
        }
    }

    public void AddBoneAnimation(string boneName, out BoneAnimation outAnimation)
    {
        if (BoneAnimations.TryGetValue(boneName, out BoneAnimation? value))
        {
            outAnimation = value;
            return;
        }

        BoneAnimation boneAnimation = new BoneAnimation();
        BoneAnimations.Add(boneName, boneAnimation);
        outAnimation = boneAnimation;
    }

    public void RemoveBoneAnimation(string boneName)
    {
        if (BoneAnimations.TryGetValue(boneName, out BoneAnimation? value))
        {
            value.Clear();
            BoneAnimations.Remove(boneName);
        }
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

    public void AddOrUpdateKeyframe(string boneName, AnimationKeyframe keyframe, out bool added)
    {
        added = false;
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
        {
            boneAnimation.AddOrUpdateKeyframe(keyframe, out added);
        }
        else
        {
            BoneAnimation newBoneAnimation = new BoneAnimation();
            newBoneAnimation.AddOrUpdateKeyframe(keyframe, out added);
            BoneAnimations.Add(boneName, newBoneAnimation);
        }
    }

    public bool RemoveKeyframe(string boneName, int index, [NotNullWhen(true)] out AnimationKeyframe? keyframe)
    {
        keyframe = null;
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
        {
            return boneAnimation.RemoveKeyframe(index, out keyframe);
        }
        return false;
    }

    public AnimationKeyframe? GetSpecificFrame(string boneName, int index)
    {
        if (BoneAnimations.TryGetValue(boneName, out var boneAnimation))
        {
            return boneAnimation.GetSpecificFrame(index);
        }
        return null;
    }

    public bool HasBoneAnimation(string boneName)
    {
        return BoneAnimations.ContainsKey(boneName);
    }

    public bool TryGetBoneAnimation(string boneName, [NotNullWhen(true)] out BoneAnimation? boneAnimation)
    {
        return BoneAnimations.TryGetValue(boneName, out boneAnimation);
    }

    public int GetFrameCount()
    {
        int count = 0;
        foreach (var (_, boneAnimation) in BoneAnimations)
        {
            count = Mathf.Max(count, boneAnimation.GetFrameCount());
        }
        return count;
    }

    public void Clear()
    {
        foreach (var boneAnimation in BoneAnimations.Values)
        {
            boneAnimation.Clear();
        }
        BoneAnimations = [];
    }
    

    public static bool LoadFromPath(string path)
    {
        return AnimationManager.LoadFromPath(path);
    }

    public static bool LoadFromPath(string path, [NotNullWhen(true)] out Animation? animation)
    {
        return AnimationManager.LoadFromPath(path, out animation);
    }
}

public class BoneAnimation
{
    public string Name = string.Empty;
    public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();
    public Func<AnimationKeyframe?> GetFrame;
    public float elapsedTime = 0;
    int index = 0;

    public BoneAnimation(string name)
    {
        Name = name;
        GetFrame = GetNullFrame;
    }
    public BoneAnimation() { GetFrame = GetNullFrame; }
    public AnimationKeyframe? GetNullFrame() { return null; }
    public AnimationKeyframe? GetFrameSingle() { return Keyframes[0]; }

    public AnimationKeyframe? GetFrameMultiple()
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
        return Keyframes[index].Lerp(Keyframes[index + 1], t);
    }

    public AnimationKeyframe GetSpecificFrame(int index)
    {
        if (Keyframes.Count > 0)
        {
            for (int i = 0; i < Keyframes.Count; i++)
            {
                AnimationKeyframe keyframe = Keyframes[i];
                if (index < keyframe.Index)
                    continue;

                if (i < Keyframes.Count - 1)
                {
                    AnimationKeyframe nextKeyframe = Keyframes[i + 1];
                    if (index >= nextKeyframe.Index)
                        continue;

                    elapsedTime = (float)index / (float)Animation.FRAMES;
                    float t1 = keyframe.Time;
                    float t2 = nextKeyframe.Time;
                    float t = Mathf.LerpI(t1, t2, elapsedTime);
                    return keyframe.Lerp(nextKeyframe, t);
                }
                else
                {
                    return keyframe;
                }
            }
            return GetLastFrame();
        }
        return new AnimationKeyframe();
    }

    public AnimationKeyframe GetLastFrame()
    {
        return Keyframes.Count > 0 ? Keyframes[^1] : new AnimationKeyframe();
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

    public void AddOrUpdateKeyframe(AnimationKeyframe keyframe, out bool added)
    {
        var existing = Keyframes.FirstOrDefault(k => k.Index == keyframe.Index);

        if (existing != null)
        {
            existing.Position = keyframe.Position;
            existing.Rotation = keyframe.Rotation;
            existing.Scale = keyframe.Scale;
            added = false;
            return;
        }
        else
        {
            added = true;
            Keyframes.Add(keyframe);
            Keyframes = Keyframes.OrderBy(k => k.Time).ToList();
        }

        GetFrame = Keyframes.Count > 1 ? GetFrameMultiple : GetFrameSingle;
    }

    public bool RemoveKeyframe(int index, [NotNullWhen(true)] out AnimationKeyframe? removedKeyframe)
    {
        removedKeyframe = null;
        bool removed = false;
        if (Keyframes.Count > 0)
        {
            foreach (var keyframe in Keyframes)
            {
                if (keyframe.Index == index)
                {
                    removedKeyframe = keyframe;
                    Keyframes.Remove(keyframe);
                    removed = true;
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
        return removed;
    }

    public bool ContainsIndex(int index)
    {
        if (Keyframes.Count > 0)
        {
            foreach (var keyframe in Keyframes)
            {
                if (keyframe.Index == index)
                {
                    return true;
                }
            }
        }
        return false;
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

    public int GetFrameCount()
    {
        int count = 0;
        foreach (var keyframe in Keyframes)
        {
            count = Mathf.Max(count, keyframe.Index);
        }
        return count;
    }

    public void Clear()
    {
        Keyframes = [];
        GetFrame = GetNullFrame;
        elapsedTime = 0;
        index = 0;
    }
}

public class AnimationKeyframe
{
    public float Time;
    public int Index;
    public Vector3 Position;
    public Quaternion Rotation;
    public float Scale;

    public AnimationKeyframe() : this(0, Vector3.Zero, Quaternion.Identity, 1) { }
    public AnimationKeyframe(int index, Vector3 position, Quaternion rotation, float scale)
    {
        Index = index;
        Time = (float)index / (float)Animation.FRAMES;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public AnimationKeyframe(int index, Bone bone) : this(index, bone.Position, bone.Rotation, bone.Scale) { }
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

    public List<string> Save()
    {
        Vector3 rotation = Rotation.ToEulerAngles();
        List<string> lines =
        [
            $"    Keyframe:",
            "    {",
            $"        Position: {Position.X} {Position.Y} {Position.Z}",
            $"        Rotation: {rotation.X} {rotation.Y} {rotation.Z}",
            $"        Scale: {Scale}",
            $"        Index: {Index}",
            "    }",
        ];
        return lines;
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