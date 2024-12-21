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
    public const float WALK_SPEED = 0.2f;
    public const float SPRINT_SPEED = 0.3f;
    public const float FALL_SPEED = 0.2f;
    public const float GRAPPLE_SPEED = 0.6f;
    
    public Vector3 forward = new Vector3(0, 0, -1);
    
    public static readonly Dictionary<PlayerMovementSpeed, float> Speeds = new Dictionary<PlayerMovementSpeed, float>()
    {
        {PlayerMovementSpeed.Walk, WALK_SPEED},
        {PlayerMovementSpeed.Sprint, SPRINT_SPEED},
        {PlayerMovementSpeed.Fall, FALL_SPEED},
        {PlayerMovementSpeed.Grappling, GRAPPLE_SPEED},
    };
    
    private PlayerBaseState _currentState;
    
    private PlayerGameState _gameState = new();
    private PlayerMenuState _menuState = new();
    
    private EntityMesh _mesh;
    
    // Animation
    private AnimationMesh _playerMesh;
    private AnimationMesh _swordMesh;
    
    private AnimationController? _playerAnimationController;
    private AnimationController? _swordAnimationController;
    
    
    private ShaderProgram _shaderProgram;
    public PhysicsBody physicsBody;
    
    public Animation currentAnimation;
    
    public float yaw;
    
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
    public override void Start()
    {
        physicsBody = gameObject.GetComponent<PhysicsBody>();
        
        _currentState = _gameState;
        _currentState.Enter(this);
        
        _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
        
        transform.Position = new Vector3(0, 60, 0);
        physicsBody.newPosition = transform.Position;
        
        //Mesh
        _mesh = new EntityMesh();
        VoxelData.GetEntityBoxMesh(_mesh, new Vector3(1, 2, 1), new Vector3(0, 0, 0), 0);
        _mesh.GenerateBuffers();
        
        _swordMesh = new AnimationMesh();
        VoxelData.GenerateStandardMeshBox(_swordMesh,
            new Vector3(0.3f, 0.2f, 0.2f), 
            new Vector3(0, 0, 0), 
            new Vector3(0, 0, 0), 
            0
        );
        VoxelData.GenerateStandardMeshBox(_swordMesh, 
            new Vector3(0.8f, 2f, 0.2f), 
            new Vector3(-0.3f, 0.3f, 0), 
            new Vector3(0, 0, 0), 
            0
        );
        
        _playerMesh = new AnimationMesh();
        VoxelData.GenerateStandardMeshBox(_playerMesh, 
            new Vector3(1, 2, 1), 
            new Vector3(-0.5f, 0, -0.5f), 
            new Vector3(0, 0, 0), 
            1
        );
        
        _swordMesh.GenerateBuffers();
        _swordMesh.UpdateMesh();
        
        _playerMesh.GenerateBuffers();
        _playerMesh.UpdateMesh();
        
        //Animation
        if (!AnimationManager.Instance.SetMesh("Player", _playerMesh))
            throw new System.Exception("Failed to set mesh");
        
        if (!AnimationManager.Instance.SetMesh("Sword", _swordMesh))
            throw new System.Exception("Failed to set mesh");
        
        if (!AnimationManager.Instance.GetController("Player", out _playerAnimationController) || _playerAnimationController == null)
            throw new System.Exception("Failed to get controller");
        
        if (!AnimationManager.Instance.GetController("Sword", out _swordAnimationController) || _swordAnimationController == null)
            throw new System.Exception("Failed to get controller");
    }

    public override void Update()
    {
        Vector2 input = InputManager.GetMovementInput();
        
        if (input != Vector2.Zero)
            yaw = -Camera.GetYaw() + _inputAngle[input];
        
        _mesh.Position = transform.Position + new Vector3(-0.5f, 0, -0.5f);
        
        forward = Mathf.YAngleToDirection(-yaw);
        
        if (!Game.MoveTest)
            return;
            
        _swordMesh.WorldPosition = transform.Position + new Vector3(0, 1f, 0);
        _playerMesh.WorldPosition = transform.Position;
        
        _swordMesh.Init();
        _playerMesh.Init();
        
        if (InputManager.IsKeyPressed(Keys.M))
            physicsBody.doGravity = !physicsBody.doGravity;
        
        _currentState.Update(this);
        PlayerData.Position = transform.Position;
        PlayerData.EyePosition = transform.Position + new Vector3(0, 1.8f, 0);
        
        if (_swordAnimationController != null && _swordAnimationController.Update(yaw))
        {
            _swordMesh.Center();
            _swordMesh.UpdateMesh();
        }

        if (_playerAnimationController != null && _playerAnimationController.Update(yaw))
        {
            _playerMesh.Center();
            _playerMesh.UpdateMesh();
        }

        IsHuggingWall();
    }
    
    public override void FixedUpdate()
    {
        if (!Game.MoveTest)
            return;
        
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
        
        //_mesh.RenderMesh();
        _swordMesh.RenderMesh();
        _playerMesh.RenderMesh();
        
        _shaderProgram.Unbind();
        
        //Debug.Render();
    }

    public int MeshUpdate()
    {
        _mesh.Position = transform.Position + new Vector3(-0.5f, 0, -0.5f);

        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
 
        return 1;
    }

    public void MeshRotateUpdate()
    {
        _mesh.Position = transform.Position + new Vector3(-0.5f, 0, -0.5f);
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public void SnapToBlockUnder()
    {
        SetPosition(new Vector3(transform.Position.X, Mathf.RoundToInt(transform.Position.Y), transform.Position.Z));
        _mesh.Position = transform.Position + new Vector3(-0.5f, 0, -0.5f);
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public bool IsGrounded()
    {
        return physicsBody.IsGroundedCheck();
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
        if (input == Vector2.Zero)
            return;
        MovePlayer(playerMovementSpeed, input);
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector2 input)
    {
        Vector3 direction = Camera.FrontYto0() * input.Y - Camera.RightYto0() * input.X;
        Vector3 oldVelocity = physicsBody.GetHorizontalVelocity();
        MovePlayer(playerMovementSpeed, direction);
        physicsBody.Velocity -= oldVelocity;
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector3 direction)
    {
        direction = Mathf.Normalize(direction);
        physicsBody.AddForce(direction, Speeds[playerMovementSpeed]);
    }

    public bool IsHuggingWall()
    {
        if (WorldManager.Instance == null)
            return false;

        if (physicsBody.Velocity.Z < 0)
            HugZ = Mathf.FloorToInt(physicsBody.Hitbox.CornerX1Y1Z1 + new Vector3(0.5f, 0.5f, -1f));
        else if (physicsBody.Velocity.Z > 0)
            HugZ = Mathf.FloorToInt(physicsBody.Hitbox.CornerX1Y1Z2 + new Vector3(0.5f, 0.5f, 1f));
        else
            HugZ.Y = Mathf.FloorToInt(transform.Position.Y + 0.5f);

        if (physicsBody.Velocity.X < 0)
            HugX = Mathf.FloorToInt(physicsBody.Hitbox.CornerX1Y1Z1 + new Vector3(-1f, 0.5f, 0.5f));
        else if (physicsBody.Velocity.X > 0)
            HugX = Mathf.FloorToInt(physicsBody.Hitbox.CornerX2Y1Z1 + new Vector3(1f, 0.5f, 0.5f));
        else
            HugX.Y = Mathf.FloorToInt(transform.Position.Y + 0.5f);
        
        //Debug.DrawBox(HugZ, new Vector3(0.5f, 0.5f, 0.5f));

        if (WorldManager.IsBlockChecks(HugZ))
            return true;
        if (WorldManager.IsBlockChecks(HugX))
            return true;
        
        return false;
    }

    private readonly Dictionary<Vector2, float> _inputAngle = new Dictionary<Vector2, float>()
    {
        { new Vector2(0, 1), 0 },
        { new Vector2(1, 1), 45 },
        { new Vector2(1, 0), 90 },
        { new Vector2(1, -1), 135 },
        { new Vector2(0, -1), 180 },
        { new Vector2(-1, -1), 225 },
        { new Vector2(-1, 0), 270 },
        { new Vector2(-1, 1), 315 },
    };
    
    public void SetPosition(Vector3 position)
    {
        transform.Position = position;
        physicsBody.newPosition = position;
    }
}

public enum PlayerMovementSpeed
{
    Walk,
    Sprint,
    Fall,
    Grappling,
}