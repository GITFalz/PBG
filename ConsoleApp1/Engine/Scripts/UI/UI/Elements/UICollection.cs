using OpenTK.Mathematics;

public class UICollection : UIElement
{
    public List<UIElement> Elements = new List<UIElement>();

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
        ResetInit();
        Elements.Add(element);
        element.SetPositionType(PositionType.Relative);
        element.ParentElement = this;
        return this;
    }

    public virtual UICollection AddElements(params UIPrefab[] element)
    {
        foreach (UIPrefab e in element)
            AddElements(e.GetMainElements());
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

    public virtual void RemoveElements(params UIPrefab[] elements)
    {
        foreach (UIPrefab element in elements)
        {
            RemoveElements(element.GetMainElements());
        }
    }

    public virtual void RemoveElements(params UIElement[] elements)
    {
        foreach (UIElement element in elements)
        {
            RemoveElement(element);
        }
    }

    public override bool RemoveElement(UIElement element)
    {
        if (Elements.Remove(element))
        {
            element.ParentElement = null;
            return true;
        }
        return false;
    }

    public override void SetVisibility(bool visible)
    {
        foreach (UIElement element in Elements)
            element.SetVisibility(visible);

        if (Visible != visible)
            base.SetVisibility(visible);
    }

    public virtual void CalculateScale() { }

    public override void Align()
    {
        OnAlign?.Invoke(); // This is when a function has to be called before the alignment of the element
        base.Align();
        foreach (UIElement element in Elements)
        {
            element.Align();
        }
    }

    public override void Move(Vector3 offset)
    {
        base.Move(offset);
        foreach (UIElement element in Elements)
            element.Move(offset);
    }

    public override void Clear()
    {
        base.Clear();
        foreach (UIElement element in Elements)
            element.Clear();
        Elements.Clear();
        OnAlign = null;
    }

    public override void RemoveChild(UIElement element)
    {
        Elements.Remove(element);
        element.ParentElement = null;
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

    public override void ResetInit()
    {
        OnAlign = CalculateScale;
        foreach (UIElement element in Elements)
        {
            element.ResetInit();
        }
    }

    public virtual void SetSpacing(float spacing) { }
    public virtual void SetBorder(Vector4 border) { }

    public override float GetYScale()
    {
        return GetElementScaleY();
    }

    public override float GetXScale()
    {
        return GetElementScaleX();
    }

    public override void SetMasked(bool masked)
    {
        base.SetMasked(masked);
        foreach (UIElement element in Elements)
            element.SetMasked(masked);
    }

    public override void SetMaskIndex(int maskIndex)
    {
        base.SetMaskIndex(maskIndex);
        foreach (UIElement element in Elements)
            element.SetMaskIndex(maskIndex);
    }

    public virtual float GetElementScaleY() { return newScale.Y; }
    public virtual float GetElementScaleX() { return newScale.X; }

    public override void Delete(bool baseOnly = false)
    {
        base.Delete();
        if (baseOnly) return;
        List<UIElement> elementsToDelete = [.. Elements];
        foreach (UIElement element in elementsToDelete)
            element.Delete();
        Elements.Clear();
    }

    public void DeleteElements()
    {
        List<UIElement> elementsToDelete = [.. Elements];
        foreach (var element in elementsToDelete)
            element.Delete();
        Elements.Clear();
    }
}

public enum CollectionType
{
    Horizontal = 0,
    Vertical = 1
}