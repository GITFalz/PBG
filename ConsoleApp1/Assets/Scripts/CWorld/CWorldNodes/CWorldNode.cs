public abstract class CWorldNode
{
    public string Name = "CWorldNode";

    protected CWorldNode() {}
    protected CWorldNode(string name) { Name = name; }

    public void Delete()
    {
        
    }

    public abstract CWorldNode Copy();
    public bool IsEmpty() => Name == "Empty";
    public bool IsntEmpty() => Name != "Empty";
}