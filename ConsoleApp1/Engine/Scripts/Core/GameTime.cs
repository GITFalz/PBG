using OpenTK.Windowing.Common;

public static class GameTime
{
    public static int Fps = 0;
    public static float DeltaTime { get; private set; }
    public static float TotalTime { get; private set; }
    public static float FixedTotalTime { get; private set; }
    /// <summary>
    /// delta use to lerp smoothly between the main update and the physics update
    /// </summary>
    public static float PhysicsDelta { get; private set; }
    private static float _time = 0;

    public const int PhysicSteps = 60;
    
    /// <summary>
    /// FixedDeltaTime is the time between each physics update, (only used in the physics thread)
    /// </summary>
    public const double FixedDeltaTime = 1f / PhysicSteps;

    /// <summary>
    /// FixedTime is the time since the last physics update and is the one used to calculate the physics
    /// </summary>
    public static float FixedTime = 0;
    
    private static float singleDeltaTime = 0;

    public static void Update(FrameEventArgs args)
    {
        DeltaTime = (float)args.Time;
        TotalTime += DeltaTime;
        singleDeltaTime = DeltaTime;

        _time += DeltaTime;
        float delta = _time / FixedTime;
        PhysicsDelta = Mathf.Clamp(0, 1, delta * 0.95f);
    }

    public static void FixedUpdate(float fixedTime)
    {
        FixedTotalTime = TotalTime;
        FixedTime = fixedTime;
        _time = 0;
    }

    public static float GetSingleDelta()
    {
        float delta = singleDeltaTime;
        singleDeltaTime = 0;
        return delta;
    }
}