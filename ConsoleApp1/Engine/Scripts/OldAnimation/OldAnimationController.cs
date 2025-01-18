using OpenTK.Mathematics;

public class OldAnimationController
{
    public string Name;
    
    public const float framesPerSecond = 30;
    public const float frameTime = 1 / framesPerSecond;
    
    public Dictionary<string, OldAnimation> Animations = new Dictionary<string, OldAnimation>();
    public Queue<AnimationQueue> AnimationQueue = new Queue<AnimationQueue>();

    private int oldIndex = 0;
    int index = 0;
    float elapsedTime = 0;
    
    public OldAnimation BaseOldAnimation;
    public OldAnimation? currentAnimation = null;
    
    public AnimationMesh mesh;
    
    public bool loop = false;
    
    public OldAnimationController(string name)
    {
        Name = name;
    }
    

    public bool Update(float angle)
    {
        if (currentAnimation == null)
            return false;
        
        Console.WriteLine(Name);
        
        bool isEnd = currentAnimation.IsEnd();
        
        if (!isEnd && currentAnimation.GetFrame(out var keyframe))
        {
            if (keyframe != null)
            {
                mesh.UpdateRotation(keyframe.Rotation);
                mesh.UpdatePosition(keyframe.Position);
                mesh.UpdateRotation((0, 1, 0), angle + 180);
            }

            return true;
        }
        
        currentAnimation.Reset();
        
        if (AnimationQueue.Count > 0 && isEnd)
        {
            AnimationQueue animationQueue = AnimationQueue.Dequeue();
            
            currentAnimation = animationQueue.OldAnimation;
            loop = animationQueue.loop;
        }
        
        if (loop)
            return true;

        BaseAnimation();

        return false;
    }

    public OldAnimation GetAnimation(string name)
    {
        return Animations.TryGetValue(name, out var animation) ? animation : new OldAnimation();
    }
    
    public void SetAnimation(OldAnimation oldAnimation)
    {
        currentAnimation?.Reset();
        currentAnimation = oldAnimation;
    }
    
    public void SetAnimation(string name)
    {
        if (Animations.TryGetValue(name, out var animation))
        {
            AnimationQueue.Clear();
            currentAnimation?.Reset();
            currentAnimation = animation;
        }
    }
    
    public void QueueAnimation(string name)
    {
        if (Animations.TryGetValue(name, out var animation))
        {
            AnimationQueue.Enqueue(new AnimationQueue(animation, false));
        }
    }
    
    public void QueueLoopAnimation(string name)
    {
        if (Animations.TryGetValue(name, out var animation))
        {
            AnimationQueue.Enqueue(new AnimationQueue(animation, true));
        }
    }

    public static AnimationKeyframe? PlayAnimation(OldAnimation oldAnimation)
    {
        oldAnimation.GetFrame(out var keyframe);
        return keyframe;
    }
    
    public void BaseAnimation()
    {
        currentAnimation = BaseOldAnimation;
    }
}

public struct AnimationQueue
{
    public OldAnimation OldAnimation;
    public bool loop;
    
    public AnimationQueue(OldAnimation oldAnimation, bool loop)
    {
        this.OldAnimation = oldAnimation;
        this.loop = loop;
    }
}