public abstract class Component
{
    public string name;
    
    public GameObject gameObject;
    public Transform transform;

    protected virtual void OnResize()
    {
        
    }
    
    protected virtual void Update()
    {
        
    }
    
    public virtual void FixedUpdate()
    {

    }
    
    public virtual void Render()
    {

    }
}