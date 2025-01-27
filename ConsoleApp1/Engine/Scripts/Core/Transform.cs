using OpenTK.Mathematics;

public class Transform(GameObject gameObject)
{
    public GameObject gameObject = gameObject;
    
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    
    public Vector3 Forward => Vector3.Transform((0, 0, 1), Rotation);
    public Vector3 Right => Vector3.Transform((1, 0, 0), Rotation);
    public Vector3 Up => Vector3.Transform((0, 1, 0), Rotation);
}