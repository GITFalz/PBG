using OpenTK.Mathematics;

public class TransformNode : MainNode
{
    public static TransformNode Empty = new();

    public Vector3 Position = Vector3.Zero;
    public List<ScriptingNode> Children = new List<ScriptingNode>();

    public List<Action> OnStart = [];
    public List<Action> OnAwake = [];
    public List<Action> OnResize = [];
    public List<Action> OnUpdate = [];
    public List<Action> OnFixedUpdate = [];
    public List<Action> OnRender = [];
    public List<Action> OnExit = [];

    public void AddChild(ScriptingNode child)
    {
        Children.Add(child);
        child.Transform = this;

        if (child.GetAction("Start", out Action? action)) OnStart.Add(action);
        if (child.GetAction("Awake", out action)) OnAwake.Add(action);
        if (child.GetAction("Resize", out action)) OnResize.Add(action);
        if (child.GetAction("Update", out action)) OnUpdate.Add(action);
        if (child.GetAction("FixedUpdate", out action)) OnFixedUpdate.Add(action);
        if (child.GetAction("Render", out action)) OnRender.Add(action);
        if (child.GetAction("Exit", out action)) OnExit.Add(action);
    }

    public T GetComponent<T>() where T : ScriptingNode
    {
        foreach (ScriptingNode child in Children)
        {
            if (child is T)
                return (T)child;
        }
        throw new Exception("Component not found");
    }

    public void AddChild(params ScriptingNode[] child)
    {
        foreach (ScriptingNode c in child) 
            AddChild(c);
    }

    public void Awake()
    {
        foreach (var action in OnAwake)
        {
            action.Invoke();
        }
    }

    public void Exit()
    {
        foreach (var action in OnExit)
        {
            action.Invoke();
        }
    }

    public void FixedUpdate()
    {
        foreach (var action in OnFixedUpdate)
        {
            action.Invoke();
        }
    }

    public void Render()
    {
        foreach (var action in OnRender)
        {
            action.Invoke();
        }
    }

    public void Resize()
    {
        foreach (var action in OnResize)
        {
            action.Invoke();
        }
    }

    public void Start()
    {
        foreach (var action in OnStart)
        {
            action.Invoke();
        }
    }

    public void Update()
    {
        foreach (var action in OnUpdate)
        {
            action.Invoke();
        }
    }
}