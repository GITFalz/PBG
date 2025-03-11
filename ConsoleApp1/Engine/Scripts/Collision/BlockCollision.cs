using OpenTK.Mathematics;

public static class BlockCollision
{
    public static Dictionary<int, Collider[]> SubBoundingBoxes = new()
    {
        { 1, [new Collider()] },
    };

    public static bool GetSubBoundingBoxes(int blockId, out Collider[] boxes)
    {
        boxes = [];
        if (SubBoundingBoxes.TryGetValue(blockId, out var values))
        {
            boxes = values;
            return true;
        }
        return false;
    }
}