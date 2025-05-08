public abstract class CWorldParameterNode : CWorldGetterNode
{
    public abstract void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap);
}