using OpenTK.Mathematics;

public class PlayerData
{
    public static Vector3 Position = new Vector3(0, 60, 0);
    public static Vector3 EyePosition = new Vector3(0, 61.8f, 0);

    public static bool TestInputs = true;
    public static bool UpdatePhysics = true;

    public static Inventory PlayerInventory;
    public static Inventory HotbarInventory;

    public PlayerData()
    {
        PlayerInventory = new Inventory(9, 4);
        HotbarInventory = new Inventory(9, 1);

        PlayerInventory.AnchorType = AnchorType.MiddleCenter;
        HotbarInventory.AnchorType = AnchorType.BottomCenter;
    }
}