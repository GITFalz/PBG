using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PhysicsBody : Updateable
{
    public float GRAVITY = 1.3f;
    public float MAX_FALL_SPEED = 150f;
    public float DRAG = 1f;
    
    public bool doGravity = false;
    
    public Vector3 Velocity = Vector3.Zero;

    public Hitbox Hitbox;
    public bool IsGrounded;
    
    public bool huggingPosZ = false;
    public bool huggingNegZ = false;
    public bool huggingPosX = false;
    public bool huggingNegX = false;
    
    public bool huggingWall = false;

    public double time = 0;

    public Vector3 oldPosition;
    public Vector3 newPosition;
    
    public PhysicsBody()
    {
        name = "PhysicsBody";
        Hitbox = new Hitbox(new Vector3(1, 2, 1));
    }
    
    public override void FixedUpdate()
    {
        if (!Game.MoveTest)
            return;
        
        if (doGravity)
            Gravity();
        Drag();
        CollisionCheck();

        oldPosition = newPosition;
        newPosition += Velocity;
        Hitbox.Position = newPosition;

        time = 0;
        
        if (InputManager.IsDown(Keys.L))
            Console.WriteLine("Physics : " + newPosition);
    }
    
    public override void Update()
    {
        if (!Game.MoveTest)
            return;
        
        transform.Position = Mathf.Lerp(oldPosition, newPosition, (float)time * GameTime.PhysicSteps);
        if (InputManager.IsDown(Keys.L))
            Console.WriteLine("PlayerPosition : " + transform.Position + " Time : " + time * GameTime.PhysicSteps);
        time = Mathf.Min(time + GameTime.DeltaTime, 1d);
    }
    
    public void Gravity()
    {
        Vector3 newVelocity = Velocity + new Vector3(0, -GRAVITY * GameTime.FixedDeltaTime, 0);
        newVelocity.Y = Mathf.Max(newVelocity.Y, -MAX_FALL_SPEED * GameTime.FixedDeltaTime);
        Velocity = newVelocity;
    }

    public void Drag()
    {
        Velocity = Vector3.Lerp(Velocity, Vector3.Zero, DRAG * GameTime.FixedDeltaTime);
    }
    
    public void AddForce(Vector3 direction, float force)
    {
        AddForce(direction * force);
    }
    public void AddForce(Vector3 direction)
    {
        Velocity += direction;
    }

    public void CollisionCheck()
    {
        if (WorldManager.Instance == null)
            return;

        Vector3i[] zPositions = Velocity.Z < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(0, 0, Velocity.Z)),
        ] :
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(0, 0, Velocity.Z)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(0, 0, Velocity.Z)),
        ];
        
        Vector3i[] xPositions = Velocity.X < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(Velocity.X, 0, 0)),
        ] :
        [
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(Velocity.X, 0, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(Velocity.X, 0, 0)),
        ];
        
        Vector3i[] yPositions = Velocity.Y < 0 ?
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z1 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z1 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y1Z2 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y1Z2 + new Vector3(0, Velocity.Y, 0)),
        ] :
        [
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z1 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z1 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX2Y2Z2 + new Vector3(0, Velocity.Y, 0)),
            Mathf.FloorToInt(Hitbox.CornerX1Y2Z2 + new Vector3(0, Velocity.Y, 0)),
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