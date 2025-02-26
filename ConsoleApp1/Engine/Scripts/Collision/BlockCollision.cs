using System.Numerics;

public static class BlockCollision
{
    public static Dictionary<int, SubBlockBoundingBox[]> SubBoundingBoxes = new()
    {
        
    };

    public static bool GetSubBoundingBoxes(int blockId, out SubBlockBoundingBox[] boxes)
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