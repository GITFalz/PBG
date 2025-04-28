using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerInventoryManager : ScriptingNode
{
    public bool IsOpen = false;

    private Action _updateAction;
    private Action _renderAction;

    public int SelectedItemIndex = 0;
    public static ItemSlot SelectedItem = new ItemSlot();

    private UIController SelectorController;

    public PlayerInventoryManager()
    {
        Name = "PlayerInventoryManager";
        
        _ = new PlayerData();

        _updateAction = ClosedUpdate;
        _renderAction = ClosedRender;

        SelectorController = new();

        UICollection selectorCollection = new UICollection("Selector", SelectorController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (20, 20), (0, 0, 0, 0), 0);

        UIImage selectorImage = new UIImage("SelectorImage", SelectorController, AnchorType.TopLeft, PositionType.Relative, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 10, (10, 0.05f));

        selectorCollection.AddElement(selectorImage);

        SelectorController.AddElement(selectorCollection);

        Vector2 hotbarPosition = PlayerData.HotbarInventory.Position;
        SelectorController.SetPosition(new Vector3(hotbarPosition.X + SelectedItemIndex * 50, hotbarPosition.Y, 0.6f));

        if (PlayerData.HotbarInventory.GetItem(SelectedItemIndex, out var item))
        {
            SelectedItem = item;
        }
    }

    void Update()
    {
        _updateAction.Invoke();
        SelectorController.Update();
    }

    void Render()
    {
        _renderAction.Invoke();
        SelectorController.RenderDepthTest();
    }

    public static void AddBlock(CWorldBlock block)
    {
        if (!ItemDataManager.GetItem(block.blockName, out ItemData? item))
            return;

        PlayerData.HotbarInventory.AddTo(item, 1);
    }

    private void OpenInventory()
    {
        Game.SetCursorState(CursorState.Normal);
        Game.Camera.Lock();

        IsOpen = true;

        PlayerData.TestInputs = false;

        PlayerData.PlayerInventory.Test = true;
        PlayerData.HotbarInventory.Test = true;

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

        PlayerData.PlayerInventory.Test = false;
        PlayerData.HotbarInventory.Test = false;
        
        MenuManager.CanOpenMenu = true;
        MenuManager.LockThisFrame();

        _updateAction = ClosedUpdate;
        _renderAction = ClosedRender;
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

        if (!Input.IsKeyDown(Keys.LeftControl))
        {
            float scroll = Input.GetMouseScrollDelta().Y;
            if (scroll < 0)
            {
                SelectedItemIndex++;
                if (SelectedItemIndex >= PlayerData.HotbarInventory.Width)
                    SelectedItemIndex = 0;

                Vector2 hotbarPosition = PlayerData.HotbarInventory.Position;
                SelectorController.SetPosition(new Vector3(hotbarPosition.X + SelectedItemIndex * 50, hotbarPosition.Y, 0.6f));
                SelectedItem = PlayerData.HotbarInventory.GetItem(SelectedItemIndex, out var item) ? item : new ItemSlot();
            }
            else if (scroll > 0)
            {
                SelectedItemIndex--;
                if (SelectedItemIndex < 0)
                    SelectedItemIndex = PlayerData.HotbarInventory.Width - 1;

                Vector2 hotbarPosition = PlayerData.HotbarInventory.Position;
                SelectorController.SetPosition(new Vector3(hotbarPosition.X + SelectedItemIndex * 50, hotbarPosition.Y, 0.6f));
                SelectedItem = PlayerData.HotbarInventory.GetItem(SelectedItemIndex, out var item) ? item : new ItemSlot();
            }
        }

        if (Input.IsMousePressed(MouseButton.Left))
        {
            SelectedItem.item.LeftClick(SelectedItem);
        }
        else if (Input.IsMousePressed(MouseButton.Right))
        {
            SelectedItem.item.RightClick(SelectedItem);
        }

        PlayerData.HotbarInventory.Update();
    }

    private void OpenedRender()
    {
        PlayerData.PlayerInventory.Render();
        PlayerData.HotbarInventory.Render();
    }

    private void ClosedRender()
    {
        PlayerData.HotbarInventory.Render();
    }
}