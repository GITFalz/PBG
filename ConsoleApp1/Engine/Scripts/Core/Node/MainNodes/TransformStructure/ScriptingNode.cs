public abstract class ScriptingNode
{
    public string Name = "ScriptingNode";
    public TransformNode Transform;

    public virtual void Resize() {}
    public virtual void Start() {}
    public virtual void Awake() {}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual void Render() {}
    public virtual void Exit() {}
}