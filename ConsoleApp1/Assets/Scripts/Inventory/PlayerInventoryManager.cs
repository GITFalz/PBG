using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerInventoryManager : ScriptingNode
{
    public bool IsOpen = false;

    private Action _updateAction;
    private Action _renderAction;

    public PlayerInventoryManager()
    {
        Name = "PlayerInventoryManager";
        
        _ = new PlayerData();

        _updateAction = ClosedUpdate;
        _renderAction = () => { };
    }

    void Update()
    {
        _updateAction.Invoke();
    }

    void Render()
    {
        _renderAction.Invoke();
    }

    private void OpenInventory()
    {
        Game.SetCursorState(CursorState.Normal);
        Game.Camera.Lock();

        IsOpen = true;

        PlayerData.TestInputs = false;

        MenuManager.CanOpenMenu = false;

        _updateAction = OpenedUpdate;
        _renderAction = OpenedRender;
    }

    private void CloseInventory()
    {
        Game.SetCursorState(CursorState.Grabbed);
        Game.Camera.Unlock();

        IsOpen = false;

        PlayerData.TestInputs = true;
        
        MenuManager.CanOpenMenu = true;
        MenuManager.LockThisFrame();

        _updateAction = ClosedUpdate;
        _renderAction = () => { };
    }

    private void OpenedUpdate()
    {
        if (Input.IsAnyKeyPressed(Keys.Escape, Keys.E))
        {
            CloseInventory();
        }

        PlayerData.PlayerInventory.Update();
        PlayerData.HotbarInventory.Update();
    }

    private void ClosedUpdate()
    {
        if (Input.IsKeyPressed(Keys.E))
        {
            OpenInventory();
        }
    }

    private void OpenedRender()
    {
        PlayerData.PlayerInventory.Render();
        PlayerData.HotbarInventory.Render();
    }
}