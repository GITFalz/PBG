using OpenTK.Windowing.Common;

namespace ConsoleApp1.Engine.Scripts.Core;

public class GameObject
{
    public Transform transform;
    public List<Component> components = new List<Component>();
    public List<Updateable> updateables = new List<Updateable>();
    
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
        
        components.Add(component);
        return true;
    }
    
    public bool AddComponent<T>(T component) where T : Component, new()
    {
        if (ContainsComponent(component.name))
            return false;
        
        components.Add(component);
        return true;
    }

    private bool ContainsComponent(string name)
    {
        return components.Any(component => component.name == name);
    }
    
    public void InitUpdateables()
    {
        updateables.Clear();
        
        foreach (Component component in components)
        {
            if (component is Updateable updateable)
            {
                updateables.Add(updateable);
            }
        }
    }

    public void Start()
    {
        InitUpdateables();
            
        foreach (Updateable updateable in updateables)
        {
            updateable.Start();
        }
    }
    
    public void Update(FrameEventArgs args)
    {
        foreach (Updateable updateable in updateables)
        {
            updateable.Update(args);
        }
    }
    
    public void Render()
    {
        foreach (Updateable updateable in updateables)
        {
            updateable.Render();
        }
    }
}