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
    public static Game? Instance = null;
    
    public static int Width;
    public static int Height;
    
    public static int CenterX;
    public static int CenterY;

    // User paths
    public static string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Engines", "PBG");
    public static string chunkPath = Path.Combine(mainPath, "Chunks");
    public static string nodePath = Path.Combine(mainPath, "Nodes");
    public static string worldNoiseNodePath = Path.Combine(nodePath, "WorldNoiseNodes");
    public static string worldNoiseNodeNodeEditorPath = Path.Combine(worldNoiseNodePath, "WorldNoiseNodeEditorNodes");    // path for the files that hold the data for the nodes in the world noise editor
    public static string worldNoiseNodeCWorldPath = Path.Combine(worldNoiseNodePath, "WorldNoiseNodeCWorlds");            // path for the files that hold the data for the cworld nodes used in world generation
    public static string assetPath = Path.Combine(mainPath, "Saves");
    public static string shaderPath = Path.Combine(mainPath, "Shaders");
    public static string uiPath = Path.Combine(assetPath, "UI");
    public static string texturePath = Path.Combine(assetPath, "Textures");
    public static readonly string modelPath = Path.Combine(assetPath, "Models");
    public static readonly string undoModelPath = Path.Combine(mainPath, "UndoModels");



    public static Vector3 BackgroundColor = new Vector3(0.4f, 0.6f, 0.98f);
    public static Camera Camera = Camera.Empty;
    public static Action UpdateCamera = () => { Camera?.Update(); };
    
    public ConcurrentDictionary<string, Scene> Scenes = new ConcurrentDictionary<string, Scene>();

    private Stopwatch stopwatch;


    public static bool MoveTest = true;
    
    
    private bool isRunning = true;
    private Thread _physicsThread;


    private static Scene _worldScene = new Scene("World");
    private static Scene _worldNoiseEditorScene = new Scene("WorldNoiseEditor");
    private static Scene _UIEditorScene = new Scene("UIEditor");
    
    
    public static Scene? CurrentScene;

    // Miscaleanous Ui
    private PopUp _popUp;

    // This is needed because the OnResize method is called before the load method
    private Action _resizeAction = () => { };
    
    public Game(int width, int height) : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            APIVersion = new Version(4, 6),
            Profile = ContextProfile.Core,
            Title = "OpenGL 4.6"
        })
    {
        Instance = this;
        Camera = new Camera(width, height, new Vector3(0, 0, 0));

        CenterWindow(new Vector2i(width, height));
        //this.VSync = VSyncMode.On;

        _ = new Info();
        _popUp = new PopUp();
        
        Width = width;
        Height = height;
        
        CenterX = width / 2;
        CenterY = height / 2;

        _resizeAction = OnResize;

        /* // Example of using the DataMerger class to merge two SSBOs
        ComputeShader MergeShader = new ComputeShader("DataTransfer/MergeSSBO.compute");

        List<int> largeSSBOData = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        List<int> smallSSBOData = [1, 1, 1, 1, 1];

        SSBO<int> LargeSSBO = new SSBO<int>(largeSSBOData);
        SSBO<int> SmallSSBO = new SSBO<int>(smallSSBOData);

        DataMerger.Merge(LargeSSBO, SmallSSBO, 2);

        int[] data = LargeSSBO.ReadData(10);
        string dataString = string.Join(", ", data);
        Console.WriteLine("Data: " + dataString);
        */
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        
        Width = e.Width;
        Height = e.Height;
        
        CenterX = e.Width / 2;
        CenterY = e.Height / 2;

        UIController.OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -2, 2);

        Camera.SCREEN_WIDTH = Width;
        Camera.SCREEN_HEIGHT = Height;

        _resizeAction.Invoke();
        
        base.OnResize(e);
    }

    public void OnResize()
    {
        _popUp.Resize();
        Info.Resize();
        Timer.Resize();
        Inventory.ResizeAll();

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

        if (!Directory.Exists(nodePath))
            Directory.CreateDirectory(nodePath);
        if (!Directory.Exists(worldNoiseNodePath))
            Directory.CreateDirectory(worldNoiseNodePath);
        if (!Directory.Exists(worldNoiseNodeNodeEditorPath))
            Directory.CreateDirectory(worldNoiseNodeNodeEditorPath);
        if (!Directory.Exists(worldNoiseNodeCWorldPath))
            Directory.CreateDirectory(worldNoiseNodeCWorldPath);

        if (!Directory.Exists(shaderPath))
            Directory.CreateDirectory(shaderPath);
            
        if (!Directory.Exists(texturePath))
            Directory.CreateDirectory(texturePath);

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
        
        BlockChecker grassMask = new BlockChecker(
            new BlockMask((0, 0, 0), new BlockTest(Block.Solid, MaskType.State)),
            new BlockMask((0, 1, 0), new BlockTest(Block.Air, MaskType.State))
        );

        BlockChecker dirtMask = new BlockChecker(
            new BlockMask((0, 0, 0), new BlockTest(Block.Solid, MaskType.State)),
            new BlockMask((0, 1, 0), new BlockTest(Block.Solid, MaskType.State)),
            new BlockMask((0, 2, 0), new BlockTest(Block.Solid, MaskType.State), new BlockTest(Block.Air, MaskType.State)),
            new BlockMask((0, 3, 0), new BlockTest(Block.Air, MaskType.State))
        );

        BlockChecker waterMask = new BlockChecker(
            new BlockMask((0, 0, 0), new BlockTest(Block.Liquid, MaskType.State))
        );

        BlockChecker sandMask = new BlockChecker(
            new BlockMask((0, 0, 0), new BlockTest(Block.Solid, MaskType.State)),
            new BlockMask((0, 1, 0), new BlockTest(Block.Liquid, MaskType.State)) 
        );

        CWorldBlock grass = new("grass_block", 0, 1, BlockState.Solid, new([0, 0, 1, 0, 2, 0]), grassMask);
        CWorldBlock dirt = new("dirt_block", 1, 2, BlockState.Solid, new([2, 2, 2, 2, 2, 2]), dirtMask);
        CWorldBlock sand = new("sand_block", 2, 3, BlockState.Solid, new([4, 4, 4, 4, 4, 4]), sandMask);
        CWorldBlock water = new("water_block", 3, 4, BlockState.Liquid, new([5, 5, 5, 5, 5, 5]), waterMask);
        CWorldBlock stone = new("stone_block", 4, 5, BlockState.Solid, new([3, 3, 3, 3, 3, 3]), null);
        
        BlockManager.Add(grass);
        BlockManager.Add(dirt);
        BlockManager.Add(sand);
        BlockManager.Add(water);
        BlockManager.Add(stone);

        BlockManager.GetSortedPriorityList();

        var grassBlockData = new BlockItemData(grass);
        var dirtBlockData = new BlockItemData(dirt);
        var sandBlockData = new BlockItemData(sand);
        var waterBlockData = new BlockItemData(water);
        var stoneBlockData = new BlockItemData(stone);

        ItemDataManager.GenerateIcons();

        // Menu
        TransformNode menuNode = new TransformNode();
        MenuManager menuManager = new MenuManager();
        menuNode.AddChild(menuManager);
        
        // World
        TransformNode worldGenerationNode = new TransformNode();
        worldGenerationNode.AddChild(new WorldManager());

        TransformNode playerNode = new TransformNode();
        playerNode.AddChild(new PlayerStateMachine(), new PhysicsBody());

        TransformNode InventoryNode = new TransformNode();
        InventoryNode.AddChild(new PlayerInventoryManager());

        TransformNode SelectedItemNode = new TransformNode();
        SelectedItemNode.AddChild(new SelectedItemManager());

        // World noise
        NoiseEditor noiseEditor = new NoiseEditor();
        TransformNode noiseEditorNode = new TransformNode();
        noiseEditorNode.AddChild(noiseEditor);

        // UI
        TransformNode uiNode = new TransformNode();
        uiNode.AddChild(new UIEditor());

        _worldScene.AddNode(playerNode, worldGenerationNode, InventoryNode, SelectedItemNode, menuNode);
        _worldNoiseEditorScene.AddNode(noiseEditorNode, menuNode);
        _UIEditorScene.AddNode(uiNode, menuNode);

        AddScenes(_worldScene, _worldNoiseEditorScene, _UIEditorScene);
        //LoadScene("WorldNoiseEditor");
        LoadScene("World");

        _physicsThread = new Thread(PhysicsThread);
        _physicsThread.Start();

        GL.Enable(EnableCap.DepthTest);

        Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Renderer));

        Info.GPUText.SetText($"GPU: {GL.GetString(StringName.Renderer)}", 1.2f);

        base.OnLoad();
    }
    
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        Input.PressedKeys.Add(e.Key);

        UIController.InputField(e.Key);

        if (e.Key == Keys.P)
        {
            Camera.SetCameraMode(Camera.GetCameraMode() == CameraMode.Follow ? CameraMode.Free : CameraMode.Follow);
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

    public static void CloseGame()
    {
        Instance?.Close();
    }
    
    protected override void OnUnload()
    {
        CurrentScene?.OnExit();
        
        BufferBase.Delete();
        IDBOBase.Delete();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        isRunning = false;
        _physicsThread.Join();
        
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
        Info.Render();
        Timer.Render();
        
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
        if (Instance != null && Instance.Scenes.TryGetValue(sceneName, out Scene? scene) && CurrentScene != scene)
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