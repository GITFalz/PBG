using OpenTK.Mathematics;

public class UIScrollView : UICollection
{
    public UIMask MaskPanel;
    public UICollection SubElements;

    public float ScrollSpeed = 5f;
    public CollectionType CollectionType;
    public float ScrollPosition
    {
        get => _scrollPosition;
        set
        {
            _scrollPosition = value;
            SubElements.Offset[(int)CollectionType] = value;
        }
    }
    private float _scrollPosition = 0f;

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

        MaskPanel = new UIMask($"{name}MaskPanel", controller, anchorType, PositionType.Relative, (1, 1, 1, 0.5f), (0, 0, 0), scale, (0, 0, 0, 0), 0, -1, (0, 0));
        MaskPanel.CanTest = true;
        MaskPanel.SetOnHover(MoveScrollView);

        Elements.Add(MaskPanel);
        Elements.Add(SubElements);

        MaskPanel.ParentElement = this;
        SubElements.ParentElement = this;
    }

    private void MoveScrollView()
    {
        float scrollDelta = Input.GetMouseScrollDelta().Y;
        if (scrollDelta == 0 || SubElements.Scale[(int)CollectionType] < newScale[(int)CollectionType]) return;

        SubElements.Offset += scrollOffset[CollectionType](scrollDelta) * GameTime.DeltaTime * ScrollSpeed * 1000;
        SubElements.Offset = scrollClamp[CollectionType](SubElements.Scale - newScale, SubElements.Offset);
        ScrollPosition = SubElements.Offset[(int)CollectionType];
        
        SubElements.Align();
        SubElements.UpdateTransformation();
    }

    public void GenerateMask()
    {
        SetMasked(true);
        SetMaskIndex(MaskPanel.MaskData.GetMaskIndex(MaskPanel));
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
        ResetInit();
        SubElements.AddElement(element);
        element.SetMasked(true);
        return this;
    }

    public override void ResetInit()
    {
        OnAlign = CalculateScale;
        SubElements?.ResetInit();
    }

    public override bool RemoveElement(UIElement element) 
    {
        if (SubElements.RemoveElement(element))
        {
            element.ParentElement = null;
            return true;
        }
        return false;
    }

    public override void RemoveChild(UIElement element)
    {
        RemoveElement(element);
    }

    public void DeleteSubElements()
    {
        SubElements.Delete();
        SubElements.Clear();
    }

    public override void Delete()
    {
        base.Delete();
        DeleteSubElements();
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