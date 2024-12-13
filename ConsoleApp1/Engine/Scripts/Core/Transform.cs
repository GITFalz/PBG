using OpenTK.Mathematics;

public class Transform : Component
{
    public GameObject gameObject;
    
    public Vector3 Position = Vector3.Zero;
    
    public Transform(GameObject gameObject)
    {
        this.gameObject = gameObject;
        name = "Transform";
    }
}