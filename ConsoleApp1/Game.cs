using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Game : GameWindow
{
    public static Game Instance;
    
    public static int width;
    public static int height;
    
    public static int centerX;
    public static int centerY;

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
    public static Camera camera;
    
    public ConcurrentDictionary<string, Scene> Scenes = new ConcurrentDictionary<string, Scene>();



    private int frameCount = 0;
    private float elapsedTime = 0;
    private Stopwatch stopwatch;


    public static bool MoveTest = false;
    
    
    private bool isRunning = true;
    private Thread _physicsThread;


    private static Scene _worldScene = new Scene("World");
    
    
    public static Scene? CurrentScene;

    // Miscaleanous Ui
    private PopUp _popUp;

    public static Action<Keys> InputActions = (e) => { };
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            APIVersion = new Version(4, 6),
            Profile = ContextProfile.Core,
            Title = "OpenGL 4.6"
        })
    {
        Instance = this;

        camera = new Camera(width, height, new Vector3(0, 0, 0));
        
        CenterWindow(new Vector2i(width, height));
        Game.width = width;
        Game.height = height;
        
        Game.centerX = width / 2;
        Game.centerY = height / 2;
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        
        Game.width = e.Width;
        Game.height = e.Height;
        
        centerX = e.Width / 2;
        centerY = e.Height / 2;
        
        try
        {
            UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, e.Width, e.Height, 0, -2, 2);
            ModelSettings.Camera?.UpdateProjectionMatrix(e.Width, e.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        CurrentScene?.OnResize();

        foreach (var (_, scene) in Scenes)
        {
            if (scene != CurrentScene)
                scene.Resize = true;
        }
        
        base.OnResize(e);
    }
    
    protected override void OnLoad()
    {
        // Utils
        stopwatch = new Stopwatch();
        stopwatch.Start();

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
        UVmaps editorUvs = new UVmaps( new int[] { 0, 0, 0, 0, 0, 0 });
        
        //UVmaps grassUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        //UVmaps dirtUvs = new UVmaps( new int[] { 2, 2, 2, 2, 2, 2 });
        //UVmaps stoneUvs = new UVmaps( new int[] { 3, 3, 3, 3, 3, 3 });
    
        
        CWorldBlock editor = new CWorldBlock("editor_block", 1, 1, editorUvs);
        
        //CWorldBlock grass = new CWorldBlock("grass_block", 0, 1, grassUvs);
        //CWorldBlock dirt = new CWorldBlock("dirt_block", 1, 2, dirtUvs);
        //CWorldBlock stone = new CWorldBlock("stone_block", 2, 3, stoneUvs);
        
        BlockManager.Add(editor);
        
        //BlockManager.Add(grass);
        //BlockManager.Add(dirt);
        //BlockManager.Add(stone);
        
        
        // World
        TransformNode worldGenerationNode = new TransformNode();
        worldGenerationNode.AddChild(new WorldManager());

        TransformNode playerNode = new TransformNode();
        playerNode.AddChild(new PhysicsBody(false), new PlayerStateMachine());

        _worldScene.AddNode(worldGenerationNode, playerNode);
        
        AddScenes(_worldScene);
        LoadScene("World");

        _popUp = new PopUp();
    
        _physicsThread = new Thread(PhysicsThread);
        _physicsThread.Start();
        
        GL.Enable(EnableCap.DepthTest);
        
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
            }
            else
            {
                SetCursorState(CursorState.Normal);
            }
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
        
        base.OnUnload();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Sky blue background
        GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Viewport(0, 0, width, height);
        
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        Skybox.Render();
        CurrentScene?.OnRender();
        _popUp.Render();
        
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
        double totalTime = 0;
        
        while (isRunning)
        {
            double time = stopwatch.Elapsed.TotalSeconds;
            
            if (time - totalTime >= GameTime.FixedDeltaTime)
            {
                _worldScene.OnFixedUpdate();
                
                totalTime = time;
            }
            
            Thread.Sleep(1);
        }
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        Input.Update(keyboard, mouse);
        GameTime.Update(args);

        _popUp.Update();
        
        CurrentScene?.OnUpdate();
        
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