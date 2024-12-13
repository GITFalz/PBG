using System.Collections;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Engine.Scripts.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

public class PlayerStateMachine : Updateable
{
    public const float WALK_SPEED = 7f;
    public const float SPRINT_SPEED = 30f;
    
    public static readonly Dictionary<PlayerMovementSpeed, float> Speeds = new Dictionary<PlayerMovementSpeed, float>()
    {
        {PlayerMovementSpeed.Walk, WALK_SPEED},
        {PlayerMovementSpeed.Sprint, SPRINT_SPEED},
    };
    
    private PlayerBaseState _currentState;
    
    private PlayerGameState _gameState = new();
    private PlayerMenuState _menuState = new();
    
    private EntityMesh _mesh;
    
    private ShaderProgram _shaderProgram;
    
    public WorldManager WorldManager;
    
    public PhysicsBody physicsBody;

    public float yaw;
    
    private FrameEventArgs _args;

    public bool isGrounded;
    
    public override void Start()
    {
        physicsBody = gameObject.GetComponent<PhysicsBody>();
        
        _currentState = _gameState;
        _currentState.Enter(this);
        
        _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
        
        transform.Position = new Vector3(0, 100, 0);
        
        _mesh = new EntityMesh();

        VoxelData.GetEntityBoxMesh(_mesh, new Vector3(1, 2, 1), new Vector3(0, 0, 0), 1);

        _mesh.GenerateBuffers();

        CanUpdatePhysics();
    }

    public override void Update(FrameEventArgs args)
    {
        _args = args;
        _currentState.Update(this);
        PlayerData.Position = transform.Position + new Vector3(0.5f, 1.8f, 0.5f);
    }
    
    public override void FixedUpdate()
    {
        _currentState.FixedUpdate(this);
    }

    public override void Render()
    {
        _shaderProgram.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Camera.viewMatrix;
        Matrix4 projection = Camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        _mesh.RenderMesh();
        
        _shaderProgram.Unbind();
    }

    public int GravityUpdate()
    {
        _mesh.Position = transform.Position;

        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();

        return 1;
    }

    public void MoveMeshUpdate()
    {
        _mesh.Position = transform.Position;
        
        yaw = -Camera.GetYaw();
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public void SnapToBlockUnder()
    {
        transform.Position.Y = Mathf.RoundToInt(transform.Position.Y);
        _mesh.Position = transform.Position;
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public int CanUpdatePhysics()
    {
        Queue<Vector3i> positions = new Queue<Vector3i>();
        
        int sign = (int)Mathf.Sign(physicsBody.Velocity.Y);
        int abs = (int)Mathf.Abs(physicsBody.Velocity.Y) + 1;
        
        for (int i = 0; i < abs; i++)
        {
            Vector3 futurePosition = transform.Position + physicsBody.Velocity;
            
            int x = (int)futurePosition.X;
            int y = (int)futurePosition.Y + sign * i;
            int z = (int)futurePosition.Z;
            
            positions.Enqueue(new Vector3i(x, y, z));
        }

        Vector3i? closest = null;
        
        while (positions.TryDequeue(out var position))
        {
            int result = WorldManager.GetBlock(position, out Block? block);
            
            if (result == 0)
            {
                closest = position;
                break;
            }
            if (result == 2)
            {
                return -1;
            }
        }
        
        if (closest != null)
        {
            physicsBody.Velocity.Y = 0;
            return 0;
        }

        return 1;
    }

    public bool IsGrounded()
    {
        Vector3 posA = new Vector3(0, -0.1f + physicsBody.Velocity.Y, 0) + transform.Position;
        Vector3 posB = new Vector3(0, -0.1f + physicsBody.Velocity.Y, -1) + transform.Position;
        
        Vector3i a = new Vector3i((int)posA.X, (int)posA.Y, (int)posA.Z);
        Vector3i b = new Vector3i((int)posB.X, (int)posB.Y, (int)posB.Z);

        Block? block;
        
        int result = WorldManager.GetBlock(a, out block);
            
        if (result == 0)
        {
            isGrounded = true;
            return true;
        }
        
        result = WorldManager.GetBlock(b, out block);
        
        if (result == 0)
        {
            isGrounded = true;
            return true;
        }
        
        isGrounded = false;
        return false;
    }
    
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed)
    {
        Vector2 input = InputManager.GetMovementInput();
        
        Vector3 direction = Camera.FrontYto0() * input.Y - Camera.RightYto0() * input.X;
        Vector3 oldVelocity = physicsBody.Velocity;
        
        direction = Mathf.Normalize(direction);
        
        physicsBody.AddForce(direction, Speeds[playerMovementSpeed]);
        physicsBody.Velocity -= oldVelocity;
    }
}

public enum PlayerMovementSpeed
{
    Walk,
    Sprint,
}