using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Engine.Scripts.Core;
using ConsoleApp1.Engine.Scripts.Rendering.MeshClasses;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Game : GameWindow
{
    public static int width;
    public static int height;

    public static string mainPath;
    public static string chunkPath;

    private Camera _mainCamera;
    
    // World
    private ChunkManager _chunkManager;
    private WorldManager _worldManager;
    
    private ShaderProgram _shaderProgram;
    private Texture _textureArray;
    
    
    
    // UI
    private UIManager _uiManager;
    
    
    // Events
    private Action? _updateText;
    
    
    private FBO _fbo;
    
    
    public ConcurrentBag<GameObject> GameObjects = new ConcurrentBag<GameObject>();
    
    private bool _visibleCursor = false;
    private KeyboardSwitch _visibleCursorSwitch;



    private int frameCount = 0;
    private float elapsedTime = 0;
    private Stopwatch stopwatch;


    public static bool MoveTest = false;
    
    
    private bool isRunning = true;
    private Thread _physicsThread;
    
    
    //Skybox
    private ShaderProgram _skyboxShader;

    private Mesh _skyboxMesh;
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        Game.width = width;
        Game.height = height;
        
        //_fbo = new FBO(1080, 720);
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
        _mainCamera = new Camera(width, height, new Vector3(0, 100, 0));
        
        mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoxelGame");
        chunkPath = Path.Combine(mainPath, "Chunks");
        
        if (!Directory.Exists(chunkPath))
            Directory.CreateDirectory(chunkPath);
        
        _uiManager = new UIManager();
            
        base.OnLoad();

        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        _visibleCursorSwitch = new KeyboardSwitch(InputManager.IsKeyPressed);
        
        // World setup
        _chunkManager = new ChunkManager();
        _worldManager = new WorldManager();
        
        UVmaps editorUvs = new UVmaps( new int[] { 0, 0, 0, 0, 0, 0 });
        
        /*
        UVmaps grassUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        UVmaps dirtUvs = new UVmaps( new int[] { 2, 2, 2, 2, 2, 2 });
        UVmaps stoneUvs = new UVmaps( new int[] { 3, 3, 3, 3, 3, 3 });
        */
        
        CWorldBlock editor = new CWorldBlock("editor_block", 1, 1, editorUvs);

        /*
        CWorldBlock grass = new CWorldBlock("grass_block", 0, 1, grassUvs);
        CWorldBlock dirt = new CWorldBlock("dirt_block", 1, 2, dirtUvs);
        CWorldBlock stone = new CWorldBlock("stone_block", 2, 3, stoneUvs);
        */
        
        BlockManager.Add(editor);

        /*
        BlockManager.Add(grass);
        BlockManager.Add(dirt);
        BlockManager.Add(stone);
        */

        PlayerStateMachine player = new PlayerStateMachine();
        PhysicsBody playerPhysics = new PhysicsBody();
        
        GameObject playerObject = new GameObject();
        
        player.WorldManager = _worldManager;
        playerPhysics.doGravity = false;
        
        playerObject.AddComponent(player);
        playerObject.AddComponent(playerPhysics);

        GameObjects.Add(playerObject);
        
        _skyboxMesh = new SkyboxMesh();
        
        _skyboxShader = new ShaderProgram("Sky/Default.vert", "Sky/Default.frag");
        
        _worldManager.Start();

        _shaderProgram = new ShaderProgram("World/Default.vert", "World/Default.frag");
        _textureArray = new Texture("EditorTiles.png");
        
        _uiManager.Load();
        _uiManager.Start();
        
        GL.Enable(EnableCap.DepthTest);

        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Start();
        }
        
        _physicsThread = new Thread(PhysicsThread);
        _physicsThread.Start();
    }

    public async void GenerateChunks()
    {
        await Task.Run(() =>
        {
            Vector3i playerChunkPos = new Vector3i(0, 0, 0);

            List<Vector3i> chunks = new List<Vector3i>();

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    for (int z = 0; z < 20; z++)
                    {
                        chunks.Add(new Vector3i(x, y, z));
                    }
                }
            }

            chunks.Sort((a, b) => Vector3.Distance(playerChunkPos, a).CompareTo(Vector3.Distance(playerChunkPos, b)));

            foreach (var chunk in chunks)
            {
                _chunkManager.GenerateChunk(chunk * 32);
            }
        });
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        _chunkManager.Delete();
            
        _shaderProgram.Delete();
        _textureArray.Delete();

        _uiManager.Unload();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        isRunning = false;
        _physicsThread.Join();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.DepthFunc(DepthFunction.Lequal);
        GL.DepthMask(false);
       
        _skyboxShader.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Camera.GetViewMatrix();
        Matrix4 projection = Camera.GetProjectionMatrix();

        int sml = GL.GetUniformLocation(_skyboxShader.ID, "model");
        int svl = GL.GetUniformLocation(_skyboxShader.ID, "view");
        int spl = GL.GetUniformLocation(_skyboxShader.ID, "projection");
        
        GL.UniformMatrix4(sml, true, ref model);
        GL.UniformMatrix4(svl, true, ref view);
        GL.UniformMatrix4(spl, true, ref projection);
        
        //_chunkManager.CreateChunks();
        //_chunkManager.RenderChunks();
        
        _skyboxMesh.RenderMesh();

        _skyboxShader.Unbind();

        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
        
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        // World
        _shaderProgram.Bind();
        _textureArray.Bind();
        
        model = Matrix4.Identity;
        view = Camera.GetViewMatrix();
        projection = Camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int camPosLocation = GL.GetUniformLocation(_shaderProgram.ID, "camPos");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform3(camPosLocation, Camera.position);
        
        //_chunkManager.CreateChunks();
        //_chunkManager.RenderChunks();
        
        _worldManager.Render();
        
        _shaderProgram.Unbind();
        _textureArray.Unbind();
        
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Render();
        }
        
        _uiManager.OnRenderFrame(args);
        
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
    
    public static Matrix4 ClearTranslation(Matrix4 matrix)
    {
        matrix.Row3.X = 0; // Clear the X translation
        matrix.Row3.Y = 0; // Clear the Y translation
        matrix.Row3.Z = 0; // Clear the Z translation
        return matrix;
    }

    public void PhysicsThread()
    {
        int seconds = 0;
        double totalTime = 0;
        
        while (isRunning)
        {
            double time = stopwatch.Elapsed.TotalSeconds;
            
            if (time - totalTime >= GameTime.FixedDeltaTime)
            {
                foreach (GameObject gameObject in GameObjects)
                {
                    gameObject.FixedUpdate();
                }
                
                totalTime = time;
            }
        }
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        InputManager.Update(keyboard, mouse);
        GameTime.Update(args);

        if (InputManager.IsKeyPressed(Keys.LeftAlt))
            MoveTest = !MoveTest;
        
        
        
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Update(args);
        }
        
        if (_visibleCursor)
            _uiManager.OnUpdateFrame(keyboard, mouse, args);
        
        if (!_visibleCursor)
            _mainCamera.Update(keyboard, mouse, args);

        
        _worldManager.SetPlayerPosition(Camera.position);
        _worldManager.Update();
        
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
        
        bool update = FpsUpdate();
        
        if (update)
        {
            _uiManager.UpdateFps(GameTime.Fps);
        }
        
        base.OnUpdateFrame(args);
    }

    public bool FpsUpdate()
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