using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;


/// <summary>
/// This class hold all the animations for the model, 
/// when loading the animation it optimizes the animation to have the less data possible but also the less overhead possible, 
/// </summary>
public class ModelAnimationManager
{
    public Rig Rig;
    public Dictionary<string, NormalizedAnimation> Animations = [];
    public List<NormalizedAnimationData> AnimationQueue = new();

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

    public void AddAnimation(NormalizedAnimation animation)
    {
        if (Animations.ContainsKey(animation.Name))
            return;

        Animations[animation.Name] = animation;
    }

    public void Loop(string name)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        Loop(animation);
    }

    public void LoopAfter(string name)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        if (CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(LoopDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
        }
        else
            Loop(animation);
    }

    private void Loop(NormalizedAnimation animation)
    {
        if (LetFinish && CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(LoopDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
            LetFinish = false;
            return;
        }

        AnimationQueue = [];
        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.SetAfter(LoopDequeue);
        CurrentAnimation.Status = AnimationStatus.Playing;
    }

    private bool LoopDequeue()
    {
        if (TryDequeue(out NormalizedAnimationData? nextAnimation))
        {
            nextAnimation.Reset();
            CurrentAnimation = nextAnimation;
            CurrentAnimation.Status = AnimationStatus.Playing;
        }
        else if (CurrentAnimation != null)
        {
            CurrentAnimation.Reset();
            CurrentAnimation.Status = AnimationStatus.Playing;
        }
        return true;
    }

    public void SmoothLoop(string name, float time)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        SmoothLoop(animation, time);
    }

    private void SmoothLoop(NormalizedAnimation animation, float time)
    {
        if (LetFinish && CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(LoopDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
            LetFinish = false;
            return;
        }

        Smooth = true;
        SmoothTime = time;
        SmoothDelta = 0f;
        SmoothLerp = 0f;

        if (CurrentAnimation != null)
            PreviousAnimation = CurrentAnimation;

        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.SetAfter(LoopDequeue);
        CurrentAnimation.Status = AnimationStatus.Playing;
    }

    public void Play(string name)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;
        
        Play(animation);
    }

    public void PlayAfter(string name)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        if (CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(PlayDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
        }
        else
            Play(animation);
    }

    private void Play(NormalizedAnimation animation)
    {
        if (LetFinish && CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(PlayDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
            LetFinish = false;
            return;
        }

        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.Status = AnimationStatus.Playing;
        CurrentAnimation.SetAfter(PlayDequeue);
    }

    public void SmoothPlay(string name, float time)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        SmoothPlay(animation, time);
    }

    private void SmoothPlay(NormalizedAnimation animation, float time)
    {
        if (LetFinish && CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(PlayDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
            LetFinish = false;
            return;
        }

        Smooth = true;
        SmoothTime = time;
        SmoothDelta = 0f;
        SmoothLerp = 0f;

        if (CurrentAnimation != null)
            PreviousAnimation = CurrentAnimation;
        
        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.SetAfter(PlayDequeue);
        CurrentAnimation.Status = AnimationStatus.Playing;
    }

    public void SmoothPlayFinish(string name, float time)
    {
        if (!Animations.TryGetValue(name, out NormalizedAnimation? animation))
            return;

        SmoothPlayFinish(animation, time);
    }

    private void SmoothPlayFinish(NormalizedAnimation animation, float time)
    {
        if (LetFinish && CurrentAnimation != null)
        {
            NormalizedAnimationData nextAnimation = new NormalizedAnimationData(animation);
            nextAnimation.SetAfter(PlayDequeue);
            nextAnimation.Status = AnimationStatus.Playing;
            Enqueue(nextAnimation);
            LetFinish = false;
            return;
        }

        Smooth = true;
        SmoothTime = time;
        SmoothDelta = 0f;
        SmoothLerp = 0f;
        LetFinish = true;

        if (CurrentAnimation != null)
            PreviousAnimation = CurrentAnimation;

        animation.Reset();
        CurrentAnimation = new NormalizedAnimationData(animation);
        CurrentAnimation.SetAfter(PlayDequeue);
        CurrentAnimation.Status = AnimationStatus.Playing;
    }

    private bool PlayDequeue()
    {
        if (TryDequeue(out NormalizedAnimationData? nextAnimation))
        {
            nextAnimation.Reset();
            CurrentAnimation = nextAnimation;
            CurrentAnimation.Status = AnimationStatus.Playing;
            return true;
        }
        else
        {
            SetDefault();
            return false;
        }
    }

    public void Stop()
    {
        if (CurrentAnimation == null)
            return;

        AnimationQueue = [];
        SetDefault();
    }

    public void StopAfter()
    {
        if (CurrentAnimation == null)
            return;

        AnimationQueue = [];
        CurrentAnimation.SetAfter(SetDefault);
    }

    public void SetSpeed(float speed)
    {
        AnimationSpeed = speed;
    }

    public void Enqueue(NormalizedAnimationData animationData)
    {
        AnimationQueue.Add(animationData);
    }

    public bool TryDequeue([NotNullWhen(true)] out NormalizedAnimationData? animationData)
    {
        if (AnimationQueue.Count > 0)
        {
            animationData = AnimationQueue[0];
            AnimationQueue.RemoveAt(0);
            return true;
        }

        animationData = null;
        return false;
    }

    public bool TryGetLast([NotNullWhen(true)] out NormalizedAnimationData? animationData)
    {
        if (AnimationQueue.Count > 0)
        {
            animationData = AnimationQueue[^1];
            return true;
        }

        animationData = null;
        return false;
    }

    public bool SetDefault()
    {
        SetSpeed(1.0f);
        if (CurrentAnimation == null)
            return false;

        CurrentAnimation.Reset();
        CurrentAnimation = null;
        PreviousAnimation?.Reset();
        PreviousAnimation = null;
        return false;
    }

    public List<string> GetAnimations()
    {
        List<string> animations = [];
        foreach (var animation in Animations.Values)
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

    public void SetAfter(Func<bool> action)
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