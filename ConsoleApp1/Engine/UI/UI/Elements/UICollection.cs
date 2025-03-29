using OpenTK.Mathematics;

public class UICollection : UIElement
{
    public List<UIElement> Elements = new List<UIElement>();
    protected Action? OnAlign;

    /*
        ---Important!---
        Scale:
        The scale of the collection doesn't affect the visibility of its child elements, 
        therefore if an element is outside of the collection's bounds it will still be visible,
        it is only used for alignment purposes: 
        1. if it were to be used as a child element in another collection.
        2. to position the child correctly based on it's anchor type.
    */
    public UICollection(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        ResetInit();
    }

    public virtual UICollection AddElement(UIElement element)
    {
        Elements.Add(element);
        element.SetPositionType(PositionType.Relative);
        element.SetParent(this);
        return this;
    }

    public virtual UICollection AddElement(params UIElement[] element)
    {
        foreach (UIElement e in element)
            AddElement(e);
        return this;
    }

    public override void SetVisibility(bool visible)
    {
        if (Visible == visible)
            return;
            
        base.SetVisibility(visible);
        foreach (UIElement element in Elements)
            element.SetVisibility(visible);
    }

    public override void Align()
    {
        base.Align();
        foreach (UIElement element in Elements)
        {
            element.Align();
        }
    }

    public override void UpdateTransformation()
    {
        foreach (UIElement element in Elements)
            element.UpdateTransformation();
    }

    public static float GetTotalScale(List<UIElement> elements, int axis)
    {
        float totalScale = 0;
        foreach (UIElement element in elements)
            totalScale += element.Scale[axis];
        return totalScale;
    }


    public void ResetInit()
    {
        OnAlign = Init;
    }

    protected virtual void Init() {}
    public virtual void SetSpacing(float spacing) {}
    public virtual void SetBorder(Vector4 border) {}
}

public enum CollectionType
{
    Horizontal = 0,
    Vertical = 1
}