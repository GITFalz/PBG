using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Info
{
    private static UIController _infoController = new();

    public static UIText FpsText = new("FpsTest", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (0, 0, 0, 0), 0);
    public static UIText GPUText = new("Gpu", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (0, 0, 0, 0), 0);
    public static UIText RamUsageText = new("RamUsage", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (0, 0, 0, 0), 0);

    private static int frameCount = 0;
    private static float elapsedTime = 0;

    private static Action _updateBlocks = () => { };

    public static bool RenderInfo = true;


    public Info()
    {
        UIVerticalCollection infoCollection = new("FpsCollection", _infoController, AnchorType.BottomLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (5, -5, 5, 5), (0, 0, 0, 0), 5, 0);

        FpsText.SetMaxCharCount(9).SetText("Fps: 9999", 1.2f);
        GPUText.SetTextCharCount($"GPU: {GL.GetString(StringName.Renderer)}", 1.2f);
        RamUsageText.SetMaxCharCount(20).SetText("Ram: 0", 1.2f);

        infoCollection.AddElements(FpsText, GPUText, RamUsageText);

        _infoController.AddElements(infoCollection);
    }

    public static void Resize()
    {
        _infoController.Resize();
    }

    public static void Update()
    {
        if (!RenderInfo)
            return;

        if (FpsUpdate())
        {
            FpsText.SetText($"Fps: {GameTime.Fps}", 1.2f).UpdateCharacters();
            long memoryBytes = Process.GetCurrentProcess().WorkingSet64; 
            RamUsageText.SetText($"Ram: {memoryBytes / (1024 * 1024)} Mb", 1.2f).UpdateCharacters();
        }  
        
        _infoController.Update(); 
        _updateBlocks();
    }

    public static void Render()
    {
        if (!RenderInfo)
            return;

        _infoController.RenderDepthTest();
    }

    private static bool FpsUpdate()
    {
        frameCount++;
        elapsedTime += (float)GameTime.DeltaTime;
        
        if (elapsedTime >= 1.0f)
        {
            int fps = Mathf.FloorToInt(frameCount / elapsedTime);
            frameCount = 0;
            elapsedTime = 0;
            
            GameTime.Fps = fps;
            
            return true;
        }
        
        return false;
    }
}