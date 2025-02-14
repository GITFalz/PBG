using OpenTK.Mathematics;

public class UICollectionVerticalStacking : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UICollectionVerticalStacking(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, Vector4 border, float spacing, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        Border = border;
        Spacing = spacing;
    }

    public override void Align()
    {
        OnAlign?.Invoke();
        base.Align();
        OnAlign = null;
    }

    protected override void Init()
    {
        float totalOffset = Border.Y;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            element.SetPositionType(PositionType.Relative);
            element.SetAnchorType(AnchorType.TopLeft);

            element.SetOffset((element.Offset.X + Border.X, totalOffset, 0, 0));
            totalOffset += element.Scale.Y + Spacing;
        }
    }
}