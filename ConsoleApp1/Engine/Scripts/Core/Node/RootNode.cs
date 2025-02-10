public class RootNode : Node
{
    public Scene Scene;
    public List<MainNode> Children = new List<MainNode>();

    public RootNode(Scene scene)
    {
        Scene = scene;
    }

    public void AddNode(MainNode node)
    {
        Children.Add(node);
    }

    public void Resize()
    {
        foreach (MainNode child in Children)
        {
            child.Resize();
        }
    }

    public void Start()
    {
        foreach (MainNode child in Children)
        {
            child.Start();
        }
    }

    public void Awake()
    {
        foreach (MainNode child in Children)
        {
            child.Awake();
        }
    }

    public void Update()
    {
        foreach (MainNode child in Children)
        {
            child.Update();
        }
    }

    public void FixedUpdate()
    {
        foreach (MainNode child in Children)
        {
            child.FixedUpdate();
        }
    }

    public void Render()
    {
        foreach (MainNode child in Children)
        {
            child.Render();
        }
    }

    public void Exit()
    {
        foreach (MainNode child in Children)
        {
            child.Exit();
        }
    }
}