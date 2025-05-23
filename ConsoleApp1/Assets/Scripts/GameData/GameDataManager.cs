public class GameDataManager : ScriptingNode
{
    public UIController GameDataController;
    public UIScrollView SelectionScrollView;
    public FileManager FileManager;

    public GameDataManager(FileManager fileManager)
    {
        GameDataController = new UIController();
       FileManager = fileManager;

        UICollection mainCollection = new UICollection("MainCollection", GameDataController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        UICollection selectionCollection = new UICollection("SelectionCollection", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0);

        UIImage selectionBackground = new UIImage("SelectionBackground", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        SelectionScrollView = new UIScrollView("SelectionScrollView", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, CollectionType.Vertical, (290, Game.Height - 10), (5, 5, 0, 0));

        selectionCollection.AddElements(selectionBackground, SelectionScrollView);

        mainCollection.AddElement(selectionCollection);

        GameDataController.AddElement(mainCollection);
    }

    void Start()
    {

    }

    void Awake()
    {

    }

    void Resize()
    {
        GameDataController.Resize();
    }

    void Update()
    {
        GameDataController.Update();
    }

    void Render()
    {
        GameDataController.RenderNoDepthTest();
    }

    void Exit()
    {

    }
}