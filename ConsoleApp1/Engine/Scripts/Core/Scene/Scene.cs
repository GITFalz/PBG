using OpenTK.Windowing.GraphicsLibraryFramework;

public class Scene : Updateable
{
    public string Name;

    private HashSet<SceneSwitcher> _sceneSwitchers = new HashSet<SceneSwitcher>();
    private List<GameObject> _gameObjects = new List<GameObject>();

    public UIController? UiController = null;
    public Dictionary<string, SceneComponent> Components = new Dictionary<string, SceneComponent>();

    private bool _started = false;

    public Scene(string name)
    {
        Name = name;
    }

    public bool Started => _started;

    public void AddSceneSwitcher(SceneSwitcher sceneSwitcher)
    {
        _sceneSwitchers.Add(sceneSwitcher);
    }

    public void AddGameObject(GameObject gameObject)
    {
        _gameObjects.Add(gameObject);
        gameObject.Scene = this;
    }

    public override void Awake()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.Awake();
        }
    }

    public override void Start()
    {
        if (_started)
            return;

        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.Start();
        }

        _started = true;

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

    public override void FixedUpdate()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.FixedUpdate();
        }
    }

    public override void Update()
    {
        foreach (SceneSwitcher sceneSwitcher in _sceneSwitchers)
        {
            if (sceneSwitcher.CanSwitch())
            {
                Game.LoadScene(sceneSwitcher.SceneName);
                return;
            }
        }

        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.Update();
        }
    }

    public override void Render()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            foreach (Component component in gameObject.components)
            {
                component.Render();
            }
        }

        UiController?.Render();
    }

    public override void Exit()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.Exit();
        }
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

    public void LoadUi()
    {
        Console.WriteLine(Path.Combine("Load: " + Game.uiPath, $"{Name}.ui"));
        
        string[] lines = File.ReadAllLines(Path.Combine(Game.uiPath, $"{Name}.ui"));
        if (lines[0].Split(":")[1].Trim() != Name)
            return;

        UIController? controller;
        try
        {
            controller = UiController;
        }
        catch (Exception e)
        {
            Console.WriteLine("Scene: " + Name + " with error: " + e);
            return;
        }
        
        if (controller == null)
            return; 
            
        UiLoader.Load(controller, lines);
    }

    public void SaveUi()
    {
        if (UiController == null)
            return;
        
        List<string> lines = new List<string>() { "Scene: " + Name, ""};
        
        foreach (var element in UiController.GetUiElements())
        {
            lines.AddRange(element.ToLines(0));
        }
        
        File.WriteAllLines(Path.Combine(Game.uiPath, $"{Name}.ui"), lines);
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