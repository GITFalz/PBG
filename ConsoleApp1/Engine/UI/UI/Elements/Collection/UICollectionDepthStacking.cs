using OpenTK.Mathematics;

public class UICollectionDepthStacking : UICollection
{
    public UICollectionDepthStacking(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
    }

    public override void Align()
    {
        OnAlign?.Invoke();
        base.Align();
        OnAlign = null;
    }

    protected override void Init()
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            element.Depth = i * 0.5f;
        }
    }
}