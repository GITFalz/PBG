public abstract class Component
{
    public string Name;
    public GameObject GameObject;
    public Transform Transform;

    public Component()
    {
        Name = GetType().Name;
        GameObject = new GameObject();
        Transform = GameObject.transform;
    }

    public virtual void OnResize()
    {
        
    }

    public virtual void Awake()
    {
        
    }
    
    public virtual void Start()
    {
        
    }
    
    public virtual void Update()
    {
        
    }
    
    public virtual void FixedUpdate()
    {

    }
    
    public virtual void Render()
    {

    }
    
    public virtual void Exit()
    {
        
    }
}