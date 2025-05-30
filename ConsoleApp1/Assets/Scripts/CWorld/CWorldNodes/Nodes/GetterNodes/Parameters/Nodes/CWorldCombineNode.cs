using OpenTK.Mathematics;

public class CWorldCombineNode : CWorldParameterNode
{
    public float First {
        get => FirstNode.GetCachedValue(FirstValueIndex);
        set => FirstNode.SetValue(value);
    }

    public float Second {
        get => SecondNode.GetCachedValue(SecondValueIndex);
        set => SecondNode.SetValue(value);
    }

    public CWorldGetterNode FirstNode = new CWorldEmptyNode("FirstNode");
    public int FirstValueIndex = 0;
    public CWorldGetterNode SecondNode = new CWorldEmptyNode("SecondNode");
    public int SecondValueIndex = 1;

    public override void Init(Vector2 position)
    {
        FirstNode.Init(position);
        SecondNode.Init(position);
    }
    
    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }

    public override Block GetBlock(int y, int index = 0)
    {
        if (FirstNode.GetBlock(y, out Block firstBlock, FirstValueIndex))
            return firstBlock;

        if (SecondNode.GetBlock(y, out Block secondBlock, SecondValueIndex))
            return secondBlock;

        return Block.Air;
    }

    public override bool GetBlock(int y, out Block block, int index = 0)
    {
        if (FirstNode.GetBlock(y, out Block firstBlock, FirstValueIndex))
        {
            block = firstBlock;
            return true;
        }

        if (SecondNode.GetBlock(y, out Block secondBlock, SecondValueIndex))
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
            FirstValueIndex = FirstValueIndex,
            SecondValueIndex = SecondValueIndex,
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