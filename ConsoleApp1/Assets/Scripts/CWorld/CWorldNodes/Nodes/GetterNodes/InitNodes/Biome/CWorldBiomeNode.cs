using OpenTK.Mathematics;

public class CWorldBiomeNode : CWorldGetterNode
{
    /// <summary>
    /// the height of the biome in blocks, /!\ make sure the value is the height in blocks and not a noise value between 0 and 1
    /// </summary>
    public int Height {
        get => (int)HeightMap.CachedValue;
        set => HeightMap.SetValue(value);
    }

    public CWorldGetterNode HeightMap = new CWorldEmptyNode("HeightMap");

    public override void Init(Vector2 position)
    {
        HeightMap.Init(position);
        CachedValue = HeightMap.CachedValue;
    }

    public override Block GetBlock(int y)
    {
        return HeightMap.GetBlock(y);
    }

    public override CWorldNode Copy()
    {
        return new CWorldBiomeNode()
        {
            Name = Name,
            Height = Height,
        };
    }
}