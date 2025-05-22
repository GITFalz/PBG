using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class MenuManager : ScriptingNode
{
    public static MenuManager Instance;
    public static bool IsOpen { get; private set; } = false;

    public UIController MainMenuController;

    private Action _updateAction = () => {};
    private Action _renderAction = () => {};

    private bool _started = false;

    public static bool CanOpenMenu = true;

    public MenuManager()
    {
        Instance = this;
        MainMenuController = new UIController();
    }

    void Start()
    {
        if (_started)
            return;

        UICollection mainMenuCollection = new UICollection("mainMenuCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UIImage mainMenuBackground = new UIImage("mainMenuBackground", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (600, 800), (0, 0, 0, 0), 0, 0, (10, 0.05f));

        UICollection mainMenuButtonCollection = new UICollection("mainMenuButtonCollection", MainMenuController, AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (600, 800), (0, 0, 0, 0), 0);

        UITextButton exitGameButton = new UITextButton("ExitGame", MainMenuController, AnchorType.BottomCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (180, 30), (0, -15, 0, 0), 0, 10, (10, 0.05f));

        UITextButton gameDataButton = new UITextButton("GameData", MainMenuController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (180, 30), (15, 15, 0, 0), 0, 10, (10, 0.05f));
        UITextButton WorldSwitchButton = new UITextButton("World", MainMenuController, AnchorType.TopCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (180, 30), (0, 15, 0, 0), 0, 10, (10, 0.05f));
        UITextButton WorldNoiseEditorSwitchButton = new UITextButton("WorldNoiseEditor", MainMenuController, AnchorType.TopRight, PositionType.Relative, (0.6f, 0.6f, 0.6f), null, (180, 30), (-15, 15, 0, 0), 0, 10, (10, 0.05f));

        exitGameButton.SetMaxCharCount(9).SetText("Exit Game", 1.2f);
        exitGameButton.SetOnClick(QuitGame);

        gameDataButton.SetMaxCharCount(9).SetText("Game Data", 1.2f);
        gameDataButton.SetOnClick(LoadGameData);

        WorldSwitchButton.SetMaxCharCount(5).SetText("World", 1.2f);
        WorldSwitchButton.SetOnClick(LoadWorld);

        WorldNoiseEditorSwitchButton.SetMaxCharCount(12).SetText("Noise Editor", 1.2f);
        WorldNoiseEditorSwitchButton.SetOnClick(LoadWorldNoiseEditor);

        mainMenuButtonCollection.AddElements(exitGameButton.GetMainElements(), gameDataButton.GetMainElements(), WorldSwitchButton.GetMainElements(), WorldNoiseEditorSwitchButton.GetMainElements());

        mainMenuCollection.AddElements(mainMenuBackground, mainMenuButtonCollection);

        MainMenuController.AddElements(mainMenuCollection);

        _started = true;

        _updateAction = ClosedUpdate;  
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
        Game.SetCursorState(CursorState.Normal);

        _updateAction = OpenedUpdate;
        _renderAction = OpenedRender;

        PlayerData.TestInputs = false;
        PlayerData.UpdatePhysics = false;
        IsOpen = true;
    }

    public void CloseMainMenu()
    {
        Console.WriteLine("Closing Main Menu");

        if (Game.CurrentScene?.Name == "World")
            Game.SetCursorState(CursorState.Grabbed);

        _updateAction = ClosedUpdate;
        _renderAction = () => { };

        PlayerData.TestInputs = true;
        PlayerData.UpdatePhysics = true;
        IsOpen = false;
    }

    public void OpenedUpdate()
    {
        if (Input.IsKeyPressed(Keys.Escape) && CanOpenMenu)
        {
            CloseMainMenu();
        }

        MainMenuController.Update();
    }

    public void WaitForNextFrameUpdate()
    {
        Console.WriteLine("Waiting for next frame");
        if (IsOpen)
        {
            _updateAction = ClosedUpdate;
            _renderAction = () => { };
        }
        else
        {
            _updateAction = OpenedUpdate;
            _renderAction = OpenedRender;
        }
    }

    public void ClosedUpdate()
    {
        if (Input.IsKeyPressed(Keys.Escape) && CanOpenMenu)
        {
            OpenMainMenu();
        }
    }

    public void OpenedRender()
    {
        MainMenuController.RenderNoDepthTest();
    }
    #endregion

    #region UI functions
    public void QuitGame()
    {
        Game.CloseGame();
    }

    public void LoadGameData()
    {
        Game.LoadScene("GameData");
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


    public static void LockThisFrame()
    {   
        Instance._renderAction = () => { };
        Instance._updateAction = () => { 
            if (IsOpen)
            {
                Instance._updateAction = Instance.OpenedUpdate;
                Instance._renderAction = Instance.OpenedRender;
            }
            else
            {
                Instance._updateAction = Instance.ClosedUpdate;
                Instance._renderAction = () => { };
            }
        };
    }
}