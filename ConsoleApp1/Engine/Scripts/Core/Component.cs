public abstract class Component
{
    public string name;
    
    public virtual void Update()
    {
        Console.WriteLine("Updating Component: " + name);
    }

    public virtual void OnResize()
    {
        
    }
}