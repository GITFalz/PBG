namespace ConsoleApp1.Engine.Scripts.Core;

public class GameObject
{
    public Transform transform;
    public List<Component> components = new List<Component>();
    
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

    private bool ContainsComponent(string name)
    {
        return components.Any(component => component.name == name);
    }
    
    public void Update()
    {
        foreach (Component component in components)
        {
            component.Update();
        }
    }
}