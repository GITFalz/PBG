using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Info
{
    private static UIController _infoController = new();
    private static TextMesh _textMesh = _infoController.TextMesh;

    public static UIText FpsText = new("FpsTest", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText GPUText = new("Gpu", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText XPosText = new("XPos", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText YPosText = new("YPos", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText ZPosText = new("ZPos", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText ChunkRenderingText = new("ChunkRendering", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText VertexCountText = new("VertexCount", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);
    public static UIText RamUsageText = new("RamUsage", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, _textMesh);

    private static int _oldVertexCount = 0;
    public static int VertexCount = 0;


    private static int frameCount = 0;
    private static float elapsedTime = 0;


    private static VAO _blockVao = new VAO();
    private static SSBO<InfoBlockData> _blockSSBO = new();
    private static List<InfoBlockData> _blockData = [];
    private static ConcurrentBag<InfoBlockData> _blocks = new ConcurrentBag<InfoBlockData>();
    private static ShaderProgram _blockShader = new ShaderProgram("Info/InfoBlock.vert", "Info/InfoBlock.frag");

    private static Action _updateBlocks = () => { };

    private static ArrayIDBO indirectBuffer = new([]);
    private static List<DrawArraysIndirectCommand> _indirectCommands = [];

    private static object lockObj = new object();

    public static bool RenderInfo = true;


    public Info()
    {
        UIVerticalCollection infoCollection = new("FpsCollection", _infoController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 1000), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
        UIHorizontalCollection positionCollection = new("PositionCollection", _infoController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (1000, 100), (5, 5, 5, 5), (0, 0, 0, 0), 5, 0);

        FpsText.SetMaxCharCount(9).SetText("Fps: 9999", 0.5f);
        GPUText.SetMaxCharCount(40).SetText("GPU: None", 0.5f);
        XPosText.SetMaxCharCount(15).SetText("X: 0", 0.5f);
        YPosText.SetMaxCharCount(15).SetText("Y: 0", 0.5f);
        ZPosText.SetMaxCharCount(15).SetText("Z: 0", 0.5f);
        ChunkRenderingText.SetMaxCharCount(20).SetText("Chunks: 0", 0.5f);
        VertexCountText.SetMaxCharCount(20).SetText("Vertices: 0", 0.5f);
        RamUsageText.SetMaxCharCount(20).SetText("Ram: 0", 0.5f);

        positionCollection.SetScale((positionCollection.Scale.X, FpsText.Scale.Y));

        positionCollection.AddElements(XPosText, YPosText, ZPosText);
        infoCollection.AddElements(FpsText, GPUText, positionCollection, ChunkRenderingText, VertexCountText, RamUsageText);

        GenerateBlocks();

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
            FpsText.SetText($"Fps: {GameTime.Fps}", 0.5f).GenerateChars().UpdateText();
            long memoryBytes = Process.GetCurrentProcess().WorkingSet64;
            RamUsageText.SetText($"Ram: {memoryBytes / (1024 * 1024)} Mb", 0.5f).GenerateChars();
        }  
        
        _infoController.Update(); 
        _updateBlocks();
    }

    public static void Render()
    {
        if (!RenderInfo)
            return;

        UpdateVertexCount();
        RenderBlocks();
        _infoController.RenderDepthTest();
    }

    public static void GenerateBlocks()
    {
        _blockVao.Renew();
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
                List<DrawArraysIndirectCommand> commands = new List<DrawArraysIndirectCommand>();
                for (int i = 0; i < _blockData.Count; i++)
                {
                    DrawArraysIndirectCommand command = new DrawArraysIndirectCommand
                    {
                        count = 36,
                        instanceCount = 1,
                        first = i * 36,
                        baseInstance = 0
                    };
                    commands.Add(command);
                }
                indirectBuffer = new ArrayIDBO(commands);
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
        indirectBuffer.Bind();

        GL.MultiDrawArraysIndirect(PrimitiveType.Triangles, IntPtr.Zero, indirectBuffer.Commands.Count, 0);
        
        _blockSSBO.Unbind();
        _blockVao.Unbind();
        indirectBuffer.Unbind();

        _blockShader.Unbind();

        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
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