using System.Collections.Concurrent;
using System.Diagnostics;
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
    public static string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Engines", "ModelingEngine");
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

    public static bool MoveTest = false;

    private static Scene _modelingScene = new Scene("Modeling");
    
    public static Scene? CurrentScene;

    // Miscaleanous Ui
    private PopUp _popUp;
    
    
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
        Game.Width = width;
        Game.Height = height;
        
        Game.CenterX = width / 2;
        Game.CenterY = height / 2;
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        
        Game.Width = e.Width;
        Game.Height = e.Height;
        
        CenterX = e.Width / 2;
        CenterY = e.Height / 2;
        
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

        TransformNode modelingNode = new TransformNode();

        modelingNode.AddChild(new GeneralModelingEditor());
        
        _modelingScene.AddNode(modelingNode);
        
        AddScenes(_modelingScene);
        LoadScene("Modeling");

        _popUp = new PopUp();
        
        GL.Enable(EnableCap.DepthTest);
        
        base.OnLoad();
    }
    
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!Input.PressedKeys.Contains(e.Key))
            Input.PressedKeys.Add(e.Key);
        UIController.InputField(e.Key);
    }
    
    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        Input.PressedKeys.Remove(e.Key);
    }
    
    protected override void OnUnload()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        CurrentScene?.OnExit();
        
        base.OnUnload();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Sky blue background
        GL.ClearColor(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Viewport(0, 0, Width, Height);
        
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
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        Input.Update(keyboard, mouse);
        GameTime.Update(args);

        if (Input.IsKeyPressed(Keys.LeftAlt))
        {
            MoveTest = !MoveTest;

            if (MoveTest)
            {
                Game.SetCursorState(CursorState.Grabbed);
            }
            else
            {
                Game.SetCursorState(CursorState.Normal);
            }
        }

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