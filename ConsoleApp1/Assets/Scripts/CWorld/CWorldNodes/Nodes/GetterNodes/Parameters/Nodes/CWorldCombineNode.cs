using OpenTK.Mathematics;

public class CWorldCombineNode : CWorldParameterNode
{
    public float First {
        get => FirstNode.CachedValue;
        set => FirstNode.SetValue(value);
    }

    public float Second {
        get => SecondNode.CachedValue;
        set => SecondNode.SetValue(value);
    }

    public CWorldGetterNode FirstNode = new CWorldEmptyNode("FirstNode");
    public CWorldGetterNode SecondNode = new CWorldEmptyNode("SecondNode");

    public override void Init(Vector2 position)
    {
        FirstNode.Init(position);
        SecondNode.Init(position);
    }

    public override Block GetBlock(int y)
    {
        if (FirstNode.GetBlock(y, out Block firstBlock))
            return firstBlock;

        if (SecondNode.GetBlock(y, out Block secondBlock))
            return secondBlock;
        
        return Block.Air;
    }

    public override bool GetBlock(int y, out Block block)
    {
        if (FirstNode.GetBlock(y, out Block firstBlock))
        {
            block = firstBlock;
            return true;
        }

        if (SecondNode.GetBlock(y, out Block secondBlock))
        {
            block = secondBlock;
            return true;
        }
        
        block = Block.Air;
        return false;
    }

    public override CWorldNode Copy()
    {
        return new CWorldCombineNode()
        {
            Name = Name,
            First = First,
            Second = Second,
        };
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (FirstNode.IsntEmpty())
        {
            string firstName = nodeNameMap[FirstNode];
            ((CWorldCombineNode)copiedNode).FirstNode = (CWorldGetterNode)copiedNodes[firstName];
        }
        if (SecondNode.IsntEmpty())
        {
            string secondName = nodeNameMap[SecondNode];
            ((CWorldCombineNode)copiedNode).SecondNode = (CWorldGetterNode)copiedNodes[secondName];
        }
    }
}