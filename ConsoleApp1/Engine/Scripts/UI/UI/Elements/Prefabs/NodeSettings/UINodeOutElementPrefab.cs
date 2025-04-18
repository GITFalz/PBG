using OpenTK.Mathematics;

public class UINodeOutElementPrefab : UIPrefab
{
    public UICollection Collection;
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton OutputButton;

    private Vector2 _scale = (100, 100);
    private Vector4 _offset = (0, 0, 0, 0);
    private UIController _controller;

    public UINodeOutElementPrefab(Vector2 scale, Vector4 offset, UIController controller)
    {
        _scale = scale;
        _offset = offset;
        _controller = controller;

        UIMesh uiMesh = _controller.UiMesh;

        Collection = new UICollection("NodeOutElementPrefabCollection", controller, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), _scale, _offset, 0);
        Background = new UIImage("NodeOutElementPrefabBackground", controller, AnchorType.TopRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), _scale, (0, 0, 0, 0), 0, 10, (10, 0.05f), uiMesh);
        
        ElementCollection = new UICollection("NodeOutElementPrefabElementCollection", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), _scale, (0, 0, 0, 0), 0);
        OutputButton = new UIButton("NodeOutElementPrefabOutputImage", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (30, 30), (-5, 0, 0, 0), 0, 11, (10, 0.05f), uiMesh, UIState.Interactable);

        ElementCollection.AddElements(OutputButton);

        Collection.AddElements(Background, ElementCollection);
    }

    public override UIElement[] GetMainElements()
    {
        return [Collection];
    }

    public override void SetVisibility(bool visible)
    {
        Collection.SetVisibility(visible);

        UIMesh uiMesh = _controller.UiMesh;
        TextMesh textMesh = _controller.textMesh;

        uiMesh.UpdateVisibility();
        textMesh.UpdateVisibility();
    }

    public override void Clear()
    {
        Collection.Clear();
    }
}