namespace ConsoleApp1.Engine.Scripts.Core;

public abstract class Component
{
    public string name;
    
    public virtual void Update()
    {
        Console.WriteLine("Updating Component: " + name);
    }
}