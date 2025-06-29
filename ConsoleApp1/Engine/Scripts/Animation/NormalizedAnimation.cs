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

    public NormalizedAnimation(Rig rig, NewAnimation animation)
    {
        Name = animation.Name;
        FrameSpeed = NewAnimation.FRAMES;
        BoneCount = rig.Bones.Count;
        FrameCount = animation.FrameCount;
        _boneAnimations = new NormalizedBoneAnimation[BoneCount];

        for (int i = 0; i < BoneCount; i++)
        {
            Bone bone = rig.BonesList[i];
            NewBoneAnimation boneAnimation;
            if (animation.GetBoneAnimation(bone.Name, out var b))
                boneAnimation = b;
            else
                boneAnimation = new NewBoneAnimation(bone.Name);

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

    public GlobalKeyframe GetSingleBoneKeyframe(int boneIndex)
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

    public GlobalKeyframe GetBoneKeyframe(int boneIndex)
    {
        (var k1, var k2) = _boneAnimations[boneIndex].GetKeyframe(_frameIndex, _frameIndex + 1);
        return k1.Lerp(k2, _t);
    }
}

public class NormalizedBoneAnimation
{
    public int FrameCount;
    public GlobalKeyframe[] Keyframes { get; private set; }

    public NormalizedBoneAnimation(NewBoneAnimation boneAnimation, int frameCount)
    {
        FrameCount = frameCount;
        Keyframes = new GlobalKeyframe[frameCount];
        for (int i = 0; i < frameCount; i++)
            Keyframes[i] = boneAnimation.GetKeyframe(i);
    }

    // No error checking, assumes valid index
    public GlobalKeyframe GetKeyframe(int i)
    {
        return Keyframes[i];
    }

    // No error checking, assumes valid indices
    public (GlobalKeyframe, GlobalKeyframe) GetKeyframe(int a, int b)
    {
        try
        {
            return (Keyframes[a], Keyframes[b]);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"Error: Invalid keyframe indices {a}, {b} in NormalizedBoneAnimation.");
            return (new GlobalKeyframe(), new GlobalKeyframe());
        }
    }
}

public enum AnimationStatus
{
    Stopped,
    Playing,
    Paused,
    Done,
}