using OpenTK.Windowing.GraphicsLibraryFramework;

public class Scene
{
    public string Name;

    private HashSet<SceneSwitcher> _sceneSwitchers = new HashSet<SceneSwitcher>();

    public List<UIController> UIControllers = [];
    public Dictionary<string, SceneComponent> Components = new Dictionary<string, SceneComponent>();

    public bool Started = false;

    public bool Resize = false;
    public string ScenePath = "";


    private GameObject _root;

    public Scene(string name)
    {
        Name = name;
        ScenePath = Path.Combine(Game.uiPath, name + ".ui");
        _root = new GameObject(this);
    }

    public void AddSceneSwitcher(SceneSwitcher sceneSwitcher)
    {
        _sceneSwitchers.Add(sceneSwitcher);
    }

    public void AddGameObject(GameObject gameObject, string[] path)
    {
        if (path.Length == 0 || path[0] == "" || path[0] == "Root")
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

    public void OnResize()
    {
        foreach (UIController uiController in UIControllers)
        {
            uiController.OnResize();
        }
        _root.Resize();
    }

    public void Awake()
    {
        _root.Awake();
    }

    public void Start()
    {
        if (Started)
            return;

        _root.Start();


        if (!File.Exists(ScenePath))
            File.Create(ScenePath).Close();

        Started = true;

        if (!Components.ContainsKey("ModelingEditor"))
            return;

        Dictionary<string, ClassMethods> methods = Components["ModelingEditor"].GetMethods();

        foreach (var (className, classMethods) in methods)
        {
            Console.WriteLine(className);
            foreach (var method in classMethods.Methods)
            {
                Console.WriteLine("|----->" + method);
            }
        }
    }

    public void FixedUpdate()
    {
        _root.FixedUpdate();
    }

    public void Update()
    {
        foreach (UIController uiController in UIControllers)
        {
            uiController.Test();
        }

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

    public void Render()
    {
        _root.Render();

        foreach (UIController uiController in UIControllers)
        {
            uiController.Render();
        }
    }

    public void Exit()
    {
        _root.Exit();
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