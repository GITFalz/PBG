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

    private Action Gravity = () => { };

    private struct PhysicsData
    {
        public Vector3 Position;
        public float Time;
    }

    private Queue<PhysicsData> physicsQueue = new Queue<PhysicsData>();
    private object queueLock = new object();

    public PhysicsBody()
    {
        Name = "PhysicsBody";
        collider = new Collider((-0.4f, 0, -0.4f), (0.4f, 1.75f, 0.4f));
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

        float renderTime = (float)(GameTime.TotalTime - GameTime.FixedDeltaTime); // or add a small interpolation offset
        var (a, b) = GetValidPhysicsData(renderTime);
        float t = (renderTime - a.Time) / (b.Time - a.Time + float.Epsilon);
        Transform.Position = Vector3.Lerp(a.Position, b.Position, Mathf.Clamp(0, 1, t));
    }
    
    void FixedUpdate()
    {
        if (!PlayerData.TestInputs)
            return;
        
        Gravity();
        Velocity += Acceleration * (float)GameTime.FixedDeltaTime;
        //Console.WriteLine($"Check1 -- Velocity: {Velocity} Acceleration: {Acceleration} IsGrounded: {IsGrounded} Position previous: {previousPosition} physics: {physicsPosition}");
        Velocity *= 1f - Drag * (float)GameTime.FixedDeltaTime;
        //Console.WriteLine($"Check2 -- Velocity: {Velocity} Acceleration: {Acceleration} IsGrounded: {IsGrounded} Position previous: {previousPosition} physics: {physicsPosition}");
        float decelerationFactor = IsGrounded ? 10f : DecelerationFactor;
        Acceleration = -decelerationFactor * Velocity;
        //Console.WriteLine($"Check3 -- Velocity: {Velocity} Acceleration: {Acceleration} IsGrounded: {IsGrounded} Position previous: {previousPosition} physics: {physicsPosition}");
        CollisionCheck();
        //Console.WriteLine($"Check4   -- Velocity: {Velocity} Acceleration: {Acceleration} IsGrounded: {IsGrounded} Position previous: {previousPosition} physics: {physicsPosition}");
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

    private (PhysicsData, PhysicsData) GetValidPhysicsData(float renderTime)
    {
        lock (queueLock)
        {
            if (physicsQueue.Count < 2)
            {
                var only = physicsQueue.FirstOrDefault();
                return (only, only);
            }

            PhysicsData prev = physicsQueue.First();

            foreach (var current in physicsQueue)
            {
                if (current.Time >= renderTime)
                {
                    return (prev, current);
                }
                prev = current;
            }

            return (physicsQueue.Last(), physicsQueue.Last());
        }
    }
    
    
    public void CollisionCheck()
    {
        Collider currentCollider = collider + physicsPosition;
        Vector3 checkDistance = Velocity * (float)GameTime.FixedDeltaTime;

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

        Vector3 newPhysicsPosition = physicsPosition;

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
            newPhysicsPosition.Y += testingDirection.Y * entryData.entryTime;
            IsGrounded = entryData.normal.Value.Y > 0;
        }
        else
        {
            newPhysicsPosition.Y += checkDistance.Y;
        }

        currentCollider = collider + newPhysicsPosition;

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
            newPhysicsPosition.X += testingDirection.X * entryData.entryTime;
        }
        else
        {
            newPhysicsPosition.X += checkDistance.X;
        }

        currentCollider = collider + newPhysicsPosition;

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
            newPhysicsPosition.Z += testingDirection.Z * entryData.entryTime;
        }
        else
        {
            newPhysicsPosition.Z += checkDistance.Z;
        }

        if (Velocity.Y != 0)
            IsGrounded = false;

        previousPosition = physicsPosition;
        physicsPosition = newPhysicsPosition;
        lock (queueLock)
        {
            physicsQueue.Enqueue(new PhysicsData
            {
                Position = physicsPosition,
                Time = GameTime.FixedTotalTime
            });
            if (physicsQueue.Count > 5)
                physicsQueue.Dequeue(); // Limit the queue size to avoid memory issues
        }
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