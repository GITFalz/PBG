using System.Collections.Concurrent;
using OpenTK.Windowing.Common;

public class GameObject
{
    public Transform transform;
    public List<Component> components = new List<Component>();
    
    public GameObject()
    {
        transform = new Transform(this)
        {
            gameObject = this
        };
    }
    
    public static GameObject Create()
    {
        GameObject go = new GameObject();
        go.transform = new Transform(go);
        return go;
    }
    
    public bool AddComponent<T>() where T : Component, new()
    {
        T component = new T();
        
        if (ContainsComponent(component.name))
            return false;
        
        component.transform = transform;
        component.gameObject = this;
        
        components.Add(component);
        return true;
    }
    
    public bool AddComponent<T>(T component) where T : Component, new()
    {
        if (ContainsComponent(component.name))
            return false;
        
        components.Add(component);
        
        component.transform = transform;
        component.gameObject = this;
        
        return true;
    }

    private bool ContainsComponent(string name)
    {
        return components.Any(component => component.name == name);
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
    
    public void Awake()
    {
        foreach (Component component in components)
        {
            component.Awake();
        }
    }
    
    public void Start()
    {
        foreach (Component component in components)
        {
            component.Start();
        }
    }
    
    public void FixedUpdate()
    {
        foreach (Component component in components)
        {
            component.FixedUpdate();
        }
    }
    
    public void Update()
    {
        foreach (Component component in components)
        {
            component.Update();
        }
    }
    
    public void Render()
    {
        foreach (Component component in components)
        {
            component.Render();
        }
    }
    
    public void Exit()
    {
        foreach (Component component in components)
        {
            component.Exit();
        }
    }
}