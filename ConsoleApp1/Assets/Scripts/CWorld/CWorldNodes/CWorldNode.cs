using OpenTK.Mathematics;

public abstract class CWorldNode
{
    public string Name = "CWorldNode";

    protected CWorldNode() {}
    protected CWorldNode(string name) { Name = name; }

    public void Delete()
    {
        
    }

    public abstract void Init(Vector2 position);
    public abstract CWorldNode Copy();
    public bool IsEmpty() => this is CWorldEmptyNode;
    public bool IsntEmpty() => this is not CWorldEmptyNode;
}