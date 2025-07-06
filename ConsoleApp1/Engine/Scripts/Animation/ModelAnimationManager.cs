using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;


/// <summary>
/// This class hold all the animations for the model, 
/// when loading the animation it optimizes the animation to have the less data possible but also the less overhead possible, 
/// </summary>
public class ModelAnimationManager
{
    public Rig Rig;
    private Dictionary<string, NormalizedAnimation> _animations = [];
    private List<NormalizedAnimationData> _animationQueue = new();

    public float SmoothDelta = 0f;
    public float SmoothTime = 0f;
    public float SmoothLerp = 0f;
    public bool Smooth = false;

    public bool LetFinish = false;

    public NormalizedAnimationData? PreviousAnimation;
    public NormalizedAnimationData? CurrentAnimation;


    public SSBO<Matrix4> BoneMatrices;
    private Matrix4[] _baseMatrices;

    public float AnimationSpeed = 1.0f;

    public ModelAnimationManager(Rig rig)
    {
        Rig = rig;
        Rig.Create();
        Rig.Initialize();
        _baseMatrices = new Matrix4[Rig.BonesList.Count];
        for (int i = 0; i < Rig.BonesList.Count; i++)
        {
            _baseMatrices[i] = Rig.BonesList[i].GlobalAnimatedMatrix;
        }
        BoneMatrices = new(_baseMatrices);
    }

    // No need to check if the rig is null here, this method should only be called at a point where the rig is guaranteed to be initialized.
    public void UpdateMatrices()
    {
        Rig.RootBone.UpdateGlobalTransformation();
        foreach (var bone in Rig.BonesList)
        {
            _baseMatrices[bone.Index] = bone.GlobalAnimatedMatrix;
        }
        BoneMatrices.Update(_baseMatrices, 0);
    }

    public void Update()
    {
        if (CurrentAnimation == null)
            return;

        CurrentAnimation.Animation.Update(AnimationSpeed);
        if (Smooth)
            PreviousAnimation?.Animation.Update();
        
        if (CurrentAnimation.Status == AnimationStatus.Done)
        {
            if (!CurrentAnimation.OnAnimationEnd?.Invoke() ?? true)
                return;
        }

        foreach (var bone in Rig.BonesList)
        {
            var frame = CurrentAnimation.Animation.GetBoneKeyframe(bone.Index);
            if (Smooth && PreviousAnimation != null)
            {
                frame = PreviousAnimation.Animation.GetBoneKeyframe(bone.Index).Lerp(frame, SmoothLerp);
            }
            bone.Position = frame.Position;
            bone.Rotation = frame.Rotation;
            bone.Scale = frame.Scale;
            bone.LocalAnimatedMatrix = frame.GetLocalTransform(); ;
        }

        if (Smooth)
        {
            if (SmoothDelta >= SmoothTime)
            {
                Smooth = false;
                SmoothDelta = 0f;
                SmoothLerp = 0f;
                SmoothTime = 0f;
                PreviousAnimation?.Reset();
                PreviousAnimation = null;
            }

            SmoothDelta += GameTime.DeltaTime;
            SmoothLerp = Mathf.LerpI(0, SmoothTime, SmoothDelta);
        }

        UpdateMatrices();
    }

    public void AddAnimations(Rig rig, params (string name, Animation? animation)[] animations)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            Animation? animation = animations[i].animation;
            if (animation == null)
            {
                PopUp.AddPopUp($"Animation {animations[i].name} was not found");
                continue;
            }

            AddAnimation(new NormalizedAnimation(rig, animation));
        }
    }

    public void AddAnimations(params NormalizedAnimation[] animation)
    {
        for (int i = 0; i < animation.Length; i++)
        {
            AddAnimation(animation[i]);
        }
    }

    public void AddAnimation(NormalizedAnimation animation)
    {
        if (_animations.ContainsKey(animation.Name))
            return;

        _animations[animation.Name] = animation;
    }

    public void Play(string animationName, float blendTime = -1f)
    {
        if (!_animations.TryGetValue(animationName, out var animation))
            return;

        PlayInternal(animation, blendTime, false);
    }

    public void Loop(string animationName, float blendTime = -1f)
    {
        if (!_animations.TryGetValue(animationName, out var animation))
            return;

        PlayInternal(animation, blendTime, true);
    }

    public void PlayQueued(string animationName, float blendTime = -1f)
    {
        if (!_animations.TryGetValue(animationName, out var animation))
            return;

        PlayQueued(animation, blendTime, false);
    }

    private void PlayQueued(NormalizedAnimation animation, float blendTime, bool loop)
    {
        if (CurrentAnimation == null)
        {
            PlayInternal(animation, 0, loop);
        }
        else
        {
            var animData = new NormalizedAnimationData(animation);
            animData.OnCallbackEnd(loop ? CreateLoopCallback(animation) : CreatePlayCallback());
            Enqueue(animData);
        }
    }

    private void PlayInternal(NormalizedAnimation animation, float blendTime, bool loop)
    {
        if (LetFinish && CurrentAnimation?.Status == AnimationStatus.Playing)
        {
            PlayQueued(animation, blendTime, loop);
            LetFinish = false;
            return;
        }

        ClearQueue();

        if (CurrentAnimation != null)
        {
            PreviousAnimation = CurrentAnimation;
            if (blendTime > 0)
            {
                Smooth = true;
                SmoothTime = blendTime;
                SmoothDelta = 0f;
                SmoothLerp = 0f;
            }
        }

        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.Status = AnimationStatus.Playing;
        CurrentAnimation.OnCallbackEnd(loop ? CreateLoopCallback(animation) : CreatePlayCallback());
    }

    public void ForceFinish()
    {
        LetFinish = true;
    }

    public void Stop()
    {
        SetDefault();
    }

    private Func<bool> CreateLoopCallback(NormalizedAnimation animationData)
    {
        return () =>
        {
            if (TryDequeue(out var nextAnimation))
            {
                return true;
            }
            else
            {
                animationData.Reset();
                animationData.Status = AnimationStatus.Playing;
                return true;
            }
        };
    }

    private Func<bool> CreatePlayCallback()
    {
        return () =>
        {
            if (TryDequeue(out var nextAnimation))
            {
                return true;
            }
            else
            {
                SetDefault();
                return false;
            }
        };
    }

    public void SetSpeed(float speed)
    {
        AnimationSpeed = speed;
    }

    public void Enqueue(NormalizedAnimationData animationData)
    {
        _animationQueue.Add(animationData);
    }

    public bool TryDequeue([NotNullWhen(true)] out NormalizedAnimationData? animationData)
    {
        if (_animationQueue.Count > 0)
        {
            animationData = _animationQueue[0];
            _animationQueue.RemoveAt(0);
            animationData.Reset();
            CurrentAnimation = animationData;
            CurrentAnimation.Status = AnimationStatus.Playing;
            return true;
        }

        animationData = null;
        return false;
    }

    public void ClearQueue()
    {
        _animationQueue.Clear();
    }

    public bool SetDefault()
    {
        SetSpeed(1.0f);
        CurrentAnimation?.Reset();
        CurrentAnimation = null;
        PreviousAnimation?.Reset();
        PreviousAnimation = null;
        return false;
    }

    public List<string> GetAnimations()
    {
        List<string> animations = [];
        foreach (var animation in _animations.Values)
        {
            animations.Add("    Animation: " + animation.Name);
        }
        return animations;
    }
}

public class NormalizedAnimationData
{
    public AnimationStatus Status
    {
        get => Animation.Status;
        set => Animation.Status = value;
    }
    public NormalizedAnimation Animation;
    public Func<bool>? OnAnimationEnd;

    public NormalizedAnimationData(NormalizedAnimation animation)
    {
        Animation = animation;
    }

    public void Reset()
    {
        Animation.Reset();
    }

    public void OnCallbackEnd(Func<bool> action)
    {
        OnAnimationEnd = action;
    }
}

public enum AnimationStatus
{
    Stopped,
    Playing,
    Paused,
    Done,
}