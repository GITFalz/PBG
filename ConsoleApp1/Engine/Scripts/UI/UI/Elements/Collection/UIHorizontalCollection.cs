using System.Reflection.Metadata.Ecma335;
using OpenTK.Mathematics;

public class UIHorizontalCollection : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UIHorizontalCollection(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, Vector4 border, float spacing, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        Border = border;
        Spacing = spacing;
    }

    public override void Align()
    {
        OnAlign?.Invoke();
        base.Align();
    }

    protected override void Init()
    {
        float totalOffset = Border.X;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            element.SetPositionType(PositionType.Relative);
            element.SetOffset((totalOffset, Border.Y, 0, 0));
            totalOffset += element.Scale.X + Spacing;
        }

        Scale = (totalOffset - Spacing + Border.Z, Scale.Y);
        newScale = Scale;

        OnAlign = null;
    }

    public override void SetSpacing(float spacing)
    {
        Spacing = spacing;
    }

    public override void SetBorder(Vector4 border)
    {
        Border = border;
    }
}