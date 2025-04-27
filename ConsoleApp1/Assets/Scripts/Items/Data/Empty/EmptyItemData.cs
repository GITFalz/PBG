using OpenTK.Mathematics;

public class EmptyItemData : ItemData
{
    public EmptyItemData()
    {
        Name = "empty";
        MaxStackSize = 0;
        Base();
    }

    public override void GenerateIcon() {}
    public override void RenderIcon(Vector2 position, float scale) {}
    public override void RenderIcon(Vector3 position, float scale) {}
}