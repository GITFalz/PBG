public static class CWorldNodeManager
{
    public static List<CWorldNode> CWorldNodes = [];

    public static Dictionary<string, CWorldNode> GetNodeDictionary()
    {
        Dictionary<string, CWorldNode> cWorldNodeDictionary = [];
        foreach (var node in CWorldNodes)
        {
            while (cWorldNodeDictionary.ContainsKey(node.Name))
                node.Name += "_1";

            cWorldNodeDictionary.Add(node.Name, node);
        }
        return cWorldNodeDictionary;
    }
}