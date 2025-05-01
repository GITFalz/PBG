using OpenTK.Mathematics;

public class UIListPrefab : UIPrefab
{
    public UIImage Background;

    public UIScrollView ScrollView;

    public Vector2 Scale;

    public UIListPrefab(string name, UIController controller, AnchorType anchorType, Vector2 scale, Vector4 offset) : base(name, controller, offset)
    {
        Scale = scale;

        Collection = new($"{name}Collection", controller, anchorType, PositionType.Relative, (0, 0, 0), scale, offset, 0f);
        Collection.SetScale(scale);
        Background = new("ListPrefabBackground", controller, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (0, 0), (0, 0, 0, 0), 0f, 11, (10f, 0.05f));

        ScrollView = new("ScrollView", controller, AnchorType.MiddleCenter, PositionType.Relative, CollectionType.Vertical, scale - (14, 14), (0, 0, 0, 0));
        ScrollView.SetSpacing(0);

        Collection.AddElements(Background, ScrollView);
    }

    public void ClearButtons()
    {
        ScrollView.DeleteSubElements();
    }

    public UIElement AddButton(string text, out UIButton button, out UIButton deleteButton)
    {
        UICollection collection = new($"{text}Collection", Controller, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (Scale.X - 14, 30), (0, 0, 0, 0), 0f);
        
        UITextButton textButton = new($"{text}Button", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (Scale.X - 40, 30), (0, 0, 0, 0), 0f, 10, (10f, 0.05f), UIState.Interactable);
        textButton.Text.SetAnchorType(AnchorType.MiddleLeft);
        textButton.Text.Offset.X = 7;
        textButton.SetMaxCharCount(22).SetText(text, 0.9f);

        button = textButton.Button;
        deleteButton = new($"{text}DeleteButton", Controller, AnchorType.MiddleRight, PositionType.Relative, (0.6f, 0.2f, 0.2f, 1f), (0, 0, 0), (20, 20), (-5, 0, 0, 0), 0f, 10, (10f, 0.05f), UIState.Interactable);

        collection.AddElements(textButton.GetMainElements(), [deleteButton]);

        ScrollView.AddElements(collection);

        return collection;
    }

    public bool DeleteButton(UIElement button)
    {
        return ScrollView.RemoveElement(button);
    }
}