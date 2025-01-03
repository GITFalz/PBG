using OpenTK.Windowing.GraphicsLibraryFramework;

public class Scene : Updateable
{
    public string Name;
    
    private HashSet<SceneSwitcher> _sceneSwitchers = new HashSet<SceneSwitcher>();
    private List<GameObject> _gameObjects = new List<GameObject>();
    
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
    }
    
    public override void Exit()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            gameObject.Exit();
        }
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