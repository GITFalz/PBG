using OpenTK.Mathematics;

public class UIVerticalCollection : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UIVerticalCollection(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, Vector4 border, float spacing, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
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
        float totalOffset = Border.Y;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            element.SetPositionType(PositionType.Relative);
            element.SetOffset((Border.X, totalOffset, 0, 0));
            Console.WriteLine($"{element.Name} - {element.Offset}");
            totalOffset += element.Scale.Y + Spacing;
        }

        Scale = (Scale.X, totalOffset - Spacing + Border.W);
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