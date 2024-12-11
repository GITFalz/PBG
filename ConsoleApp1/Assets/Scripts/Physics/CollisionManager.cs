using OpenTK.Mathematics;

public class CollisionManager
{
    public static List<Vector3i> GetChunkPositions(Hitbox entityHitbox, Vector3 position, Vector3 velocity)
    {
        List<Vector3i> chunkPositions = new List<Vector3i>();
        
        Vector3 newPosition = position + velocity;
        entityHitbox.SetPosition(newPosition);

        foreach (var corner in entityHitbox.GetCorners(newPosition))
        {
            int x = (int)corner.X & ~31;
            int y = (int)corner.Y & ~31;
            int z = (int)corner.Z & ~31;
            
            chunkPositions.Add(new Vector3i(x, y, z));
        }
        
        return chunkPositions;
    }
}