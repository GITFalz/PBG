using OpenTK.Windowing.Common;

public static class GameTime
{
    public static float DeltaTime { get; private set; }
    public static float TotalTime { get; private set; }
    public const float FixedDeltaTime = 1f / 60f;

    public static void Update(FrameEventArgs args)
    {
        DeltaTime = (float)args.Time;
        TotalTime += DeltaTime;
    }
}