using OpenTK.Mathematics;

public class TransformNode : MainNode
{
    public Vector3 Position = Vector3.Zero;
    public List<ScriptingNode> Children = new List<ScriptingNode>();

    public void AddChild(ScriptingNode child)
    {
        Children.Add(child);
        child.Transform = this;
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
        foreach (ScriptingNode child in Children)
        {
            child.Awake();
        }
    }

    public void Exit()
    {
        foreach (ScriptingNode child in Children)
        {
            child.Exit();
        }
    }

    public void FixedUpdate()
    {
        foreach (ScriptingNode child in Children)
        {
            child.FixedUpdate();
        }
    }

    public void Render()
    {
        foreach (ScriptingNode child in Children)
        {
            child.Render();
        }
    }

    public void Resize()
    {
        foreach (ScriptingNode child in Children)
        {
            child.Resize();
        }
    }

    public void Start()
    {
        foreach (ScriptingNode child in Children)
        {
            child.Start();
        }
    }

    public void Update()
    {
        foreach (ScriptingNode child in Children)
        {
            child.Update();
        }
    }
}