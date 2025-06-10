using OpenTK.Mathematics;

public class CameraEffect
{
    public EaseEffect? LastEffect { get; private set; } = null;
    public EaseEffect? CurrentEffect { get; private set; } = null;
    public EaseEffect? NextEffect { get; private set; } = null;
}

public abstract class EaseEffect
{
    public float Duration { get; set; } = 1f;
    public float ElapsedTime { get; private set; } = 0f;

    public bool IsFinished => ElapsedTime >= Duration;
    public EaseEffect? FollowingEffect = null;

    public EaseEffect(float duration = 1f)
    {
        Duration = duration;
    }

    public EaseEffect(EaseEffect followingEffect, float duration = 1f)
    {
        Duration = duration;
        FollowingEffect = followingEffect;
    }

    public void Update()
    {
        ElapsedTime += GameTime.DeltaTime;
    }

    public void Reset()
    {
        ElapsedTime = 0f;
    }

    public EaseEffect? GetFollowingEffect()
    {
        return FollowingEffect;
    }

    public abstract float Ease(float start, float end, float t);
}

public class LinearEaseEffect : EaseEffect
{
    public override float Ease(float start, float end, float t)
    {
        return start + (end - start) * t;
    }
}

public class EaseInEffect : EaseEffect
{
    public float EaseFactor = 2f;

    public EaseInEffect(float easeFactor = 2f)
    {
        EaseFactor = easeFactor;
    }

    public override float Ease(float start, float end, float t)
    {
        return start + (end - start) * (float)Math.Pow(t, EaseFactor);
    }
}

public class EaseOutEffect : EaseEffect
{
    public float EaseFactor = 2f;

    public EaseOutEffect(float easeFactor = 2f)
    {
        EaseFactor = easeFactor;
    }

    public override float Ease(float start, float end, float t)
    {
        return start + (end - start) * (1 - (float)Math.Pow(1 - t, EaseFactor));
    }
}

public class EaseInOutEffect : EaseEffect
{
    public float EaseFactor = 2f;

    public EaseInOutEffect(float easeFactor = 2f)
    {
        EaseFactor = easeFactor;
    }

    public override float Ease(float start, float end, float t)
    {
        if (t < 0.5f)
        {
            return start + (end - start) * (float)Math.Pow(t * 2, EaseFactor) / 2;
        }
        else
        {
            return start + (end - start) * (1 - (float)Math.Pow(1 - t * 2, EaseFactor) / 2);
        }
    }
}