using OpenTK.Mathematics;

public class UIVerticalCollection : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UIVerticalCollection(
        string name, 
        UIController controller,
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        Vector4 border, 
        float spacing, 
        float rotation) : 
        base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
    {
        Border = border;
        Spacing = spacing;
    }

    public UIVerticalCollection(string name, UIController controller, AnchorType anchorType, PositionType positionType) : 
    this(name, controller, anchorType, positionType, (0, 0, 0), (0, 0), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0) { }

    public override void Align()
    {
        base.Align();
    }

    public override void CalculateScale()
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i].OnAlign?.Invoke(); // Allows child collections to have the correct scale before alignment
        }

        float totalOffset = Border.Y;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            if (IgnoreInvisibleElements && !element.Visible)
                continue;

            element.SetPositionType(PositionType.Relative);
            element.SetOffset((Border.X, totalOffset, Border.Z, Border.W));
            totalOffset += element.Scale.Y + Spacing;
        }

        Scale = (Scale.X, totalOffset - Spacing + Border.W);
        Scale = Scale;
        OnAlign = null;
    }

    public override float GetElementScaleY()
    {
        float totalOffset = Border.Y;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            if (!element.Visible)
                continue;
            totalOffset += element.GetYScale() + Spacing;
        }

        return totalOffset - Spacing + Border.W;
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