using OpenTK.Mathematics;

public class CWorldThresholdInitMaskNode : CWorldParameterNode
{
    public CWorldGetterNode ChildNode = new CWorldEmptyNode("ChildNode");
    public int ChildValueIndex = 0;
    public CWorldGetterNode MaskNode = new CWorldEmptyNode("MaskNode");
    public int MaskValueIndex = 0;

    public float Threshold = 0;
    
    public override void Init(Vector2 position)
    {
        MaskNode.Init(position);
        if (MaskNode.GetCachedValue(MaskValueIndex) > Threshold)
        {
            ChildNode.Init(position);
            CachedValue = ChildNode.GetCachedValue(ChildValueIndex); 
        }
        else
        {
            CachedValue = 0; 
        }
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }

    public override CWorldNode Copy()
    {
        return new CWorldThresholdInitMaskNode()
        {
            Name = Name,
            Threshold = Threshold,
            MaskValueIndex = MaskValueIndex,
            ChildValueIndex = ChildValueIndex,
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