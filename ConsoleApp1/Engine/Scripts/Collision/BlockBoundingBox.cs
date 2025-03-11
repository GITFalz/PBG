using OpenTK.Mathematics;

public class BlockBoundingBox
{
    public Vector3 Min = (0, 0, 0);
    public Vector3 Max = (1, 1, 1);
    public Vector3 Center = (.5f, .5f, .5f);

    public BlockBoundingBox() {}
    public BlockBoundingBox(Vector3 min, Vector3 max) { Min = min; Max = max; Center = (max + min) / 2f; }

    public float Distance(Vector3 minA, Vector3 maxA, Vector3 velocity, Vector3 origin, out Vector3 normal)
    {
        normal = Vector3.Zero;

        Vector3 minB = Min + origin;
        Vector3 maxB = Max + origin;

        Console.WriteLine($"Testing Collision: MinA: {minA} - MaxA: {maxA} - MinB: {minB} - MaxB: {maxB} at {origin} going {velocity}");

        float xInvEntry, yInvEntry, zInvEntry;
        float xInvExit, yInvExit, zInvExit;

        if (velocity.X > 0.0f)
        {
            xInvEntry = minB.X - maxA.X;
            xInvExit = maxB.X - minA.X;
        }
        else
        {
            xInvEntry = maxB.X - minA.X;
            xInvExit = minB.X - maxA.X;
        }

        if (velocity.Y > 0.0f)
        {
            yInvEntry = minB.Y - maxA.Y;
            yInvExit = maxB.Y - minA.Y;
        }
        else
        {
            yInvEntry = maxB.Y - minA.Y;
            yInvExit = minB.Y - maxA.Y;
        }

        if (velocity.Z > 0.0f)
        {
            zInvEntry = minB.Z - maxA.Z;
            zInvExit = maxB.Z - minA.Z;
        }
        else
        {
            zInvEntry = maxB.Z - minA.Z;
            zInvExit = minB.Z - maxA.Z;
        }


        float xEntry, yEntry, zEntry;
        float xExit, yExit, zExit;

        if (velocity.X == 0.0f)
        {
            xEntry = float.NegativeInfinity;
            xExit = float.PositiveInfinity;
        }
        else
        {
            xEntry = xInvEntry / velocity.X;
            xExit = xInvExit / velocity.X;
        }

        if (velocity.Y == 0.0f)
        {
            yEntry = float.NegativeInfinity;
            yExit = float.PositiveInfinity;
        }
        else
        {
            yEntry = yInvEntry / velocity.Y;
            yExit = yInvExit / velocity.Y;
        }

        if (velocity.Z == 0.0f)
        {
            zEntry = float.NegativeInfinity;
            zExit = float.PositiveInfinity;
        }
        else
        {
            zEntry = zInvEntry / velocity.Z;
            zExit = zInvExit / velocity.Z;
        }

        Console.WriteLine($"X: InvEntry - InvExit  {xInvEntry} - {xInvExit}  Y: {yInvEntry} - {yInvExit}  Z: {zInvEntry} - {zInvExit}");
        Console.WriteLine($"X: Entry - Exit  {xEntry} - {xExit}  Y: {yEntry} - {yExit}  Z: {zEntry} - {zExit}");

        float entryTime = Mathf.Max(xEntry, yEntry, zEntry);
        float exitTime = Mathf.Min(xExit, yExit, zExit);

        Console.WriteLine($"EntryTime: {entryTime} - ExitTime: {exitTime}");

        if (entryTime > exitTime ||
            (velocity.X != 0 && (xEntry < -0.001f || xEntry > 1.001f)) ||
            (velocity.Y != 0 && (yEntry < -0.001f || yEntry > 1.001f)) ||
            (velocity.Z != 0 && (zEntry < -0.001f || zEntry > 1.001f)))
        {
            return 1.0f;
        }
        else
        {
            if (xEntry > yEntry && xEntry > zEntry)
            {
                normal = new Vector3(-Mathf.SignNo0(xInvEntry), 0, 0);
            }
            else if (yEntry > xEntry && yEntry > zEntry)
            {
                normal = new Vector3(0, -Mathf.SignNo0(yInvEntry), 0);
            }
            else
            {
                normal = new Vector3(0, 0, -Mathf.SignNo0(zInvEntry));
            }

            return entryTime;
        }
    }
}