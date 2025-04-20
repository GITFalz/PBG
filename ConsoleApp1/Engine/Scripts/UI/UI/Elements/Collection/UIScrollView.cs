using OpenTK.Mathematics;

public class UIScrollView : UICollection
{
    public UIImage MaskPanel;
    public UICollection SubElements;

    public float ScrollSpeed = 5f;
    public CollectionType CollectionType;

    public UIScrollView(
        string name, 
        UIController controller,
        AnchorType anchorType, 
        PositionType positionType, 
        CollectionType collectionType, 
        Vector2 scale, 
        Vector4 offset) : 
        base(name, controller, anchorType, positionType, (0, 0, 0), scale, offset, 0)
    {
        CollectionType = collectionType;

        if (collectionType == CollectionType.Horizontal)
            SubElements = new UIHorizontalCollection("HorizontalStacking", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 100), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);
        else 
            SubElements = new UIVerticalCollection("VerticalStacking", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 100), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

        SubElements.SetScale(scale);

        //MaskPanel = new UIImage($"{name}MaskPanel", controller, anchorType, PositionType.Relative, (1, 1, 1), (0, 0, 0), scale, (0, 0, 0, 0), 0, -1, (0, 0), maskMesh);
        //MaskPanel.CanTest = true;
        //MaskPanel.SetOnHover(MoveScrollView);
        
        //Elements.Add(MaskPanel);
        Elements.Add(SubElements);

        //MaskPanel.ParentElement = this;
        SubElements.ParentElement = this;
    }

    private void MoveScrollView()
    {
        float scrollDelta = Input.GetMouseScrollDelta().Y;
        if (scrollDelta == 0 || SubElements.Scale[(int)CollectionType] < newScale[(int)CollectionType]) return;

        SubElements.Offset += scrollOffset[CollectionType](scrollDelta) * GameTime.DeltaTime * ScrollSpeed * 1000;
        SubElements.Offset = scrollClamp[CollectionType](SubElements.Scale - newScale, SubElements.Offset);
        
        SubElements.Align();
        SubElements.UpdateTransformation();
    }

    public override void SetVisibility(bool visible)
    {
        if (Visible != visible)
            base.SetVisibility(visible);

        MaskPanel.SetVisibility(visible);
        SubElements.SetVisibility(visible);
    }

    public UIScrollView SetScrollSpeed(float speed)
    {
        ScrollSpeed = speed;
        return this;
    }

    public new UIScrollView SetBorder(Vector4 border)
    {
        SubElements.SetBorder(border);
        return this;
    }

    public new UIScrollView SetScale(Vector2 scale)
    {
        base.SetScale(scale);
        MaskPanel.SetScale(scale);
        SubElements.SetScale(scale);
        return this;
    }

    public override void SetSpacing(float spacing)
    {
        SubElements.SetSpacing(spacing);
    }

    protected override void Internal_UpdateTransformation()
    {
        foreach (UIElement element in Elements)
            element.UpdateTransformation();
    }

    public override UICollection AddElement(UIElement element)
    {
        SubElements.AddElement(element);
        return this;
    }

    public override UICollection AddElements(params UIElement[] elements)
    {
        SubElements.AddElements(elements);
        return this;
    }

    public readonly Dictionary<CollectionType, Func<float, Vector4>> scrollOffset = new Dictionary<CollectionType, Func<float, Vector4>>()
    {
        { CollectionType.Horizontal, (scrollDelta) => (scrollDelta, 0, 0, 0) },
        { CollectionType.Vertical, (scrollDelta) => (0, scrollDelta, 0, 0) },
    };

    public readonly Dictionary<CollectionType, Func<Vector2, Vector4, Vector4>> scrollClamp = new Dictionary<CollectionType, Func<Vector2, Vector4, Vector4>>()
    {
        { CollectionType.Horizontal, (scale, offset) => { offset.X = Mathf.Clamp(-scale.X, 0, offset.X); return offset; } },
        { CollectionType.Vertical, (scale, offset) => { offset.Y = Mathf.Clamp(-scale.Y, 0, offset.Y); return offset; } },
    };
}