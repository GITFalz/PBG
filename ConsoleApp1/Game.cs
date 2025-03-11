using System.Collections.Concurrent;
using System.Diagnostics;
using ConsoleApp1.Engine.Scripts.Core.Data;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Game : GameWindow
{
    public static Game Instance;
    
    public static int Width;
    public static int Height;
    
    public static int CenterX;
    public static int CenterY;

    // User paths
    public static string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Engines", "PBG");
    public static string chunkPath = Path.Combine(mainPath, "Chunks");
    public static string assetPath = Path.Combine(mainPath, "Saves");
    public static string shaderPath = Path.Combine(mainPath, "Shaders");
    public static string uiPath = Path.Combine(assetPath, "UI");
    public static string texturePath = Path.Combine(assetPath, "Textures");
    public static readonly string modelPath = Path.Combine(assetPath, "Models");
    public static readonly string undoModelPath = Path.Combine(mainPath, "UndoModels");



    public static Vector3 BackgroundColor = new Vector3(0.4f, 0.6f, 0.98f);
    public static Camera camera { get; private set; }
    public static Action UpdateCamera = () => { camera?.Update(); };
    
    public ConcurrentDictionary<string, Scene> Scenes = new ConcurrentDictionary<string, Scene>();

    private Stopwatch stopwatch;


    public static bool MoveTest = false;
    
    
    private bool isRunning = true;
    private Thread _physicsThread;


    private static Scene _worldScene = new Scene("World");
    
    
    public static Scene? CurrentScene;

    // Miscaleanous Ui
    private PopUp _popUp;

    // Initializations
    private Action Resize = () => { };

    public static Action<Keys> InputActions = (e) => { };


    private bool physicsStep = false;
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            APIVersion = new Version(4, 6),
            Profile = ContextProfile.Core,
            Title = "OpenGL 4.6"
        })
    {
        Instance = this;

        CenterWindow(new Vector2i(width, height));

        camera = new Camera(width, height, new Vector3(0, 0, 0));

        _ = new Info();
        
        Width = width;
        Height = height;
        
        CenterX = width / 2;
        CenterY = height / 2;

        Resize = () => { Resize = OnResize; };
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        
        Width = e.Width;
        Height = e.Height;
        
        CenterX = e.Width / 2;
        CenterY = e.Height / 2;

        UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -2, 2);

        Resize.Invoke();
        
        base.OnResize(e);
    }

    private void OnResize()
    {
        camera = new Camera(Width, Height, new Vector3(0, 0, 0));

        _popUp.Resize();
        Info.Resize();
        Timer.Resize();

        CurrentScene?.OnResize();

        foreach (var (_, scene) in Scenes)
        {
            if (scene != CurrentScene)
                scene.Resize = true;
        }
    }
    
    protected override void OnLoad()
    {
        // Utils
        stopwatch = new Stopwatch();
        stopwatch.Start();

        Timer.Start();

        if (!Directory.Exists(chunkPath))
            Directory.CreateDirectory(chunkPath);
        if (!Directory.Exists(modelPath))
            Directory.CreateDirectory(modelPath);
        if (!Directory.Exists(undoModelPath))
            Directory.CreateDirectory(undoModelPath);
        
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);
        if (!Directory.Exists(uiPath))
            Directory.CreateDirectory(uiPath);
        
        // Input
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        Input.Start(keyboard, mouse);
        
        // Blocks
        //CWorldBlock editor = new CWorldBlock("editor_block", 1, 1, new([0, 0, 0, 0, 0, 0]));

        //BlockManager.Add(editor);
        
        CWorldBlock grass = new("grass_block", 0, 1, new([ 0, 0, 1, 0, 2, 0]));
        CWorldBlock dirt = new("dirt_block", 1, 2, new([2, 2, 2, 2, 2, 2]));
        CWorldBlock stone = new("stone_block", 2, 3, new([3, 3, 3, 3, 3, 3]));
        
        BlockManager.Add(grass);
        BlockManager.Add(dirt);
        BlockManager.Add(stone);
        
        // World
        TransformNode worldGenerationNode = new TransformNode();
        worldGenerationNode.AddChild(new WorldManager());

        TransformNode playerNode = new TransformNode();
        playerNode.AddChild(new PhysicsBody(), new PlayerStateMachine());

        _worldScene.AddNode(worldGenerationNode, playerNode);

        AddScenes(_worldScene);
        LoadScene("World");

        _popUp = new PopUp();

        _physicsThread = new Thread(PhysicsThread);
        _physicsThread.Start();

        GL.Enable(EnableCap.DepthTest);

        Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Renderer));

        Info.GPUText.SetText($"GPU: {GL.GetString(StringName.Renderer)}", 0.5f).GenerateChars().UpdateText();

        base.OnLoad();
    }
    
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        Input.PressedKeys.Add(e.Key);

        if (e.Key == Keys.Escape)
        {
            MoveTest = !MoveTest;

            if (MoveTest)
            {
                SetCursorState(CursorState.Grabbed);
                UpdateCamera = () =>
                {
                    camera.SetMoveFirst();
                    camera.Update();
                    UpdateCamera = () => { camera.Update(); };
                };
            }
            else
            {
                SetCursorState(CursorState.Normal);
                UpdateCamera = () => { };
            }
            
        }
        else if (e.Key == Keys.P)
        {
            camera.SetCameraMode(camera.GetCameraMode() == CameraMode.Follow ? CameraMode.Free : CameraMode.Follow);
        }
        else if (e.Key == Keys.L)
        {
            physicsStep = true;
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Input.PressedButtons.Add(e.Button);
    }
    
    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        Input.PressedKeys.Remove(e.Key);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        Input.PressedButtons.Remove(e.Button);
    }
    
    protected override void OnUnload()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        isRunning = false;
        _physicsThread.Join();
        
        CurrentScene?.OnExit();
        Timer.Stop();
        UIController.ClearAll();
        
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        Input.Update(keyboard, mouse);
        GameTime.Update(args);

        Timer.Update();
        Info.Update();

        _popUp.Update();
        
        UpdateCamera.Invoke();
        CurrentScene?.OnUpdate();

        ThreadPool.Update();

        base.OnUpdateFrame(args);
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Viewport(0, 0, Width, Height);
        
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        Skybox.Render();
        CurrentScene?.OnRender();
        _popUp.Render();
        //Timer.Render();
        Info.Render();
        
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }

    public void PhysicsThread()
    {
        double totalTime = 0;
        
        while (isRunning)
        {
            double time = stopwatch.Elapsed.TotalSeconds;
            
            double fixedTime = time - totalTime;
            if (fixedTime >= GameTime.FixedDeltaTime)
            {
                GameTime.FixedTime = (float)fixedTime;
                _worldScene.OnFixedUpdate();  
                totalTime = time;
            }
            
            Thread.Sleep(1);
        }
    }
    
    public static void SetCursorState(CursorState state)
    {
        Instance.CursorState = state;
    }
    
    public static void LoadScene(string sceneName)
    {
        if (Instance.Scenes.TryGetValue(sceneName, out Scene? scene))
        {
            CurrentScene?.OnExit();
            CurrentScene = scene;
            scene.OnAwake();
            if (scene.Resize)
            {
                scene.OnResize();
                scene.Resize = false;
            }
        }
    }
    
    public static void AddScenes(params Scene[] scene)
    {
        foreach (Scene s in scene)
        {
            if (!Instance.Scenes.TryAdd(s.Name, s))
                throw new Exception($"Failed to add {s.Name} Scene");
        }
    }
    
    public Scene? GetScene(string name)
    {
        if (Scenes.TryGetValue(name, out Scene? scene))
        {
            return scene;
        }
        
        return null;
    }
}