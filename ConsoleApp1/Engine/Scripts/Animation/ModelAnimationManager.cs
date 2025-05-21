using OpenTK.Mathematics;


/// <summary>
/// This class hold all the animations for the model, 
/// when loading the animation it optimizes the animation to have the less data possible but also the less overhead possible, 
/// </summary>
public class ModelAnimationManager
{
    public Dictionary<string, ModelAnimation> Animations = [];
    public ModelAnimation? CurrentAnimation;

    public SSBO<Matrix4> BoneMatrices;
    private Matrix4[] _baseMatrices;

    public ModelAnimationManager(Rig rig)
    {
        rig.Create();
        rig.Initialize();
        _baseMatrices = new Matrix4[rig.BonesList.Count];
        for (int i = 0; i < rig.BonesList.Count; i++)
        {
            _baseMatrices[i] = rig.BonesList[i].GlobalAnimatedMatrix;
        }
        BoneMatrices = new(_baseMatrices);
    }

    public void Update()
    {
        if (CurrentAnimation == null || !CurrentAnimation.PlayFrame())
            return;

        BoneMatrices.Update(CurrentAnimation.CurrentTransformations, 0);
    }

    public void Loop(string name)
    {
        if (!Animations.TryGetValue(name, out ModelAnimation? animation))
            return;

        CurrentAnimation?.Reset();
        CurrentAnimation = animation;
    }

    public void LoopAfter(string name)
    {
        if (!Animations.TryGetValue(name, out ModelAnimation? animation))
            return;

        if (CurrentAnimation != null)
            CurrentAnimation.OnAnimationEnd = () => Loop(name);
        else
            Loop(name);
    }

    public void Play(string name)
    {
        if (!Animations.TryGetValue(name, out ModelAnimation? animation))
            return;

        CurrentAnimation?.Reset();
        CurrentAnimation = animation;
        CurrentAnimation.OnAnimationEnd = SetDefault;
    }

    public void PlayAfter(string name)
    {
        if (!Animations.TryGetValue(name, out ModelAnimation? animation))
            return;

        if (CurrentAnimation != null)
            CurrentAnimation.OnAnimationEnd = () => Play(name);
        else
            Play(name);
    }

    public void SetDefault()
    {
        CurrentAnimation?.Reset();
        CurrentAnimation = null;
        BoneMatrices.Update(_baseMatrices, 0);
    }
    
}

public class ModelAnimation
{
    public const int FrameRate = 24;

    public string Name;
    public int AnimationFrames;
    public int BoneCount;
    public KeyframeData[] AnimationData;
    public Matrix4[] CurrentTransformations;
    public AnimationStatus Status = AnimationStatus.Stopped;
    public Action? OnAnimationEnd;

    private float _elapsedTime;
    private float _totalDuration;

    public ModelAnimation(Rig rig, Animation animation)
    {
        Name = animation.Name;
        AnimationFrames = animation.GetFrameCount();
        BoneCount = rig.BonesList.Count;
        AnimationData = new KeyframeData[AnimationFrames * BoneCount];
        CurrentTransformations = new Matrix4[BoneCount];

        for (int i = 0; i < rig.BonesList.Count; i++)
        {
            Bone bone = rig.BonesList[i];
            if (!animation.TryGetBoneAnimation(bone.Name, out BoneAnimation? boneAnimation))
            {
                boneAnimation = new BoneAnimation(bone.Name);
                boneAnimation.AddOrUpdateKeyframe(new AnimationKeyframe(0, bone));
            }

            for (int j = 0; j < AnimationFrames; j++)
            {
                AnimationKeyframe keyframe = boneAnimation.GetSpecificFrame(j) ?? new();
                KeyframeData data = new KeyframeData
                {
                    Position = keyframe.Position,
                    Rotation = keyframe.Rotation,
                    Scale = keyframe.Scale
                };
                AnimationData[j + i * AnimationFrames] = data;
            }
        }

        _elapsedTime = 0;
        _totalDuration = AnimationFrames / (float)FrameRate;
    }

    public bool PlayFrame()
    {
        float frameTime = _elapsedTime * FrameRate;
        int index = Mathf.FloorToInt(frameTime);
        float t = (float)(frameTime - index);

        if (index >= AnimationFrames - 1)
        {
            for (int i = 0; i < BoneCount; i++)
            {
                int offset = i * AnimationFrames;
                KeyframeData keyframe = AnimationData[offset + index];
                CurrentTransformations[i] = keyframe.GetLocalTransform();
            }

            OnAnimationEnd?.Invoke();
            Status = AnimationStatus.Done;
            _elapsedTime = 0;
            return false;
        }

        for (int i = 0; i < BoneCount; i++)
        {
            int offset = i * AnimationFrames;
            KeyframeData start = AnimationData[offset + index];
            KeyframeData end = AnimationData[offset + index + 1];
            CurrentTransformations[i] = start.Lerp(end, t).GetLocalTransform();
        }

        Status = AnimationStatus.Playing;
        _elapsedTime += GameTime.DeltaTime;
        return true;
    }

    public void Reset()
    {
        Status = AnimationStatus.Stopped;
        OnAnimationEnd = null;
        _elapsedTime = 0;
    }
}

public struct KeyframeData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Scale;

    public KeyframeData Lerp(KeyframeData other, float t)
    {
        return new KeyframeData
        {
            Position = Vector3.Lerp(Position, other.Position, t),
            Rotation = Quaternion.Slerp(Rotation, other.Rotation, t),
            Scale = Mathf.Lerp(Scale, other.Scale, t)
        };
    }

    public Matrix4 GetLocalTransform()
    {
        return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
    }
}

public enum AnimationStatus
{
    Stopped,
    Playing,
    Paused,
    Done,
}