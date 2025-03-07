using System.Diagnostics;
using OpenTK.Mathematics;

public class Info
{
    private static UIController _infoController = new();
    private static UIVerticalCollection _timesCollection = new("Times", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (-5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
    private static List<(UIText, UIText)> _timesPool = new List<(UIText, UIText)>();
    private static List<(string, double)> _times = new List<(string, double)>();
    private static TextMesh _textMesh = _infoController.textMesh;

    public static UIText FpsText = new("FpsTest", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText GPUText = new("Gpu", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText XPosText = new("XPos", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText YPosText = new("YPos", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText ZPosText = new("ZPos", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText ChunkRenderingText = new("ChunkRendering", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText VertexCountText = new("VertexCount", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText RamUsageText = new("RamUsage", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);

    private static int _oldVertexCount = 0;
    public static int VertexCount = 0;



    private static int frameCount = 0;
    private static float elapsedTime = 0;

    public Info()
    {
        UIVerticalCollection infoCollection = new("FpsCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
        UIHorizontalCollection positionCollection = new("PositionCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);

        FpsText.SetMaxCharCount(9).SetText("Fps: 9999", 0.5f);
        GPUText.SetMaxCharCount(40).SetText("GPU: None", 0.5f);
        XPosText.SetMaxCharCount(15).SetText("X: 0", 0.5f);
        YPosText.SetMaxCharCount(15).SetText("Y: 0", 0.5f);
        ZPosText.SetMaxCharCount(15).SetText("Z: 0", 0.5f);
        ChunkRenderingText.SetMaxCharCount(20).SetText("Chunks: 0", 0.5f);
        VertexCountText.SetMaxCharCount(20).SetText("Vertices: 0", 0.5f);
        RamUsageText.SetMaxCharCount(20).SetText("Ram: 0", 0.5f);

        positionCollection.SetScale((positionCollection.Scale.X, FpsText.Scale.Y));

        positionCollection.AddElement(XPosText, YPosText, ZPosText);
        infoCollection.AddElement(FpsText, GPUText, positionCollection, ChunkRenderingText, VertexCountText, RamUsageText);
        
        _infoController.AddElement(infoCollection);
        _infoController.GenerateBuffers();
    }

    public static void Resize()
    {
        _infoController.OnResize();
    }

    public static void Update()
    {
        if (FpsUpdate())
        {
            FpsText.SetText($"Fps: {GameTime.Fps}", 0.5f).GenerateChars().UpdateText();
            long memoryBytes = Process.GetCurrentProcess().WorkingSet64;
            RamUsageText.SetText($"Ram: {memoryBytes / (1024 * 1024)} Mb", 0.5f).GenerateChars();
        }  
        
        _infoController.Update(); 
    }

    public static void Render()
    {
        UpdateVertexCount();
        _infoController.Render();
    }

    private static void UpdateVertexCount()
    {
        if (_oldVertexCount != VertexCount)
        {
            VertexCountText.SetText("Vertices: " + VertexCount, 0.5f).GenerateChars().UpdateText();
            _oldVertexCount = VertexCount;
        }
    }

    public static void SetPositionText(Vector3 oldPos, Vector3 position)
    {
        bool update = false;

        if (oldPos.X != position.X)
        {
            XPosText.SetText($"X: {position.X}", 0.5f).GenerateChars();
            update = true;
        }

        if (oldPos.Y != position.Y)
        {
            YPosText.SetText($"Y: {position.Y}", 0.5f).GenerateChars();
            update = true;
        }

        if (oldPos.Z != position.Z)
        {
            ZPosText.SetText($"Z: {position.Z}", 0.5f).GenerateChars();
            update = true;
        }

        if (update)
        {
            ZPosText.textMesh.UpdateText();
        }
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