using OpenTK.Mathematics;

public class SubBlockBoundingBox
{
    public Vector3 Min = (0, 0, 0);
    public Vector3 Max = (1, 1, 1);
    public Vector3 Center = (.5f, .5f, .5f);

    public SubBlockBoundingBox() {}
    public SubBlockBoundingBox(Vector3 min, Vector3 max) { Min = min; Max = max; Center = (max + min) / 2f; }

    /// <summary>
    /// From point towards direction, what is the distance until being outside the boundingbox
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direction"></param>
    /// <returns>the distance vector</returns>
    public Vector3 DistanceToBorder(Vector3 point, Vector3 direction)
    {
        return (0,0,0);
    }

    private float DistanceToX(float x, float dir)
    {
        if (x <= Min.X || x >= Max.X) return -1;
        if (dir < 0) return x - Min.X;
        if (dir > 0) return Max.X - x;
        return 0;
    }
}