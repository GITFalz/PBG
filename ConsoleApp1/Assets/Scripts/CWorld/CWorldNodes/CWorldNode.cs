public class CWorldNode
{
    public string Name = "CWorldNode";

    protected CWorldNode()
    {
        CWorldNodeManager.CWorldNodes.Add(this);
    }

    public void Delete()
    {
        CWorldNodeManager.CWorldNodes.Remove(this);
    }
}