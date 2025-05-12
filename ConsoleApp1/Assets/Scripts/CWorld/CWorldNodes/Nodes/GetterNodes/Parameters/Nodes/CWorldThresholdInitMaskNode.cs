using OpenTK.Mathematics;

public class CWorldThresholdInitMaskNode : CWorldParameterNode
{
    public CWorldGetterNode ChildNode = new CWorldEmptyNode("ChildNode");
    public CWorldGetterNode MaskNode = new CWorldEmptyNode("MaskNode");

    public float Threshold = 0;
    
    public override void Init(Vector2 position)
    {
        MaskNode.Init(position);
        if (MaskNode.CachedValue > Threshold)
        {
            ChildNode.Init(position);
            CachedValue = ChildNode.CachedValue;
        }
        else
        {
            CachedValue = 0;
        }
    }

    public override CWorldNode Copy()
    {
        return new CWorldThresholdInitMaskNode()
        {
            Name = Name,
            Threshold = Threshold,
        };
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (ChildNode.IsntEmpty())
        {
            string startName = nodeNameMap[ChildNode];
            ((CWorldThresholdInitMaskNode)copiedNode).ChildNode = (CWorldGetterNode)copiedNodes[startName];
        }
        if (MaskNode.IsntEmpty())
        {
            string heightName = nodeNameMap[MaskNode];
            ((CWorldThresholdInitMaskNode)copiedNode).MaskNode = (CWorldGetterNode)copiedNodes[heightName];
        }
        ((CWorldThresholdInitMaskNode)copiedNode).Threshold = Threshold;
    }
}