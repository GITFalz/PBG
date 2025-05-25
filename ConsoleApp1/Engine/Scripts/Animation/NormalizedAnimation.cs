using OpenTK.Mathematics;
using Veldrid;

/// <summary>
/// This is still an animation, but all frames are pre processed to avoid interpolation overhead at runtime.
/// </summary>
public class NormalizedAnimation
{
    public string Name;
    public int FrameSpeed { get; private set; } = 24;
    public int BoneCount { get; private set; }
    public int FrameCount { get; private set; }

    public AnimationStatus Status = AnimationStatus.Stopped;

    private NormalizedBoneAnimation[] _boneAnimations = [];

    private float _elapsedTime = 0f;
    private int _frameIndex = 0;
    private float _t = 0f;

    public NormalizedAnimation(Rig rig, Animation animation)
    {
        Name = animation.Name;
        FrameSpeed = Animation.FRAMES;
        BoneCount = rig.Bones.Count;
        FrameCount = animation.GetFrameCount();
        _boneAnimations = new NormalizedBoneAnimation[BoneCount];

        for (int i = 0; i < BoneCount; i++)
        {
            Bone bone = rig.BonesList[i];
            BoneAnimation boneAnimation;
            if (animation.TryGetBoneAnimation(bone.Name, out var b))
                boneAnimation = b;
            else
                boneAnimation = new BoneAnimation(bone.Name);

            _boneAnimations[i] = new NormalizedBoneAnimation(boneAnimation, FrameCount);
        }
    }

    public void Reset()
    {
        _elapsedTime = 0f;
        _frameIndex = 0;
        _t = 0f;
        Status = AnimationStatus.Stopped;
    }

    public AnimationKeyframe GetSingleBoneKeyframe(int boneIndex)
    {
        return _boneAnimations[boneIndex].GetKeyframe(0);
    }

    public void Update(float speed = 1f)
    {
        float frame = _elapsedTime * FrameSpeed;
        _frameIndex = Mathf.FloorToInt(frame);
        if (_frameIndex + 1 >= FrameCount)
        {
            _frameIndex = 0;
            _elapsedTime = 0f;
            frame = 0f;
            Status = AnimationStatus.Done;
        }
        _t = frame - _frameIndex;
        _elapsedTime += GameTime.DeltaTime * speed;
    }

    public AnimationKeyframe GetBoneKeyframe(int boneIndex)
    {
        (var k1, var k2) = _boneAnimations[boneIndex].GetKeyframe(_frameIndex, _frameIndex + 1);
        return k1.Lerp(k2, _t);
    }
}

public class NormalizedBoneAnimation
{
    public int FrameCount;
    public AnimationKeyframe[] Keyframes { get; private set; }

    public NormalizedBoneAnimation(BoneAnimation boneAnimation, int frameCount)
    {
        FrameCount = frameCount;
        Keyframes = new AnimationKeyframe[frameCount];
        for (int i = 0; i < frameCount; i++)
            Keyframes[i] = boneAnimation.GetSpecificFrame(i);
    }

    // No error checking, assumes valid index
    public AnimationKeyframe GetKeyframe(int i)
    {
        return Keyframes[i];
    }

    // No error checking, assumes valid indices
    public (AnimationKeyframe, AnimationKeyframe) GetKeyframe(int a, int b)
    {
        return (Keyframes[a], Keyframes[b]);
    }
}