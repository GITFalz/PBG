using System.Reflection.Metadata.Ecma335;
using OpenTK.Mathematics;

public class UIHorizontalCollection : UICollection
{
    public Vector4 Border = (5, 5, 5, 5);
    public float Spacing = 5;

    public UIHorizontalCollection(
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

    public override void Align()
    {
        base.Align();
    }

    public override void CalculateScale()
    {
        float totalOffset = Border.X;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            if (IgnoreInvisibleElements && !element.Visible)
                continue;

            element.SetPositionType(PositionType.Relative);
            element.SetOffset((totalOffset, Border.Y, Border.Z, Border.W));
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

    public override float GetElementScaleX()
    {
        float totalOffset = Border.X;

        for (int i = 0; i < Elements.Count; i++)
        {
            UIElement element = Elements[i];
            if (!element.Visible)
                continue;
            totalOffset += element.GetXScale() + Spacing;
        }

        return totalOffset - Spacing + Border.Z;
    }
}