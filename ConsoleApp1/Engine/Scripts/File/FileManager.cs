public class FileManager
{
    public UIController UIController;
    public UIController FilesUIController;

    public string DefaultPath;

    public FileManager()
    {
        UIController = new UIController();
        FilesUIController = new UIController();
        DefaultPath = Game.mainPath;

        string[] directories = Directory.GetDirectories(DefaultPath);
        string[] files = Directory.GetFiles(DefaultPath);

        int elementCount = directories.Length + files.Length;
        int collumns = 6;
        int rows = (int)Math.Ceiling((float)elementCount / collumns);

        UIVerticalCollection verticalCollection = new UIVerticalCollection("FileVerticalCollection", FilesUIController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), (5, 5, 5, 5), 10, 0);

        UIHorizontalCollection[] horizontalCollections = new UIHorizontalCollection[rows];

        for (int i = 0; i < elementCount; i++)
        {
            int currentRow = i / collumns;
            int currentColumn = i % collumns;


        }
    }

    public void Resize()
    {
        UIController.Resize();
        FilesUIController.Resize();
    }

    public void Update()
    {
        UIController.Update();
        FilesUIController.Update();
    }

    public void Render()
    {
        UIController.RenderNoDepthTest();
        FilesUIController.RenderNoDepthTest();
    }
}