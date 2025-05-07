using OpenTK.Mathematics;

public class CWorldRangeNode : CWorldGetterNode
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
        return (!Flipped) ? (y >= Start && y < Start + Height ? _trueBlock : _falseBlock) : (y >= Start - Height && y < Start ? _trueBlock : _falseBlock);
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
}