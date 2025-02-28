using OpenTK.Mathematics;

public class SubBlockBoundingBox
{
    public Vector3 Min = (0, 0, 0);
    public Vector3 Max = (1, 1, 1);
    public Vector3 Center = (.5f, .5f, .5f);

    public SubBlockBoundingBox() {}
    public SubBlockBoundingBox(Vector3 min, Vector3 max) { Min = min; Max = max; Center = (max + min) / 2f; }

    public Vector3 DistanceToCollision(Hitbox playerHitbox, Vector3 direction)
    {
        var corners = playerHitbox.GetCorners(direction);
        return (0, 0, 0);
    }

    public bool InsideHitbox(Vector3[] corners, Vector3 direction, out Vector3 newDirection)
    {
        newDirection = Vector3.Zero;
        return true;

    }

    public bool InsideHitbox(Vector3 corner, Vector3 direction, out Vector3 newDirection)
    {
        newDirection = Vector3.Zero;
        return true;
    }

    /// <summary>
    /// assuming that the point IS inside of the block
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direction"></param>
    public void EdgeCheck(Vector3 point, Vector3 direction)
    {
        // Get the distance to the edge that is opposite to the direction
        float distanceX = direction.X < 0 ? Max.X - point.X : point.X - Min.X;
        float distanceY = direction.Y < 0 ? Max.Y - point.Y : point.Y - Min.Y;
        float distanceZ = direction.Z < 0 ? Max.Z - point.Z : point.Z - Min.Z;

        // Map the direction vector so X = 1 but stays proportional
        Vector3 mappedVector = direction / direction.X;

        float multiplierX = distanceX;
        float multiplierY = distanceY / mappedVector.Y;
        float multiplierZ = distanceZ / mappedVector.Z;

        
    }
}