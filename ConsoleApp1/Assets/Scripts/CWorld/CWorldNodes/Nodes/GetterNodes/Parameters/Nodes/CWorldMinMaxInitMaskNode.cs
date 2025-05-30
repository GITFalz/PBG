using OpenTK.Mathematics;

public class CWorldMinMaxInitMaskNode : CWorldParameterNode
{
    public CWorldGetterNode ChildNode = new CWorldEmptyNode("ChildNode");
    public int ChildValueIndex = 0;
    public CWorldGetterNode MaskNode = new CWorldEmptyNode("MaskNode");
    public int MaskValueIndex = 0;

    public float Min = 0;
    public float Max = 1;
    
    public override void Init(Vector2 position)
    {
        MaskNode.Init(position);
        if (MaskNode.GetCachedValue(MaskValueIndex) >= Min && MaskNode.GetCachedValue(MaskValueIndex) < Max)
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
        return new CWorldMinMaxInitMaskNode()
        {
            Name = Name,
            Min = Min,
            Max = Max,
            ChildValueIndex = ChildValueIndex,
            MaskValueIndex = MaskValueIndex,
        };
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (ChildNode.IsntEmpty())
        {
            string startName = nodeNameMap[ChildNode];
            ((CWorldMinMaxInitMaskNode)copiedNode).ChildNode = (CWorldGetterNode)copiedNodes[startName];
        }
        if (MaskNode.IsntEmpty())
        {
            string heightName = nodeNameMap[MaskNode];
            ((CWorldMinMaxInitMaskNode)copiedNode).MaskNode = (CWorldGetterNode)copiedNodes[heightName];
        }
        ((CWorldMinMaxInitMaskNode)copiedNode).Min = Min;
        ((CWorldMinMaxInitMaskNode)copiedNode).Max = Max;
    }
}