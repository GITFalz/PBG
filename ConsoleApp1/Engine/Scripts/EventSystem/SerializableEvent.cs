using System.Reflection;
using System.Xml.Serialization;

[Serializable]
public class SerializableEvent
{
    public string MethodName = null!;
    public string TargetName = null!;
    public bool IsStatic = false;
    
    [XmlIgnore]
    private Action? _action;
    public string? FixedParameter = null;
    
    public SerializableEvent() {}
    
    public SerializableEvent(Action? action)
    {
        _action = action;
        if (action?.Target == null)
        {
            if (action?.Method == null) 
                return;
            
            IsStatic = true;
            MethodName = action.Method.Name;
            TargetName = action.Method.DeclaringType?.FullName ?? string.Empty;
            return;
        }
        
        MethodName = action.Method.Name;
        TargetName = action.Target.GetType().Name;
    }
    
    public void Invoke()
    {
        _action?.Invoke();
    }
    
    public void SetAction(Action action)
    {
        _action = action;
    }
    
    public bool BindStaticMethod(string methodName, string targetTypeName, string? fixedParameter = null)
    {
        var targetType = Type.GetType(targetTypeName);
        var methodInfo = targetType?.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        
        if (targetType == null || methodInfo == null) 
            return false;
    
        MethodName = methodName;
        TargetName = targetType.FullName ?? targetType.Name;
        IsStatic = true;
        
        return AssignMethod(methodInfo, null, fixedParameter);
    }
    
    public bool BindMethod(string methodName, object target, string? fixedParameter = null)
    {
        var targetType = target.GetType();
        var methodInfo = targetType.GetMethod(methodName);

        if (methodInfo == null) 
            return false;
    
        MethodName = methodName;
        TargetName = targetType.Name;
        IsStatic = false;
        
        return AssignMethod(methodInfo, target, fixedParameter);
    }

    public bool AssignMethod(MethodInfo methodInfo, object? target, string? fixedParameter)
    {
        FixedParameter = fixedParameter;
        if (fixedParameter == null)
            _action = (Action)Delegate.CreateDelegate(typeof(Action), target, methodInfo);
        else
            _action = () => ((Action<string>)Delegate.CreateDelegate(typeof(Action<string>), target, methodInfo))(fixedParameter);
        return true;
    }
}