using OpenTK.Mathematics;

public class PhysicsBody : Component
{
    public const float GRAVITY = 2.8f;
    public const float MAX_FALL_SPEED = 190f;
    public const float DRAG = 0.9f;
    
    public bool doGravity = true;
    
    public Vector3 Velocity = Vector3.Zero;
    
    public PhysicsBody()
    {
        name = "PhysicsBody";
    }
    
    public override void FixedUpdate()
    {
        if (doGravity)
            Gravity();
        Drag();
        transform.Position += Velocity;
    }
    
    public void Gravity()
    {
        Vector3 newVelocity = Velocity + new Vector3(0, -GRAVITY * GameTime.FixedDeltaTime, 0);
        newVelocity.Y = Mathf.Max(newVelocity.Y, -MAX_FALL_SPEED * GameTime.FixedDeltaTime);
        Velocity = newVelocity;
    }

    public void Drag()
    {
        Velocity *= DRAG;
    }
    
    public void AddForce(Vector3 direction, float force)
    {
        AddForce(direction * force);
    }
    
    public void AddForce(Vector3 direction)
    {
        Velocity += direction * GameTime.FixedDeltaTime;
    }
}