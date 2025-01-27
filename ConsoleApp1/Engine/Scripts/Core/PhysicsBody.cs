using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PhysicsBody : Component
{
    public float gravity = 50f;
    public float maxFallSpeed = 150f;
    
    public bool doGravity = false;
    
    public Vector3 Acceleration = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;
    public float Drag = 0.1f;
    public float Mass = 1f;

    public Hitbox Hitbox;
    public bool IsGrounded;
    
    public bool huggingPosZ = false;
    public bool huggingNegZ = false;
    public bool huggingPosX = false;
    public bool huggingNegX = false;
    
    public bool huggingWall = false;
    
    public Vector3 physicsPosition;
    private Vector3 previousPosition;
    private float interpolationSpeed = 10f;

    public PhysicsBody()
    {
        name = "PhysicsBody";
        Hitbox = new Hitbox(new Vector3(1, 2, 1));
    }
    
    private Vector3 targetVelocity;
    private Vector3 currentForce;
    private float velocityLerpSpeed = 8f;
    private float forceInterpolationSpeed = 5f;
    
    public void AddForce(Vector3 force, float strength)
    {
        AddForce(force * strength);
    }
    
    public void AddForce(Vector3 force)
    {
        Acceleration += force / Mass;
    }
    
    public void SetPosition(Vector3 newPosition)
    {
        physicsPosition = newPosition;
        previousPosition = newPosition;
        transform.Position = newPosition;
    }
    
    public void AddImpulse(Vector3 impulse)
    {
        Velocity += impulse / Mass;
        targetVelocity = Velocity;
    }
    
    public void AddAcceleration(Vector3 acceleration)
    {
        Acceleration += acceleration;
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        Velocity = newVelocity;
        targetVelocity = newVelocity;
        currentForce = Vector3.Zero;
    }
    
    public override void Update()
    {
        float deltaTime = GameTime.DeltaTime;
        
        transform.Position = Vector3.Lerp(
            transform.Position,
            physicsPosition,
            interpolationSpeed * deltaTime
        );
    }
    
    public override void FixedUpdate()
    {
        if (!Game.MoveTest)
            return;
        
        float deltaTime = GameTime.FixedDeltaTime;
        
        previousPosition = physicsPosition;
        
        Velocity += Acceleration * deltaTime;

        if (doGravity)
            Gravity();
        
        CollisionCheck();
        
        Velocity *= 1 - Drag * deltaTime;

        physicsPosition += Velocity * deltaTime;
        
        Hitbox.Position = physicsPosition;
        
        Acceleration = Vector3.Zero;
    }
    
    public void Gravity()
    {
        Velocity += new Vector3(0, -gravity * GameTime.FixedDeltaTime, 0);

        Velocity = new Vector3(
            Velocity.X,
            Math.Max(Velocity.Y, -maxFallSpeed),
            Velocity.Z
        );
    }

    public void Stop()
    {
        Velocity = Vector3.Zero;
        targetVelocity = Vector3.Zero;
        currentForce = Vector3.Zero;
        Acceleration = Vector3.Zero;
    }
    
    public float GetSpeed()
    {
        return Velocity.Length;
    }
    
    public Vector3 GetMomentum()
    {
        return Velocity * Mass;
    }
    
    public float GetKineticEnergy()
    {
        return 0.5f * Mass * Velocity.LengthSquared;
    }
    
    // New 3D specific methods
    public void AddTorque(Vector3 torque)
    {
        // Add rotational force - you'll need to implement rotation handling
        // This is a placeholder for now
    }

    public void SnapToBlockY()
    {
        physicsPosition.Y = Mathf.RoundToInt(physicsPosition.Y);
        previousPosition.Y = physicsPosition.Y;
    }
    
    
    public void CollisionCheck()
    {
        if (WorldManager.Instance == null)
            return;
        
        Vector3 checkDistance = Velocity * GameTime.FixedDeltaTime;

        Vector3i[] zPositions = checkDistance.Z < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(0, 0, checkDistance.Z)),
        ]:
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(0, 0, checkDistance.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(0, 0, checkDistance.Z)),
        ];
        
        Vector3i[] xPositions = checkDistance.X < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(checkDistance.X, 0, 0)),
        ]:
        [
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(checkDistance.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(checkDistance.X, 0, 0)),
        ];
        
        Vector3i[] yPositions = checkDistance.Y < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(0, checkDistance.Y, 0)),
        ]:
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(0, checkDistance.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(0, checkDistance.Y, 0)),
        ];
        
        huggingWall = false;

        huggingPosZ = false;
        huggingNegZ = false;
        
        if (WorldManager.IsBlockChecks(zPositions))
        {
            if (Velocity.Z < 0)
                huggingNegZ = true;
            else
                huggingPosZ = true;
            
            huggingWall = true;
            
            Velocity.Z = 0;
        }
        
        huggingPosX = false;
        huggingNegX = false;
        
        if (WorldManager.IsBlockChecks(xPositions))
        {
            if (Velocity.X < 0)
                huggingNegX = true;
            else
                huggingPosX = true;
            
            huggingWall = true;
            
            Velocity.X = 0;
        }
        if (WorldManager.IsBlockChecks(yPositions))
        {
            Velocity.Y = 0;
        }
    }
    public bool IsGroundedCheck()
    {
        Vector3i[] yPositions =
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(0, -0.2f, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(0, -0.2f, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(0, -0.2f, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(0, -0.2f, 0)),
        ];
        
        return WorldManager.IsBlockChecks(yPositions);
    }
    
    #region Getters
    public Vector3 GetHorizontalVelocity()
    {
        return new Vector3(Velocity.X, 0, Velocity.Z);
    }
    #endregion
}