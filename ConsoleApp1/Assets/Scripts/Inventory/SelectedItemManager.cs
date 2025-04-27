using OpenTK.Mathematics;

public class SelectedItemManager : ScriptingNode
{
    public static ItemSlot SelectedItem = new ItemSlot();
    public static Action UpdateSelectedItemText = () => { };

    public UIController SelectedItemController;
    private UIText _selectedItemText;

    public SelectedItemManager()
    {
        Name = "SelectedItemManager";

        SelectedItemController = new UIController();

        UICollection selectedItemCollection = new UICollection("SelectedItem", SelectedItemController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (50, 50), (0, 0, 0, 0), 0);

        _selectedItemText = new UIText("SelectedItemText", SelectedItemController, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (50, 50), (0, 0, 0, 0), 0);
        _selectedItemText.SetMaxCharCount(3).SetText("1", 1.2f).SetTextType(TextType.Numeric);

        selectedItemCollection.AddElement(_selectedItemText);

        SelectedItemController.AddElement(selectedItemCollection);

        UpdateSelectedItemText = UpdateSelectedItem;
    }

    public void UpdateSelectedItem()
    {
        _selectedItemText.SetText(SelectedItem.GetAmountToString(), 1.2f).UpdateCharacters();
    }

    void Update()
    {
        if (SelectedItem.item.IsEmpty() || SelectedItem.Amount <= 0)
            return;

        Vector2 position2d = Input.GetMousePosition() + (-20, 10);
        Vector3 position = new Vector3(position2d.X, position2d.Y, 1);

        SelectedItemController.SetPosition(position);
        SelectedItemController.Update();
    }

    void Render()
    {
        if (SelectedItem.item.IsEmpty() || SelectedItem.Amount <= 0)
            return;

        Vector2 position2d = Input.GetMousePosition() - new Vector2(25, 25);
        Vector3 position = new Vector3(position2d.X, position2d.Y, 0.5f);

        SelectedItem.item.RenderIcon(position, 0.5f);
        SelectedItemController.RenderDepthTest();
    }
}