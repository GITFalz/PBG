﻿using OpenTK.Windowing.Common;

public static class GameTime
{
    public static int Fps = 0;
    public static float DeltaTime { get; private set; }
    public static float TotalTime { get; private set; }
    public const float FixedDeltaTime = 1f / 60f;
    
    private static float singleDeltaTime = 0;

    public static void Update(FrameEventArgs args)
    {
        DeltaTime = (float)args.Time;
        TotalTime += DeltaTime;
        singleDeltaTime = DeltaTime;
    }

    public static float GetSingleDelta()
    {
        float delta = singleDeltaTime;
        singleDeltaTime = 0;
        return delta;
    }
}