using OpenTK.Mathematics;

public class Hitbox
{
    private static Vector3 _origin = Vector3.Zero;
    
    public Vector3 Min;
    public Vector3 Max;

    public Vector3 Position;

    public float Width;
    public float Height;
    public float Depth;

    public Hitbox(Vector3 size)
    {
        Width = size.X;
        Height = size.Y;
        Depth = size.Z;
        
        Min = new Vector3(-Width/2, 0, -Depth/2);
        Max = new Vector3(Width/2, Height, Depth/2);
        
        Position = _origin;
    }
    
    public void SetPosition(Vector3 position)
    {
        Position = position;
    }
    
    public void GetMinMax(out Vector3 min, out Vector3 max)
    {
        min = Min + Position;
        max = Max + Position;
    }
    
    public List<Vector3> GetCorners()
    {
        return GetCorners(Position);
    }
    
    public List<Vector3> GetCorners(Vector3 position)
    {
        List<Vector3> corners = new List<Vector3>();
        
        corners.Add(position + new Vector3(Min.X, Min.Y, Min.Z));
        corners.Add(position + new Vector3(Max.X, Min.Y, Min.Z));
        corners.Add(position + new Vector3(Min.X, Min.Y, Max.Z));
        corners.Add(position + new Vector3(Max.X, Min.Y, Max.Z));
        corners.Add(position + new Vector3(Min.X, Max.Y, Min.Z));
        corners.Add(position + new Vector3(Max.X, Max.Y, Min.Z));
        corners.Add(position + new Vector3(Min.X, Max.Y, Max.Z));
        corners.Add(position + new Vector3(Max.X, Max.Y, Max.Z));

        return corners;
    }
}