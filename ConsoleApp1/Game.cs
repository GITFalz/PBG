using System.Diagnostics;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Assets.Scripts.World.Chunk;
using ConsoleApp1.Engine.Scripts.Core;
using ConsoleApp1.Engine.Scripts.Core.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
public class Game : GameWindow
{
    public static int width;
    public static int height;

    private Camera _mainCamera;
    
    // World
    private ChunkManager _chunkManager;
    private ShaderProgram _shaderProgram;
    private TextureArray _textureArray;
    
    
    
    // UI
    private UIManager _uiManager;
    
    
    // Events
    private Action? _updateText;
    
    
    
    public List<GameObject> GameObjects = new List<GameObject>();
    
    private bool _visibleCursor = false;
    private KeyboardSwitch _visibleCursorSwitch;



    private int frameCount = 0;
    private float elapsedTime = 0;
    private Stopwatch stopwatch;
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        Game.width = width;
        Game.height = height;
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        
        Game.width = e.Width;
        Game.height = e.Height;
        
        try
        {
            _uiManager.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, e.Width, e.Height, 0, -1, 1);
            _uiManager.OnResize();
            _mainCamera.UpdateProjectionMatrix(e.Width, e.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    protected override void OnLoad()
    {
        _uiManager = new UIManager();
            
        base.OnLoad();

        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        _visibleCursorSwitch = new KeyboardSwitch(InputManager.IsKeyPressed);
        
        // World setup
        _chunkManager = new ChunkManager();
        
        UVmaps grassUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        UVmaps dirtUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        UVmaps stoneUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });

        CWorldBlock grass = new CWorldBlock("grass_block", 0, 1, grassUvs);
        CWorldBlock dirt = new CWorldBlock("dirt_block", 1, 2, dirtUvs);
        CWorldBlock stone = new CWorldBlock("stone_block", 2, 3, stoneUvs);

        BlockManager.Add(grass);
        BlockManager.Add(dirt);
        BlockManager.Add(stone);

        for (int i = 0; i < 3; i++)
        {
            _chunkManager.GenerateChunk(new Vector3i(i * Chunk.WIDTH, 0, 0));
        }

        _shaderProgram = new ShaderProgram("World/Default.vert", "World/Default.frag");
        _textureArray = new TextureArray("Test_TextureAtlas.png", 32, 32);
        
        _uiManager.Load();
        _uiManager.Start();
        
        GL.Enable(EnableCap.DepthTest);
        
        _mainCamera = new Camera(width, height, new Vector3(0, 0, 0));
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        _chunkManager.Delete();
            
        _shaderProgram.Delete();
        _textureArray.Delete();

        _uiManager.Unload();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        // World
        _shaderProgram.Bind();
        _textureArray.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = _mainCamera.GetViewMatrix();
        Matrix4 projection = _mainCamera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        _chunkManager.RenderChunks();
        
        _shaderProgram.Unbind();
        _textureArray.Unbind();
        
        if (_visibleCursor)
            _uiManager.OnRenderFrame(args);

        
        _uiManager.UpdateFps();
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
    
    /**
    private void FpsCalculation()
    {
        frameCount++;
        elapsedTime += stopwatch.ElapsedMilliseconds / 100.0f;
        stopwatch.Restart();
        
        if (elapsedTime >= 1.0f)
        {
            int fps = Mathf.FloorToInt(frameCount / elapsedTime);
            frameCount = 0;
            elapsedTime -= 1f;
            
            _textMeshData.Clear();

            int[] fpsArray = IntToArray(fps);
            
            UI.GenerateCharacters(new Vector3(55f, 55f, 0.001f), 1, fpsArray, _textMeshData);

            _textVbo.Bind();
            _textVbo.Update(_textMeshData.verts);
            
            _textTextureVbo.Bind();
            _textTextureVbo.Update(_textMeshData.uvs);
            
            _textIbo.Bind();
            _textIbo.Update(_textMeshData.tris);
        }
    }

    private int[] IntToArray(int value)
    {
        int digitCount = (int)Math.Floor(Math.Log10(value) + 1);
        int[] digits;
        
        try
        {
            digits = new int[digitCount];
        }
        catch (OverflowException)
        {
            Console.WriteLine("Overflow");
            return [0];
        }
        
        for (int i = digitCount - 1; i >= 0; i--)
        {
            digits[i] = value % 10;
            value /= 10; 
        }

        return digits;
    }
    */

    
    
    
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        base.OnUpdateFrame(args);
        
        if (_visibleCursor)
            _uiManager.OnUpdateFrame(keyboard, mouse, args);
        
        if (!_visibleCursor)
            _mainCamera.Update(keyboard, mouse, args);
        
        if (_visibleCursorSwitch.CanSwitch(keyboard, Keys.Escape))
        {
            _visibleCursor = !_visibleCursor;
            if (!_visibleCursor)
            {
                CursorState = CursorState.Grabbed;
                _mainCamera.firstMove = true;
            }
            else
            {
                CursorState = CursorState.Normal;
            }
        }
    }
}