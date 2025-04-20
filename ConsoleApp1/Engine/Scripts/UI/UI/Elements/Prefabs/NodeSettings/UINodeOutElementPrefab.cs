using OpenTK.Mathematics;

public class UINodeOutElementPrefab : UIPrefab
{
    public UIImage Background;

    public UICollection ElementCollection;
    public UIButton OutputButton;

    private Vector2 _scale = (100, 100);

    public UINodeOutElementPrefab(Vector2 scale, Vector4 offset, UIController controller) : base("NodeOutElementPrefab", controller, offset)
    {
        _scale = scale;

        Collection = new UICollection("NodeOutElementPrefabCollection", controller, AnchorType.TopRight, PositionType.Relative, (0, 0, 0), _scale, Offset, 0);
        Background = new UIImage("NodeOutElementPrefabBackground", controller, AnchorType.TopRight, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), _scale, (0, 0, 0, 0), 0, 10, (10, 0.05f));
        
        ElementCollection = new UICollection("NodeOutElementPrefabElementCollection", controller, AnchorType.TopRight, PositionType.Relative, (0.5f, 0.5f, 0.5f), _scale, (0, 0, 0, 0), 0);
        OutputButton = new UIButton("NodeOutElementPrefabOutputImage", controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (30, 30), (-5, 0, 0, 0), 0, 11, (10, 0.05f), UIState.Interactable);

        ElementCollection.AddElements(OutputButton);

        Collection.AddElements(Background, ElementCollection);

        Controller.AddElements(this);
    }
}