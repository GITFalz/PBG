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
    public UICollection(
        string name, 
        UIController controller,
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float rotation) : 
        base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
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

    public virtual UICollection AddElements(params UIElement[] element)
    {
        foreach (UIElement e in element)
            AddElement(e);
        return this;
    }

    public virtual UICollection AddElements(params UIElement[][] element)
    {
        foreach (UIElement[] e in element)
            AddElements(e);
        return this;
    }

    public UICollection RemoveElements(params UIElement[] elements)
    {
        foreach (UIElement element in elements)
            RemoveElement(element);
        return this;
    }

    public override void RemoveElement(UIElement element)
    {
        if (Elements.Remove(element))
            element.SetParent(null);
    }

    public override void SetVisibility(bool visible)
    {
        Console.WriteLine($"SetVisibility: {Name} {visible}");
        
        foreach (UIElement element in Elements)
            element.SetVisibility(visible);

        if (Visible != visible)
            base.SetVisibility(visible);
    }

    public override void Align()
    {
        base.Align();
        foreach (UIElement element in Elements)
        {
            element.Align();
        }
    }

    public override void Clear()
    {
        base.Clear();
        foreach (UIElement element in Elements)
            element.Clear();
        Elements.Clear();
        OnAlign = null;
    }

    protected override void Internal_UpdateTransformation()
    {
        foreach (UIElement element in Elements)
            element.UpdateTransformation();
    }

    protected override void Internal_UpdateScale()
    {
        foreach (UIElement element in Elements)
            element.UpdateScale();
    }

    public void ResetInit()
    {
        OnAlign = Init;
    }

    public virtual void Init() {}
    public virtual void SetSpacing(float spacing) {}
    public virtual void SetBorder(Vector4 border) {}

    public override float GetYScale()
    {
        return GetElementScaleY();
    }

    public override float GetXScale()
    {
        return GetElementScaleX();
    }

    public virtual float GetElementScaleY() { return newScale.Y; }
    public virtual float GetElementScaleX() { return newScale.X; }
}

public enum CollectionType
{
    Horizontal = 0,
    Vertical = 1
}