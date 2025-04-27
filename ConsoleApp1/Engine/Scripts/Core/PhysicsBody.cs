using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PhysicsBody : ScriptingNode
{
    private float gravity = 50;
    private float maxFallSpeed = 150f;
    
    public Vector3 Acceleration = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;     
    public float Drag = 0.2f;
    public float DecelerationFactor = 1;
    public float Mass = 1f;

    public Collider collider;
    public bool IsGrounded;
    
    public Vector3 physicsPosition;
    private Vector3 previousPosition;
    private float interpolationSpeed = 20f;

    private Action Gravity = () => { };

    public PhysicsBody()
    {
        Name = "PhysicsBody";
        Vector3 size = (0.8f, 1.75f, 0.8f);
        Vector3 halfSize = size / 2;
        collider = new Collider(-halfSize, halfSize);
        EnableGravity();
    }

    void Exit()
    {
        Acceleration = Vector3.Zero;
        Velocity = Vector3.Zero;
    }

    public void DisableGravity()
    {
        Gravity = () => { };
    }

    public void EnableGravity()
    {
        Gravity = ApplyGravity;
    }
    
    public void SetPosition(Vector3 position)
    {
        physicsPosition = position;
        previousPosition = position;
        Transform.Position = position;
    }

    public void AddForce(Vector3 force)
    {
        Acceleration += force / Mass;
    }

    public void AddForce(Vector3 direction, float maxSpeed)
    {
        Acceleration += direction.Normalized() * maxSpeed / Mass;
    }

    void Update()
    {
        if (!PlayerData.TestInputs)
            return;

        Transform.Position = Vector3.Lerp(Transform.Position, physicsPosition, GameTime.DeltaTime * interpolationSpeed);
    }
    
    void FixedUpdate()
    {
        if (!PlayerData.TestInputs)
            return;

        previousPosition = physicsPosition;
        
        Gravity();
        Velocity += Acceleration * GameTime.FixedTime;
        Velocity *= 1f - Drag * GameTime.FixedTime;
        float decelerationFactor = IsGrounded ? 10f : DecelerationFactor;
        Acceleration = -decelerationFactor * Velocity;
        CollisionCheck();
    }
    
    public void ApplyGravity()
    {
        Acceleration.Y -= gravity;
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
        Collider currentCollider = collider + physicsPosition;
        Vector3 checkDistance = Velocity * GameTime.FixedTime;

        Vector3 pos1 = physicsPosition;
        Vector3 pos2 = physicsPosition + checkDistance.Normalized() * 3;

        Vector3i min = Mathf.FloorToInt(Vector3.ComponentMin(pos1, pos2) - (2, 2, 2));
        Vector3i max = Mathf.FloorToInt(Vector3.ComponentMax(pos1, pos2) + (2, 2, 2));

        List<Vector3i> blockPositions = [];

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                for (int z = min.Z; z <= max.Z; z++)
                {
                    blockPositions.Add((x, y, z));
                }
            }
        }

        // Check Y axis collision first
        (float entryTime, Vector3? normal) entryData = (1f, null);
        Vector3 testingDirection = (0, checkDistance.Y, 0);

        foreach (var position in blockPositions)
        {
            if (!WorldManager.GetBlock(position)) continue;
            entryData = BlockCollision.GetEntry(currentCollider, testingDirection, position, 1, entryData);
        }

        if (entryData.normal != null && entryData.normal.Value.Y != 0)
        {
            Velocity.Y = 0;
            physicsPosition.Y += testingDirection.Y * entryData.entryTime;
            IsGrounded = entryData.normal.Value.Y > 0;
        }
        else
        {
            physicsPosition.Y += checkDistance.Y;
        }

        currentCollider = collider + physicsPosition;

        // Check X axis collision
        entryData = (1f, null);
        testingDirection = (checkDistance.X, 0, 0);

        foreach (var position in blockPositions)
        {
            if (!WorldManager.GetBlock(position)) continue;
            entryData = BlockCollision.GetEntry(currentCollider, testingDirection, position, 1, entryData);
        }

        if (entryData.normal != null && entryData.normal.Value.X != 0)
        {
            Velocity.X = 0;
            physicsPosition.X += testingDirection.X * entryData.entryTime;
        }
        else
        {
            physicsPosition.X += checkDistance.X;
        }

        currentCollider = collider + physicsPosition;

        // Check Z axis collision
        entryData = (1f, null);
        testingDirection = (0, 0, checkDistance.Z);

        foreach (var position in blockPositions)
        {
            if (!WorldManager.GetBlock(position)) continue;
            entryData = BlockCollision.GetEntry(currentCollider, testingDirection, position, 1, entryData);
        }

        if (entryData.normal != null && entryData.normal.Value.Z != 0)
        {
            Velocity.Z = 0;
            physicsPosition.Z += testingDirection.Z * entryData.entryTime;
        }
        else
        {
            physicsPosition.Z += checkDistance.Z;
        }


        if (Velocity.Y != 0)
            IsGrounded = false;
    }

    public Collider GetCollider()
    {
        return collider + physicsPosition;
    }
    
    #region Getters
    public Vector3 GetHorizontalVelocity()
    {
        return new Vector3(Velocity.X, 0, Velocity.Z);
    }
    #endregion
}