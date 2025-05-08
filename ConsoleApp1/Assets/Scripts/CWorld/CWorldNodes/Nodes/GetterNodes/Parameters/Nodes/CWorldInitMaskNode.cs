using OpenTK.Mathematics;

public class CWorldInitMaskNode : CWorldParameterNode
{
    public CWorldGetterNode ChildNode = new CWorldEmptyNode("ChildNode");
    public CWorldGetterNode MaskNode = new CWorldEmptyNode("MaskNode");

    public float Threshold = 0;
    
    public override void Init(Vector2 position)
    {
        MaskNode.Init(position);
        if (MaskNode.GetValue() > Threshold)
            ChildNode.Init(position);
    }

    public override float GetValue()
    {
        return MaskNode.GetValue() > Threshold ? ChildNode.GetValue() : 0;
    }

    public override CWorldNode Copy()
    {
        return new CWorldInitMaskNode()
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
            ((CWorldInitMaskNode)copiedNode).ChildNode = (CWorldGetterNode)copiedNodes[startName];
        }
        if (MaskNode.IsntEmpty())
        {
            string heightName = nodeNameMap[MaskNode];
            ((CWorldInitMaskNode)copiedNode).MaskNode = (CWorldGetterNode)copiedNodes[heightName];
        }
        ((CWorldInitMaskNode)copiedNode).Threshold = Threshold;
    }
}