using System.Reflection.Metadata.Ecma335;
using OpenTK.Mathematics;

public class UICollectionHorizontalStacking : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UICollectionHorizontalStacking(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, Vector4 border, float spacing, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
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
        float scale = GetTotalScale(Elements, 0) + Border.X + Border.Z + Spacing * (Elements.Count - 1);
        float totalOffset = Border.X;
        Scale = new Vector2(scale, Scale.Y);
        newScale = Scale;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            element.SetPositionType(PositionType.Relative);
            element.SetAnchorType(AnchorType.TopLeft);
            element.SetOffset((totalOffset, element.Offset.Y + Border.Y, 0, 0));
            totalOffset += element.Scale.X + Spacing;
        }
    }
}