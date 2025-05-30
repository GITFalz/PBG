using OpenTK.Mathematics;

public class CWorldBiomeNode : CWorldGetterNode
{
    /// <summary>
    /// the height of the biome in blocks, /!\ make sure the value is the height in blocks and not a noise value between 0 and 1
    /// </summary>
    public int Height {
        get => (int)HeightMap.GetCachedValue(HeightMapIndex);
        set => HeightMap.SetValue(value);
    }

    public CWorldGetterNode HeightMap = new CWorldEmptyNode("HeightMap");
    public int HeightMapIndex = 0;

    public override void Init(Vector2 position)
    {
        HeightMap.Init(position);
        CachedValue = Height;
    }

    public override Block GetBlock(int y, int index = 0)
    {
        return HeightMap.GetBlock(y, HeightMapIndex);
    }

    public override CWorldNode Copy()
    {
        return new CWorldBiomeNode()
        {
            Name = Name,
            Height = Height,
        };
    }

    public override float GetCachedValue(int index)
    {
        return index == 0 ? CachedValue : 0;
    }
}