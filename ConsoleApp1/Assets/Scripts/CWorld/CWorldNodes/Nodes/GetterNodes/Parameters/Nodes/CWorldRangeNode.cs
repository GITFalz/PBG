using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public class CWorldRangeNode : CWorldParameterNode
{
    /// <summary>
    /// the start position on the y axis, /!\ make sure the value is the position in blocks and not a noise value between 0 and 1
    /// </summary>
    public int Start {
        get => (int)StartNode.GetValue();
        set => StartNode.SetValue(value);
    }

    /// <summary>
    /// the height on the y axis, /!\ make sure the value is the height in blocks and not a noise value between 0 and 1
    /// </summary>
    public int Height {
        get => (int)HeightNode.GetValue();
        set => HeightNode.SetValue(value);
    }

    /// <summary>
    /// the state of the block if the y position is between start and start + height
    /// </summary>
    public BlockState TrueState {
        get => _trueState;
        set {
            _trueState = value;
            _trueBlock = BlockHelper.GetDefault(value);
        }
    }
    private BlockState _trueState;
    private Block _trueBlock;

    /// <summary>
    /// the state of the block if the y position is outside of start and start + height
    /// </summary>
    public BlockState FalseState {
        get => _falseState;
        set {
            _falseState = value;
            _falseBlock = BlockHelper.GetDefault(value);
        } 
    }
    private BlockState _falseState;
    private Block _falseBlock;

    /// <summary>
    /// if flipped the height is inverted, so the block will be placed between start - height and start
    /// </summary>
    public bool Flipped = false;

    public CWorldGetterNode StartNode = new CWorldEmptyNode("StartNode");
    public CWorldGetterNode HeightNode = new CWorldEmptyNode("HeightNode");

    public CWorldRangeNode() : base() 
    { 
        FalseState = BlockState.Air;
        TrueState = BlockState.Solid;
    }

    public override void Init(Vector2 position)
    {
        StartNode.Init(position);
        HeightNode.Init(position);
    }

    public override float GetValue()
    {
        return (!Flipped) ? (Start + Height) : (Start - Height);
    }

    public override Block GetBlock(int y)
    {
        return IsInRange(y) ? _trueBlock : _falseBlock;
    }

    public override bool GetBlock(int y, [NotNullWhen(true)] out Block block)
    {
        block = _falseBlock;
        if (!IsInRange(y)) 
            return false;

        block = _trueBlock;
        return true;
    }

    public bool IsInRange(int y)
    {
        int start = Start; int height = Height;
        return (!Flipped) ? (y >= Start && y < start + height) : (y >= start - height && y < start);
    }

    public override CWorldNode Copy()
    {
        return new CWorldRangeNode()
        {
            Name = Name,
            Start = Start,
            Height = Height,
        };
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (StartNode.IsntEmpty())
        {
            string startName = nodeNameMap[StartNode];
            ((CWorldRangeNode)copiedNode).StartNode = (CWorldGetterNode)copiedNodes[startName];
        }
        if (HeightNode.IsntEmpty())
        {
            string heightName = nodeNameMap[HeightNode];
            ((CWorldRangeNode)copiedNode).HeightNode = (CWorldGetterNode)copiedNodes[heightName];
        }
        ((CWorldRangeNode)copiedNode).Flipped = Flipped;
    }
}