using System.Collections;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Engine.Scripts.Core;
using ConsoleApp1.Engine.Scripts.Core.Voxel;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

public class PlayerStateMachine : Updateable
{
    public const float GRAVITY = 0.1f;
    public const float MAX_FALL_SPEED = 1.5f;
    public const float WALK_SPEED = 7f;
    public const float SPRINT_SPEED = 18f;
    
    private static readonly Dictionary<PlayerMovementSpeed, float> Speeds = new Dictionary<PlayerMovementSpeed, float>()
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
    
    public Vector3 Position;
    public Vector3 Velocity;
    public float yaw;
    
    private FrameEventArgs _args;

    public bool isGrounded;
    
    public override void Start()
    {
        _currentState = _gameState;
        _currentState.Enter(this);
        
        _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
        
        Position = new Vector3(0, 100, 0);
        
        _mesh = new EntityMesh();

        VoxelData.GetEntityBoxMesh(_mesh, new Vector3(1, 2, 1), new Vector3(0, 0, 0), 1);

        _mesh.GenerateBuffers();
    }

    public override void Update(FrameEventArgs args)
    {
        _args = args;
        _currentState.Update(this);
        PlayerData.Position = Position + new Vector3(0.5f, 1.8f, 0.5f);
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
    
    public int ApplyGravity()
    {
        Vector3 newVelocity = Velocity + new Vector3(0, -GRAVITY * GameTime.DeltaTime, 0);
        newVelocity.Y = Mathf.Min(newVelocity.Y, MAX_FALL_SPEED);
        
        Queue<Vector3i> positions = new Queue<Vector3i>();
        
        int sign = (int)Mathf.Sign(newVelocity.Y);
        int abs = (int)Mathf.Abs(newVelocity.Y) + 1;
        
        for (int i = 0; i < abs; i++)
        {
            Vector3 futurePosition = Position + newVelocity;
            
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
            newVelocity.Y = 0;
            Velocity.Y = 0;
            return 0;
        }
        
        Position += newVelocity;
        Velocity = newVelocity;
        
        _mesh.Position = Position;

        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();

        return 1;
    }

    public void MovePlayer(Vector2 input, PlayerMovementSpeed movementSpeed)
    {
        float speed = Speeds[movementSpeed] * (float)_args.Time;
        Vector3 addedVelocity = input.Y * Camera.FrontYto0() - input.X * Camera.RightYto0();
        Velocity = Vector3.Lerp(Velocity, addedVelocity.Normalized() * speed, 0.5f);
        
        Position += Velocity;
        _mesh.Position = Position;
        
        yaw = -Camera.GetYaw();
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public void SnapToBlockUnder()
    {
        Position.Y = Mathf.FloorToInt(Position.Y);
        _mesh.Position = Position;
        _mesh.UpdateMesh();
    }

    public bool IsGrounded()
    {
        Vector3 posA = new Vector3(0, -0.1f + Velocity.Y, 0) + Position;
        Vector3 posB = new Vector3(0, -0.1f + Velocity.Y, -1) + Position;
        
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
}

public enum PlayerMovementSpeed
{
    Walk,
    Sprint,
}