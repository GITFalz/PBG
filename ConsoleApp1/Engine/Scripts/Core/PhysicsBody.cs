using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PhysicsBody : ScriptingNode
{
    public float gravity = 50f;
    public float maxFallSpeed = 150f;
    
    private bool doGravity = false;
    
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
    private float interpolationSpeed = 20f;

    private Action Gravity = () => { };

    public PhysicsBody()
    {
        Name = "PhysicsBody";
        Hitbox = new Hitbox(new Vector3(0.8f, 1.75f, 0.8f));
    }

    public PhysicsBody(bool doGravity)
    {
        Name = "PhysicsBody";
        Hitbox = new Hitbox(new Vector3(0.8f, 1.75f, 0.8f));
        if (doGravity) EnableGravity();
    }

    public void DisableGravity()
    {
        doGravity = false;
        Gravity = () => { };
    }

    public void EnableGravity()
    {
        doGravity = true;
        Gravity = ApplyGravity;
    }
    
    public void SetPosition(Vector3 position)
    {
        physicsPosition = position;
        previousPosition = position;
        Hitbox.Position = position;
        Transform.Position = position;
    }

    public void AddForce(Vector3 force)
    {
        Velocity += force / Mass;
    }

    public void AddForce(Vector3 direction, float maxSpeed)
    {
        Velocity += direction.Normalized() * maxSpeed / Mass;
    }

    public override void Update()
    {
        Transform.Position = Vector3.Lerp(Transform.Position, physicsPosition, GameTime.DeltaTime * interpolationSpeed);
    }
    
    public override void FixedUpdate()
    {
        if (!Game.MoveTest)
            return;
        
        previousPosition = physicsPosition;

        Velocity += Acceleration * GameTime.FixedDeltaTime;
        Velocity *= 1 - Drag * GameTime.FixedDeltaTime;
        Acceleration *= 1 - (Drag * 100) * GameTime.FixedDeltaTime;

        Gravity();
        CollisionCheck();

        physicsPosition += Velocity * GameTime.FixedDeltaTime;
        Hitbox.Position = physicsPosition;
    }
    
    public void ApplyGravity()
    {
        Velocity.Y = Math.Max(Velocity.Y - gravity * GameTime.FixedDeltaTime, -maxFallSpeed);
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
            Acceleration.Z = 0;
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
            Acceleration.X = 0;
        }
        if (WorldManager.IsBlockChecks(yPositions))
        {
            Velocity.Y = 0;
            Acceleration.Y = 0;
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