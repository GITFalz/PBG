public class StructureNode : MainNode
{
    public List<MainNode> Children = new List<MainNode>();

    public void AddChild(MainNode child)
    {
        Children.Add(child);
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