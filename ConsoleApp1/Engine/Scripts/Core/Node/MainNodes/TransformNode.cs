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
        for (int i = 0; i < OnAwake.Count; i++)
        {
            OnAwake[i].Invoke();
        }
    }

    public void Exit()
    {
        for (int i = 0; i < OnExit.Count; i++)
        {
            OnExit[i].Invoke();
        }
    }

    public void FixedUpdate()
    {
        for (int i = 0; i < OnFixedUpdate.Count; i++)
        {
            OnFixedUpdate[i].Invoke();
        }
    }

    public void Render()
    {
        for (int i = 0; i < OnRender.Count; i++)
        {
            OnRender[i].Invoke();
        }
    }

    public void Resize()
    {
        for (int i = 0; i < OnResize.Count; i++)
        {
            OnResize[i].Invoke();
        }
    }

    public void Start()
    {
        for (int i = 0; i < OnStart.Count; i++)
        {
            OnStart[i].Invoke();
        }
    }

    public void Update()
    {
        for (int i = 0; i < OnUpdate.Count; i++)
        {
            OnUpdate[i].Invoke();
        }
    }
}