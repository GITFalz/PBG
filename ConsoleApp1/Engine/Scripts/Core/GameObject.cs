public class GameObject
{
    public string Name = "Root";
    public Transform transform;
    private GameObject _root;
    private GameObject _parent;
    public List<Component> components = new List<Component>();
    public List<GameObject> children = new List<GameObject>();
    public Scene Scene;

    public GameObject()
    {
        Scene = Scene.Base();
        transform = new Transform(this);
        _root = this;
        _parent = this;
    }
    
    public GameObject(Scene scene)
    {
        transform = new Transform(this);
        _root = this;
        _parent = this;
        Scene = scene;
    }

    public GameObject(Scene scene, GameObject root)
    {
        transform = new Transform(this);
        _root = root;
        _parent = root;
        Scene = scene;
    }

    public GameObject(Scene scene, GameObject root, GameObject parent)
    {
        transform = new Transform(this);
        _root = root;
        _parent = parent;
        Scene = scene;
    }

    public void AddChildToPath(GameObject gameObject, string[] path)
    {
        if (path.Length == 0)
        {
            AddChild(gameObject);
            return;
        }

        GameObject parent = GetChild(path[0].Trim());
        if (parent.IsRoot()) AddChild(gameObject);
        else parent.AddChildToPath(gameObject, [.. path.Skip(1)]);
    }

    public void AddChild(GameObject gameObject)
    {
        children.Add(gameObject);
        gameObject.Scene = Scene;
        gameObject._root = _root;
        gameObject._parent = this;
        gameObject.SetNextBasicName();
    }

    public GameObject NewChild()
    {
        GameObject gameObject = new GameObject(Scene, _root, this);
        children.Add(gameObject);
        gameObject.SetNextBasicName();
        return gameObject;
    }

    public GameObject GetChild(string name)
    {
        foreach (GameObject child in children)
        {
            if (child.Name == name)
                return child;
        }

        return _root;
    }

    public GameObject? Find(string name)
    {
        if (Name == name)
            return this;

        foreach (GameObject child in children)
        {
            GameObject? gameObject = child.Find(name);
            if (gameObject != null)
                return gameObject;
        }

        return null;
    }

    private string GetNextBasicName()
    {
        int i = 0;
        while (children.Any(go => go.Name == $"GameObject{i}")) i++;
        return $"GameObject{i}";
    }

    private void SetNextBasicName()
    {
        Name = GetNextBasicName();
    }

    private string GetNextSpecificName(string name)
    {
        int i = 0;
        while (children.Any(go => go.Name == $"{name}{i}")) i++;
        return $"{name}{i}";
    }

    private void SetNextSpecificName(string name)
    {
        Name = GetNextSpecificName(name);
    }

    public List<GameObjectHierarchy> GetHierarchies()
    {
        List<GameObjectHierarchy> hierarchies = new List<GameObjectHierarchy>();
        GetHierarchies(hierarchies, 0);
        return hierarchies;
    }

    public void GetHierarchies(List<GameObjectHierarchy> hierarchies, int offset)
    {
        hierarchies.Add(new GameObjectHierarchy(this, offset));
        foreach (GameObject child in children)
        {
            child.GetHierarchies(hierarchies, offset + 1);
        }
    }

    /// <summary>
    /// Renames the GameObject to the specified name if it is not already taken else it will add a number to the end of the name.
    /// </summary>
    /// <param name="name"></param>
    public void Rename(string name)
    {
        foreach (GameObject child in children)
        {
            if (child.Name == name) {
                SetNextSpecificName(name);
                return;
            }
        }
        Name = name;
    }

    /// <summary>
    /// Checks if the name is already taken and renames the GameObject if it is.
    /// </summary>
    public void CheckName()
    {
        Rename(Name);
    }


    public GameObject? GetParent()
    {
        return IsRoot() ? null : _parent;
    }

    public bool IsRoot()
    {
        return _root == this;
    }
    
    public bool AddComponent<T>() where T : Component, new()
    {
        T component = new T();
        
        if (ContainsComponent(component.Name))
            return false;
        
        component.Transform = transform;
        component.GameObject = this;
        
        components.Add(component);
        return true;
    }
    
    public bool AddComponent<T>(T component) where T : Component
    {
        if (ContainsComponent(component.Name))
            return false;
        
        components.Add(component);
        
        component.Transform = transform;
        component.GameObject = this;
        
        return true;
    }

    private bool ContainsComponent(string name)
    {
        return components.Any(component => component.Name == name);
    }

    
    /// <summary>
    /// Returns the first component of type T attached to the GameObject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public T GetComponent<T>() where T : Component
    {
        foreach (Component component in components)
        {
            if (component is T t)
            {
                return t;
            }
        }

        throw new NullReferenceException("Component not found. [Called from GameObject.GetComponent<T>()]");
    }

    public void Resize()
    {
        foreach (Component component in components)
        {
            component.OnResize();
        }

        foreach (GameObject child in children)
        {
            child.Resize();
        }
    }
    
    public void Awake()
    {
        foreach (Component component in components)
        {
            component.Awake();
        }

        foreach (GameObject child in children)
        {
            child.Awake();
        }
    }
    
    public void Start()
    {
        foreach (Component component in components)
        {
            component.Start();
        }

        foreach (GameObject child in children)
        {
            child.Start();
        }
    }
    
    public void FixedUpdate()
    {
        foreach (Component component in components)
        {
            component.FixedUpdate();
        }

        foreach (GameObject child in children)
        {
            child.FixedUpdate();
        }
    }
    
    public void Update()
    {
        foreach (Component component in components)
        {
            component.Update();
        }

        foreach (GameObject child in children)
        {
            child.Update();
        }
    }
    
    public void Render()
    {
        foreach (Component component in components)
        {
            component.Render();
        }

        foreach (GameObject child in children)
        {
            child.Render();
        }
    }
    
    public void Exit()
    {
        foreach (Component component in components)
        {
            component.Exit();
        }

        foreach (GameObject child in children)
        {
            child.Exit();
        }
    }
}

public class GameObjectHierarchy
{
    public GameObject gameObject;
    public int offset;

    public GameObjectHierarchy(GameObject gameObject, int offset)
    {
        this.gameObject = gameObject;
        this.offset = offset;
    }
}