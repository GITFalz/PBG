using OpenTK.Mathematics;

public abstract class UIPrefab
{
    public string Name = "";
    public UIController Controller;
    public UICollection Collection;
    public Vector4 Offset;

    public UIPrefab(string name, UIController controller, Vector4 offset)
    {
        Name = name;
        Controller = controller;
        Offset = offset;
        Collection = new UICollection(name, controller, AnchorType.TopLeft, PositionType.Relative, Vector3.Zero, Vector2.Zero, offset, 0f);
    }

    public virtual UIElement[] GetMainElements()
    {
        return [Collection];
    }

    public virtual void SetVisibility(bool visible)
    {
        Collection.SetVisibility(visible);
    }

    public virtual void Clear()
    {
        Collection.Clear();
    }

    public virtual void Destroy()
    {
        Controller.RemoveElements(this);
    }
}