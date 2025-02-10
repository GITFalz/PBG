using OpenTK.Mathematics;

public class UICollection : UIElement
{
    public List<UIElement> Elements = new List<UIElement>();
    protected Action? OnAlign;

    public UICollection(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        ResetInit();
    }

    public void AddElement(UIElement element)
    {
        Elements.Add(element);
        element.SetPositionType(PositionType.Relative);
        element.SetParent(this);
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
}