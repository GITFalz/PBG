using OpenTK.Windowing.GraphicsLibraryFramework;

public class Scene
{
    public string Name;

    private HashSet<SceneSwitcher> _sceneSwitchers = new HashSet<SceneSwitcher>();

    public Dictionary<string, SceneComponent> Components = new Dictionary<string, SceneComponent>();

    public Action OnResize = () => { };
    public Action OnAwake;
    public Action OnUpdate = () => { };
    public Action OnFixedUpdate = () => { };
    public Action OnRender = () => { };
    public Action OnExit = () => { };

    public bool Resize = false;
    public string ScenePath = "";


    private GameObject _root;


    public static Scene Base () => new("Base");

    public Scene(string name)
    {
        Name = name;
        ScenePath = Path.Combine(Game.uiPath, name + ".ui");
        _root = new GameObject(this);

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

    public void AddSceneSwitcher(SceneSwitcher sceneSwitcher)
    {
        _sceneSwitchers.Add(sceneSwitcher);
    }

    public void AddGameObject(GameObject gameObject, string[] path)
    {
        if (path.Length == 0 || path[0] == "" || path[0].Trim() == "Root")
        {
            _root.AddChild(gameObject);
            gameObject.Scene = this;
            return;
        }

        _root.AddChildToPath(gameObject, path);
        gameObject.Scene = this;
    }
    
    public void AddGameObject(string[] path, params Component[] components)
    {
        GameObject gameObject = new GameObject(this, _root);
        foreach (Component component in components)
        {
            gameObject.AddComponent(component);
        }
        AddGameObject(gameObject, path);
    }

    public List<GameObjectHierarchy> GetHierarchy()
    {
        return _root.GetHierarchies();
    }

    private void ResizeInternal()
    {
        _root.Resize();
    }

    private void AwakeInternal()
    {
        _root.Awake();
    }

    private void StartInternal()
    {
        _root.Start();

        if (!File.Exists(ScenePath))
            File.Create(ScenePath).Close();

        if (!Components.ContainsKey("ModelingEditor"))
            return;

        /*
        Dictionary<string, ClassMethods> methods = Components["ModelingEditor"].GetMethods();

        foreach (var (className, classMethods) in methods)
        {
            Console.WriteLine(className);
            foreach (var method in classMethods.Methods)
            {
                Console.WriteLine("|----->" + method);
            }
        }
        */
    }

    private void FixedUpdateInternal()
    {
        _root.FixedUpdate();
    }

    private void UpdateInternal()
    {
        foreach (SceneSwitcher sceneSwitcher in _sceneSwitchers)
        {
            if (sceneSwitcher.CanSwitch())
            {
                Game.LoadScene(sceneSwitcher.SceneName);
                return;
            }
        }

        _root.Update();
    }

    private void RenderInternal()
    {
        _root.Render();
    }

    private void ExitInternal()
    {
        _root.Exit();

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


    public void AddComponent(SceneComponent component)
    {
        Components.Add(component.Name, component);
    }

    public void AddComponentClass(string name, object component)
    {
        if (!Components.TryGetValue(name, out SceneComponent? value))
            return;
        if (value.Components.Contains(component))
            return;
        value.Components.Add(component);
    }

    public bool GetClass(string name, out object? component)
    {
        component = null;
        foreach (var (_, value) in Components)
        {
            if (value.GetClass(name, out component))
                return true;
        }

        return false;
    }
}

public abstract class SceneSwitcher(string sceneName)
{
    public readonly string SceneName = sceneName;
    
    public override int GetHashCode()
    {
        return SceneName.GetHashCode();
    }
    
    public abstract bool CanSwitch();
}

public class SceneSwitcherKey(Keys key, string sceneName) : SceneSwitcher(sceneName)
{
    public override bool CanSwitch()
    {
        return Input.IsKeyPressed(key);
    }
}

public class SceneSwitcherKeys(Keys[] keys, string sceneName) : SceneSwitcher(sceneName)
{
    public override bool CanSwitch()
    {
        return Input.AreAllKeysDown(keys);
    }
}