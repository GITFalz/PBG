using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class MenuManager : ScriptingNode
{
    public static bool IsOpen { get; private set; } = false;

    public UIController MainMenuController;

    private Action _mainMenuAction = () => {};

    private Action _updateAction = () => {};
    private Action _renderAction = () => {};

    private bool _started = false;

    public MenuManager()
    {
        MainMenuController = new UIController();
    }

    void Start()
    {
        if (_started)
            return;

        _mainMenuAction = OpenMainMenu;

        UICollection mainMenuCollection = new UICollection("mainMenuCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UIImage mainMenuBackground = new UIImage("mainMenuBackground", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (600, 800), (0, 0, 0, 0), 0, 0, (10, 0.05f));

        UICollection mainMenuButtonCollection = new UICollection("mainMenuButtonCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UITextButton exitGameButton = new UITextButton("ExitGame", MainMenuController, AnchorType.BottomCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (0, -20, 0, 0), 0, 0, (10, 0.05f));
        UITextButton WorldSwitchButton = new UITextButton("World", MainMenuController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (20, 20, 0, 0), 0, 0, (10, 0.05f));
        UITextButton WorldNoiseEditorSwitchButton = new UITextButton("WorldNoiseEditor", MainMenuController, AnchorType.TopRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (-20, 20, 0, 0), 0, 0, (10, 0.05f));

        exitGameButton.SetMaxCharCount(9).SetText("Exit Game", 1.5f);
        exitGameButton.SetOnClick(QuitGame);

        WorldSwitchButton.SetMaxCharCount(5).SetText("World", 1.5f);
        WorldSwitchButton.SetOnClick(LoadWorld);

        WorldNoiseEditorSwitchButton.SetMaxCharCount(12).SetText("Noise Editor", 1.5f);
        WorldNoiseEditorSwitchButton.SetOnClick(LoadWorldNoiseEditor);

        mainMenuButtonCollection.AddElements(exitGameButton.GetMainElements(), WorldSwitchButton.GetMainElements(), WorldNoiseEditorSwitchButton.GetMainElements());

        mainMenuCollection.AddElements(mainMenuBackground, mainMenuButtonCollection);

        MainMenuController.AddElements(mainMenuCollection);

        _started = true;
    }

    void Awake()
    {

    }

    void Resize()
    {
        MainMenuController.Resize();
    }

    void Update()
    {
        if (Input.IsKeyPressed(Keys.Escape))
        {
            _mainMenuAction.Invoke();
        }

        _updateAction.Invoke();
    }

    void Render()
    {
        _renderAction.Invoke();
    }

    #region Main functions
    public void OpenMainMenu()
    {
        Console.WriteLine("Opening Main Menu");
        _mainMenuAction = CloseMainMenu;
        Game.SetCursorState(CursorState.Normal);

        _updateAction = UpdateMainMenu;
        _renderAction = RenderMainMenu;

        PlayerData.CanMove = false;
        IsOpen = true;
    }

    public void CloseMainMenu()
    {
        Console.WriteLine("Closing Main Menu");
        _mainMenuAction = OpenMainMenu;

        if (Game.CurrentScene?.Name == "World")
            Game.SetCursorState(CursorState.Grabbed);

        _updateAction = () => { };
        _renderAction = () => { };

        PlayerData.CanMove = true;
        IsOpen = false;
    }

    public void UpdateMainMenu()
    {
        MainMenuController.Update();
    }

    public void RenderMainMenu()
    {
        MainMenuController.RenderNoDepthTest();
    }
    #endregion

    #region UI functions
    public void QuitGame()
    {
        Game.CloseGame();
    }

    public void LoadWorld()
    {
        Game.LoadScene("World");
    }

    public void LoadWorldNoiseEditor()
    {
        Game.LoadScene("WorldNoiseEditor");
    }
    #endregion
}