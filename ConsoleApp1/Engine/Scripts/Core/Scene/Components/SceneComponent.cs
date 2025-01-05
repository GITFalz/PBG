using System.Reflection;

public class SceneComponent
{
    public string Name;
    public List<object> Components = new List<object>();
    
    public SceneComponent() {}
    
    public SceneComponent(string name)
    {
        Name = name;
    }
    
    public SceneComponent(string name, params object[] components)
    {
        Name = name;
        Components.AddRange(components);
    }
    
    public Dictionary<string, ClassMethods> GetMethods()
    {
        Dictionary<string, ClassMethods> methods = new Dictionary<string, ClassMethods>();
        
        foreach (object component in Components)
        {
            ClassMethods classMethods = GetMethod(component);
            methods.Add(classMethods.ClassName, classMethods);
        }
        
        return methods;
    }
    
    public ClassMethods GetMethod(object target)
    {
        ClassMethods classMethods = new ClassMethods(target.GetType().Name);
        
        var parameterlessMethods = 
            target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetParameters().Length == 0 && !method.IsSpecialName)
            .Select(method => method.Name
        );
            
        foreach (var methodName in parameterlessMethods)
        {
            classMethods.AddMethod(methodName);
        }

        return classMethods;
    }
    
    public bool GetClass(string name, out object? component)
    {
        component = Components.Find(c => c.GetType().Name == name);
        return component != null;
    }
}

public struct ClassMethods(string className)
{
    public string ClassName = className;
    public List<string> Methods = new();

    public void AddMethod(string method)
    {
        Methods.Add(method);
    }
}