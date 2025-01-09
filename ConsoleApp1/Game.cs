using System.Collections.Concurrent;
using System.Diagnostics;
using ConsoleApp1.Assets.Editors.UiEditor;
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
    public static string mainPath;
    public static string chunkPath;
    
    // Compile time paths
    public static string assetPath;
    public static string uiPath;
    public static string modelPath;

    private Camera _mainCamera;
    
    private WorldManager _worldManager;

    private Action? _updateText;
    
    
    private FBO _fbo;
    
    
    public ConcurrentDictionary<string, Scene> Scenes = new ConcurrentDictionary<string, Scene>();
    
    public static bool cameraMove = false;
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


    private Scene _worldScene = new Scene("World");
    private Scene _animationScene = new Scene("Modeling");
    private Scene _uiEditorScene = new Scene("UIEditor");
    private Scene _uiScene = new Scene("UI");
    
    
    public Scene? CurrentScene { get; private set; }
    
    
    public static HashSet<Action> OnPressedKeys = new HashSet<Action>();
    
    
    // Editor mode
    public EditorMode EditorNode = new EditorMode();
    public Action EditorUpdate;
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        Instance = this;
        
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
            UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, e.Width, e.Height, 0, -1, 1);
            _mainCamera.UpdateProjectionMatrix(e.Width, e.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        CurrentScene?.OnResize();
        
        base.OnResize(e);
    }
    
    protected override void OnLoad()
    {
        // Scenes
        _worldScene.AddSceneSwitcher(new SceneSwitcherKey(Keys.RightShift, "Modeling"));
        _animationScene.AddSceneSwitcher(new SceneSwitcherKeys([Keys.LeftControl, Keys.LeftShift, Keys.Z], "World"));
        _animationScene.AddSceneSwitcher(new SceneSwitcherKeys([Keys.LeftControl, Keys.LeftShift, Keys.U], "UIEditor"));
        _uiEditorScene.AddSceneSwitcher(new SceneSwitcherKeys([Keys.LeftControl, Keys.LeftShift, Keys.Semicolon], "Modeling"));
        
        // Utils
        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        // Camera
        _mainCamera = new Camera(width, height, new Vector3(0, 20, 0));
        _mainCamera.Start();
        
        // File Paths
        mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoxelGame");
        chunkPath = Path.Combine(mainPath, "Chunks");
        
        assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Saves");
        uiPath = Path.Combine(assetPath, "UI");
        modelPath = Path.Combine(assetPath, "Models");
        
        Console.WriteLine(uiPath);
        
        if (!Directory.Exists(chunkPath))
            Directory.CreateDirectory(chunkPath);
        if (!Directory.Exists(modelPath))
            Directory.CreateDirectory(modelPath);
        
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);
        if (!Directory.Exists(uiPath))
            Directory.CreateDirectory(uiPath);
        
        // Input
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        Input.Start(keyboard, mouse);
        
        _visibleCursorSwitch = new KeyboardSwitch(Input.IsKeyPressed);
        
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
        
        // Animation
        new AnimationManager().Start();

        // GameObjects
        GameObject playerObject = new GameObject();
        
        PlayerStateMachine player = new PlayerStateMachine();
        PhysicsBody playerPhysics = new PhysicsBody();
        
        playerPhysics.doGravity = false;
        
        playerObject.AddComponent(player);
        playerObject.AddComponent(playerPhysics);
        
        
        _skyboxMesh = new SkyboxMesh();
        _skyboxShader = new ShaderProgram("Sky/Default.vert", "Sky/Default.frag");
        
        // World setup
        GameObject worldObject = new GameObject();
        _worldManager = new WorldManager();
        worldObject.AddComponent(_worldManager);
        
        _worldScene.AddGameObject(playerObject);
        _worldScene.AddGameObject(worldObject);
        
        
        // Modeling Editor
        ModelingEditor editorComponent = new ModelingEditor();
        
        GameObject animationObject = new GameObject();
        SceneComponent animationComponent = new SceneComponent("ModelingEditor", editorComponent);
        
        animationObject.AddComponent(editorComponent);
        
        _animationScene.AddGameObject(animationObject);
        _animationScene.AddComponent(animationComponent);
        
        // UI
        GameObject uiObject = new GameObject();
        UiEditor uiEditor = new UiEditor();
        
        uiObject.AddComponent(uiEditor);
        
        _uiEditorScene.AddGameObject(uiObject);
        
        AddScenes(_worldScene, _animationScene, _uiScene, _uiEditorScene);
        
        LoadScene("Modeling");
        
        _physicsThread = new Thread(PhysicsThread);
        _physicsThread.Start();
        
        GL.Enable(EnableCap.DepthTest);
        
        base.OnLoad();
    }
    
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        UIController.InputField(e.Key);
    }
    
    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        Input.RemovePressedKey(e.Key);
    }
    
    protected override void OnUnload()
    {
        _worldManager.Delete();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        isRunning = false;
        _physicsThread.Join();
        
        CurrentScene?.Exit();
        
        base.OnUnload();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Sky blue background
        GL.ClearColor(0.4f, 0.6f, 0.98f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Viewport(0, 0, width, height);
        
        /*
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
        */
        
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        CurrentScene?.Render();
        
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
                if (_worldScene.Started)
                    _worldScene.FixedUpdate();
                
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

        if (Input.IsKeyPressed(Keys.LeftAlt))
            MoveTest = !MoveTest;
        
        CurrentScene?.Update();
        
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
            Instance.CurrentScene?.Exit();
            Instance.CurrentScene = scene;
            scene.Start();
            scene.Awake();
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