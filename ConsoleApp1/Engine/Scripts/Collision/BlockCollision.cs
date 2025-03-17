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

    public static (float, Vector3?) GetEntry(Collider playerCollider, Vector3 velocity, Vector3 blockPosition, int blockId, (float entryTime, Vector3?) entryData)
    {
        if (GetSubBoundingBoxes(blockId, out var colliders))
        {
            foreach (var collider in colliders)
            {
                if (!((playerCollider + velocity) & (collider + blockPosition)))
                    continue;

                (float newEntry, Vector3? newNormal) = playerCollider.Collide(collider + blockPosition, velocity);

                if (newNormal == null)
                    continue;

                if (newEntry < entryData.entryTime)
                    entryData = (newEntry - 0.001f, newNormal.Value);
            }
        }

        return entryData;
    }

    public static bool IsColliding(Collider entityCollider, Vector3 blockPosition, int blockId)
    {
        if (!GetSubBoundingBoxes(blockId, out var colliders))
            return false;

        foreach (var collider in colliders)
        {
            Collider blockCollider = collider + blockPosition;
            if (entityCollider & blockCollider)
                return true;
        }

        return false;
    }
}