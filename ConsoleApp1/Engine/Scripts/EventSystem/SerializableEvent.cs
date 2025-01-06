using System.Xml.Serialization;

[Serializable]
public class SerializableEvent
{
    public string MethodName = null!;
    public string TargetName = null!;
    
    [XmlIgnore]
    private Action? _action;
    public string? FixedParameter = null;
    
    public SerializableEvent() {}
    
    public SerializableEvent(Action? action)
    {
        _action = action;
        if (action?.Target == null) 
            return;
        
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

    public void Bind(object target)
    {
        if (string.IsNullOrEmpty(TargetName)) return;

        var targetName = Type.GetType(TargetName);
        var methodInfo = targetName?.GetMethod(MethodName);
        
        if (methodInfo != null && target.GetType() == targetName)
            _action = (Action) Delegate.CreateDelegate(typeof(Action), target, methodInfo);
    }

    public bool BindMethod(string methodName, string target)
    {
        object? type = Type.GetType(target);
        return type != null && BindMethod(methodName, type);
    }
    
    public bool BindMethod(string methodName, object target)
    {
        var targetType = target.GetType();
        var methodInfo = targetType.GetMethod(methodName);

        if (methodInfo == null) 
            return false;
        
        MethodName = methodName;
        TargetName = targetType.Name;
        _action = (Action) Delegate.CreateDelegate(typeof(Action), target, methodInfo);
        return true;
    }
    
    
    public bool BindMethod(string methodName, object target, string fixedParameter)
    {
        var targetType = target.GetType();
        var methodInfo = targetType.GetMethod(methodName);

        if (methodInfo == null) 
            return false;
    
        MethodName = methodName;
        TargetName = targetType.Name;

        FixedParameter = fixedParameter;
        _action = () => ((Action<string>)Delegate.CreateDelegate(typeof(Action<string>), target, methodInfo))(fixedParameter);
        return true;
    }
}