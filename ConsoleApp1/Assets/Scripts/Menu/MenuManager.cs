using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class MenuManager : ScriptingNode
{
    public UIController MainMenuController;

    private Action _mainMenuAction = () => {};

    private Action _updateAction = () => {};
    private Action _renderAction = () => {};

    private bool _started = false;

    public MenuManager()
    {
        MainMenuController = new UIController();
    }

    public override void Start()
    {
        if (_started)
            return;

        _mainMenuAction = OpenMainMenu;

        UIMesh uiMesh = MainMenuController.UiMesh;

        UICollection mainMenuCollection = new UICollection("mainMenuCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UIImage mainMenuBackground = new UIImage("mainMenuBackground", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (600, 800), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh);

        UICollection mainMenuButtonCollection = new UICollection("mainMenuButtonCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UITextButton exitGameButton = new UITextButton("ExitGame", AnchorType.BottomCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (0, -20, 0, 0), 0, 0, (10, 0.05f), MainMenuController);
        UITextButton WorldSwitchButton = new UITextButton("World", AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (20, 20, 0, 0), 0, 0, (10, 0.05f), MainMenuController);
        UITextButton WorldNoiseEditorSwitchButton = new UITextButton("WorldNoiseEditor", AnchorType.TopRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (200, 50), (-20, 20, 0, 0), 0, 0, (10, 0.05f), MainMenuController);

        exitGameButton.SetMaxCharCount(9).SetText("Exit Game", 0.7f).GenerateChars();
        exitGameButton.SetOnClick(QuitGame);

        WorldSwitchButton.SetMaxCharCount(5).SetText("World", 0.7f).GenerateChars();
        WorldSwitchButton.SetOnClick(LoadWorld);

        WorldNoiseEditorSwitchButton.SetMaxCharCount(12).SetText("Noise Editor", 0.7f).GenerateChars();
        WorldNoiseEditorSwitchButton.SetOnClick(LoadWorldNoiseEditor);

        mainMenuButtonCollection.AddElements(exitGameButton.GetMainElements(), WorldSwitchButton.GetMainElements(), WorldNoiseEditorSwitchButton.GetMainElements());

        mainMenuCollection.AddElements(mainMenuBackground, mainMenuButtonCollection);

        MainMenuController.AddElements(mainMenuCollection);

        _started = true;
    }

    public override void Awake()
    {

    }

    public override void Resize()
    {
        MainMenuController.Resize();
    }

    public override void Update()
    {
        if (Input.IsKeyPressed(Keys.Escape))
        {
            _mainMenuAction.Invoke();
        }

        _updateAction.Invoke();
    }

    public override void Render()
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
    }

    public void CloseMainMenu()
    {
        Console.WriteLine("Closing Main Menu");
        _mainMenuAction = OpenMainMenu;
        Game.SetCursorState(CursorState.Grabbed);

        _updateAction = () => { };
        _renderAction = () => { };

        PlayerData.CanMove = true;
    }

    public void UpdateMainMenu()
    {
        MainMenuController.Update();
    }

    public void RenderMainMenu()
    {
        MainMenuController.Render();
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