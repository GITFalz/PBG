using OpenTK.Windowing.GraphicsLibraryFramework;

public class Scene
{
    public string Name;

    private HashSet<SceneSwitcher> _sceneSwitchers = new HashSet<SceneSwitcher>();

    public Action OnResize = () => { };
    public Action OnAwake;
    public Action OnUpdate = () => { };
    public Action OnFixedUpdate = () => { };
    public Action OnRender = () => { };
    public Action OnExit = () => { };

    public bool Resize = false;
    public string ScenePath = "";


    public RootNode RootNode;


    public static Scene Base () => new("Base");

    public Scene(string name)
    {
        Name = name;
        ScenePath = Path.Combine(Game.uiPath, name + ".ui");
        RootNode = new RootNode(this);

        OnAwake = () =>
        {
            StartInternal();

            OnResize = ResizeInternal;
            OnUpdate = UpdateInternal;
            OnFixedUpdate = FixedUpdateInternal;
            OnRender = RenderInternal;
            OnExit = ExitInternal;

            AwakeInternal();
            OnAwake = AwakeInternal;
        };
    }



    


    // Scene Switching
    public void AddSceneSwitcher(SceneSwitcher sceneSwitcher)
    {
        _sceneSwitchers.Add(sceneSwitcher);
    }

    public void AddNode(MainNode node)
    {
        RootNode.AddNode(node);
    }

    public void AddNode(params MainNode[] node)
    {
        RootNode.AddNode(node);
    }



    // Base functions
    internal void ResizeInternal()
    {
        RootNode.Resize();
    }

    internal void AwakeInternal()
    {
        RootNode.Awake();
    }

    internal void StartInternal()
    {
        if (!File.Exists(ScenePath))
            File.Create(ScenePath).Close();

        RootNode.Start();
    }

    internal void FixedUpdateInternal()
    {
        RootNode.FixedUpdate();
    }

    internal void UpdateInternal()
    {
        foreach (SceneSwitcher sceneSwitcher in _sceneSwitchers)
        {
            if (sceneSwitcher.CanSwitch())
            {
                Game.LoadScene(sceneSwitcher.SceneName);
                return;
            }
        }

        RootNode.Update();
    }

    internal void RenderInternal()
    {
        RootNode.Render();
    }

    internal void ExitInternal()
    {
        RootNode.Exit();

        OnResize = () => {};
        OnUpdate = () => {};
        OnFixedUpdate = () => {};
        OnRender = () => {};

        OnAwake = () =>
        {
            OnResize = ResizeInternal;
            OnUpdate = UpdateInternal;
            OnFixedUpdate = FixedUpdateInternal;
            OnRender = RenderInternal;
            OnExit = ExitInternal;

            AwakeInternal();
            OnAwake = AwakeInternal;
        };

        OnExit = () => {};
    }
}