using OpenTK.Mathematics;

namespace ConsoleApp1.Engine.Scripts.Core;

public class Transform : Component
{
    public GameObject gameObject;
    public Vector3 position = Vector3.Zero;
    
    public Transform(GameObject gameObject)
    {
        this.gameObject = gameObject;
        name = "Transform";
    }
}