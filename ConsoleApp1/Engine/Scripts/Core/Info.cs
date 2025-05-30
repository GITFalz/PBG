using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Info
{
    private static UIController _infoController = new();

    // Global Info
    public static UIText FpsText = new("FpsTest", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText GPUText = new("Gpu", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText RamUsageText = new("RamUsage", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);

    // Player Info
    public static UIText PositionText = new("Position", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);

    // Chunk Info
    public static UIText GlobalChunkCount = new("GlobalChunkCount", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText GlobalChunkVertexCount = new("GlobalChunkVertexCount", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText AvgChunkGenerationTime = new("AvgChunkGenerationTime", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    
    // Selected Chunk Info
    public static UIText SelectedChunkPosition = new("SelectedChunkPosition", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText SelectedChunkVertexCount = new("SelectedChunkVertexCount", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
    public static UIText SelectedChunkIndexCount = new("SelectedChunkIndexCount", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);

    private static int _oldVertexCount = 0;
    public static int VertexCount = 0;

    private static int _oldChunkCount = 0;
    public static int ChunkCount = 0;

    public static double TotalGenTime = 0;
    public static double TotalGenCount = 1;
    public static double AvgChunkGenTime = 0;


    private static int frameCount = 0;
    private static float elapsedTime = 0;


    private static VAO _blockVao = new VAO();
    private static SSBO<InfoBlockData> _blockSSBO = new();
    private static List<InfoBlockData> _blockData = [];

    private static ConcurrentBag<InfoBlockData> _blocks = new ConcurrentBag<InfoBlockData>();
    private static ShaderProgram _blockShader = new ShaderProgram("Info/InfoBlock.vert", "Info/InfoBlock.frag");

    private static Action _updateBlocks = () => { };
    private static Action _toggleAction = () => { };

    private static object lockObj = new object();

    public static bool RenderInfo = false;


    private static Vector3 _oldPosition = Vector3.Zero;


    public Info()
    {
        UIVerticalCollection infoCollection = new("FpsCollection", _infoController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);

        // General info
        UIVerticalCollection generalInfo = new("GeneralInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 20), 5, 0);

        UIText GeneralInfo = new("GeneralInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
        GeneralInfo.SetTextCharCount("--General--", 1.2f);

        FpsText.SetMaxCharCount(9).SetText("Fps: 9999", 1.2f);
        GPUText.SetMaxCharCount(40).SetText("GPU: None", 1.2f);
        RamUsageText.SetMaxCharCount(20).SetText("Ram: 0", 1.2f);

        generalInfo.AddElements(GeneralInfo, FpsText, GPUText, RamUsageText);

        // Player info
        UIVerticalCollection playerInfo = new("PlayerInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 20), 5, 0);

        UIText PlayerInfo = new("PlayerInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
        PlayerInfo.SetTextCharCount("--Player--", 1.2f);

        PositionText.SetMaxCharCount(100).SetText("Position: X: 0, Y: 0, Z: 0", 1.2f);

        playerInfo.AddElements(PlayerInfo, PositionText);

        // Global chunk info
        UIVerticalCollection globalChunkInfo = new("GlobalChunkInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 20), 5, 0);

        UIText GlobalChunkInfo = new("GlobalChunkInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
        GlobalChunkInfo.SetTextCharCount("--Global--", 1.2f);

        GlobalChunkCount.SetMaxCharCount(50).SetText("Chunks: 0", 1.2f);
        GlobalChunkVertexCount.SetMaxCharCount(50).SetText("Vertices: 0", 1.2f);
        AvgChunkGenerationTime.SetMaxCharCount(50).SetText("Average gen time: 0ms", 1.2f);

        globalChunkInfo.AddElements(GlobalChunkInfo, GlobalChunkCount, GlobalChunkVertexCount, AvgChunkGenerationTime);


        // Selected chunk info
        UIVerticalCollection selectedChunkInfo = new("SelectedChunkInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 20), 5, 0);

        UIText SelectedChunkInfo = new("SelectedChunkInfo", _infoController, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0);
        SelectedChunkInfo.SetTextCharCount("--Selected--", 1.2f);

        SelectedChunkPosition.SetMaxCharCount(50).SetText("X: 0, Y: 0, Z: 0", 1.2f);
        SelectedChunkVertexCount.SetMaxCharCount(20).SetText("Vertices: 0", 1.2f);
        SelectedChunkIndexCount.SetMaxCharCount(20).SetText("Indices: 0", 1.2f);

        selectedChunkInfo.AddElements(SelectedChunkInfo, SelectedChunkPosition, SelectedChunkVertexCount, SelectedChunkIndexCount);


        infoCollection.AddElements(generalInfo, playerInfo, globalChunkInfo, selectedChunkInfo);

        GenerateBlocks();

        _infoController.AddElements(infoCollection);


        _toggleAction = () => ToggleOn();
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
            AvgChunkGenerationTime.SetText($"Average gen time: {AvgChunkGenTime.ToString("0.00")}ms", 1.2f).UpdateCharacters();
        }  
        
        _infoController.Update(); 
        _updateBlocks();
    }

    public static void Render()
    {
        if (Input.IsKeyPressed(Keys.F3))
            _toggleAction.Invoke();

        if (!RenderInfo)
            return;

        RenderBlocks();
        _infoController.RenderDepthTest();
    }

    public static void GenerateBlocks()
    {
        _blockSSBO.Renew(_blockData.ToArray());
    }

    public static void ClearBlocks()
    {
        _blocks.Clear();
    }

    public static void AddBlock(InfoBlockData block)
    {
        lock (lockObj)
        {
            _blocks.Add(block);
        }
    }

    public static void AddBlock(params InfoBlockData[] block)
    {
        foreach (var b in block)
            AddBlock(b);
    }


    public static void UpdateBlocks()
    {
        lock (lockObj)
        {
            _updateBlocks = () => 
            {
                _blockData = [.. _blocks];
                GenerateBlocks();
                _updateBlocks = () => { };
            };
        } 
    }

    public static void RenderBlocks()
    {
        if (_blockData.Count == 0)
            return;

        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        _blockShader.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Game.Camera.ViewMatrix;
        Matrix4 projection = Game.Camera.ProjectionMatrix;

        int modelLocationA = GL.GetUniformLocation(_blockShader.ID, "model");
        int viewLocationA = GL.GetUniformLocation(_blockShader.ID, "view");
        int projectionLocationA = GL.GetUniformLocation(_blockShader.ID, "projection");
        
        GL.UniformMatrix4(viewLocationA, true, ref view);
        GL.UniformMatrix4(projectionLocationA, true, ref projection);
        GL.UniformMatrix4(modelLocationA, true, ref model);

        _blockVao.Bind();
        _blockSSBO.Bind(1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _blockData.Count * 36);

        Shader.Error("Error rendering info blocks");
        
        _blockSSBO.Unbind();
        _blockVao.Unbind();

        _blockShader.Unbind();

        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
    }

    public static void SetChunkVertexCount()
    {
        if (_oldVertexCount != VertexCount)
        {
            GlobalChunkVertexCount.SetText($"Vertices: {VertexCount}", 1.2f).UpdateCharacters();
            _oldVertexCount = VertexCount;
        }  
    }

    public static void SetChunkRenderCount()
    {
        if (_oldChunkCount != ChunkCount)
        {
            GlobalChunkCount.SetText($"Chunks: {ChunkCount}", 1.2f).UpdateCharacters();
            _oldChunkCount = ChunkCount;
        }
    }

    public static void SetSelectedChunkInfo(Vector3i position, int vertexCount, int indexCount)
    {

    }

    public static void SetPositionText(Vector3 position)
    {
        if (_oldPosition != position)
        {
            PositionText.SetText($"Position: X: {position.X}, Y: {position.Y}, Z: {position.Z}", 1.2f).UpdateCharacters();
            _oldPosition = position;
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


    private void ToggleOn()
    {
        RenderInfo = true;
        _toggleAction = () => ToggleOff();
    }

    private void ToggleOff()
    {
        RenderInfo = false;
        _toggleAction = () => ToggleOn();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InfoBlockData
{
    public Vector3 Position;
    private float Padding1 = 0;
    public Vector3 Size;
    private float Padding2 = 0;
    public Vector4 Color;

    public InfoBlockData(Vector3 position, Vector3 size, Vector4 color)
    {
        Position = position;
        Size = size;
        Color = color;
    }
}