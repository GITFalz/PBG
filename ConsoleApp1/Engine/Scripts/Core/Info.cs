using System.Diagnostics;

public class Info
{
    private static UIController _infoController = new();
    private static UIVerticalCollection _timesCollection = new("Times", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (-5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
    private static List<(UIText, UIText)> _timesPool = new List<(UIText, UIText)>();
    private static List<(string, double)> _times = new List<(string, double)>();
    private static TextMesh _textMesh = _infoController.textMesh;

    public static UIText FpsText = new("FpsTest", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText XPosText = new("XPos", AnchorType.TopLeft, PositionType.Relative, (0, 20, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText YPosText = new("YPos", AnchorType.TopLeft, PositionType.Relative, (0, 40, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText ZPosText = new("ZPos", AnchorType.TopLeft, PositionType.Relative, (0, 60, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);
    public static UIText ChunkRenderingText = new("ChunkRendering", AnchorType.TopLeft, PositionType.Relative, (0, 80, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), _textMesh);

    public Info()
    {
        UIVerticalCollection infoCollection = new("FpsCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
        UIHorizontalCollection positionCollection = new("PositionCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);

        FpsText.SetMaxCharCount(9).SetText("Fps: 9999", 0.5f);
        XPosText.SetMaxCharCount(15).SetText("X: 0", 0.5f);
        YPosText.SetMaxCharCount(15).SetText("Y: 0", 0.5f);
        ZPosText.SetMaxCharCount(15).SetText("Z: 0", 0.5f);
        ChunkRenderingText.SetMaxCharCount(20).SetText("Chunks: 0", 0.5f);

        positionCollection.SetScale((positionCollection.Scale.X, FpsText.Scale.Y));

        positionCollection.AddElement(XPosText, YPosText, ZPosText);
        infoCollection.AddElement(FpsText, positionCollection, ChunkRenderingText);
        
        _infoController.AddElement(infoCollection);
        _infoController.GenerateBuffers();
    }

    public static void Render()
    {
        _infoController.Render();
    }
}