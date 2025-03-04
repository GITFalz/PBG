using System.Collections;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Engine.Scripts.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerStateMachine : ScriptingNode
{
    public const float WALK_SPEED = 7;
    public const float RUN_SPEED = 14;
    public const float DASH_SPEED = 24;
    public const float SPRINT_SPEED = 22;
    public const float FALL_SPEED = 1;
    public const float GRAPPLE_SPEED = 30;
    public const float JUMP_SPEED = 14;
    
    public Vector3 forward = (0, 0, -1);
    private Vector3 _oldPosition = (0, 0, 0);
    
    public static readonly Dictionary<PlayerMovementSpeed, float> Speeds = new Dictionary<PlayerMovementSpeed, float>()
    {
        {PlayerMovementSpeed.Walk, WALK_SPEED},
        {PlayerMovementSpeed.Run, RUN_SPEED},
        {PlayerMovementSpeed.Sprint, SPRINT_SPEED},
        {PlayerMovementSpeed.Fall, FALL_SPEED},
        {PlayerMovementSpeed.Grappling, GRAPPLE_SPEED},
        {PlayerMovementSpeed.Jump, JUMP_SPEED},
    };
    
    private PlayerBaseState _currentState;
    
    private PlayerGameState _gameState = new();
    private PlayerMenuState _menuState = new();
    
    // Animation
    private OldAnimationMesh _playerMesh;
    private EntityMesh _mesh;
    public PhysicsBody physicsBody;

    private ShaderProgram _shaderProgram;
    
    public float yaw;
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
    
    private Vector3 _lastCameraPosition;
    private float _lastCameraYaw;
    private float _lastCameraPitch;
    
    public override void Start()
    {
        new OldAnimationManager();

        Game.camera = new Camera(Game.Width, Game.Height, new Vector3(0, 20, 0));
        
        _lastCameraPitch = Game.camera.pitch;
        _lastCameraYaw = Game.camera.yaw;
        _lastCameraPosition = Game.camera.Position;
        
        physicsBody = Transform.GetComponent<PhysicsBody>();
        
        _currentState = _gameState;
        _currentState.Enter(this);
        
        _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
        
        Transform.Position = new Vector3(0, 60, 0);
        physicsBody.SetPosition(Transform.Position);
        
        //Mesh
        _mesh = new EntityMesh();
        VoxelData.GetEntityBoxMesh(_mesh, new Vector3(0.8f, 1.75f, 0.8f), new Vector3(0, 0, 0), 0);
        _mesh.GenerateBuffers();
        
        _playerMesh = new OldAnimationMesh();
        VoxelData.GenerateStandardMeshBox(_playerMesh, 
            new Vector3(0.8f, 1.75f, 0.8f), 
            new Vector3(-0.4f, 0, -0.4f), 
            new Vector3(0, 0, 0), 
            1
        );
        
        _playerMesh.GenerateBuffers();
        _playerMesh.UpdateMesh();
    }

    public override void Resize()
    {
        
    }

    public override void Awake()
    {
        Camera camera = Game.camera;

        camera.SetCameraMode(CameraMode.Free);
        
        camera.Position = _lastCameraPosition;
        camera.yaw = _lastCameraYaw;
        camera.pitch = _lastCameraPitch;
        
        camera.UpdateVectors();
        
        base.Awake();
    }

    public override void Update()
    {
        if (!Game.MoveTest)
            return;
            
        Camera camera = Game.camera;

        camera.Center = Transform.Position + new Vector3(0, 1.7f, 0);

        Vector2 input = Input.GetMovementInput();
        
        if (input != Vector2.Zero)
            yaw = -camera.yaw + _inputAngle[input];
        
        forward = Mathf.YAngleToDirection(-yaw);
        
        _currentState.Update(this);

        IsHuggingWall();

        if (camera.GetCameraMode() == CameraMode.Follow)
            Info.SetPositionText(_oldPosition, Transform.Position);

        _oldPosition = Transform.Position;
    }
    
    public override void FixedUpdate()
    {
        if (!Game.MoveTest)
            return;
        
        _currentState.FixedUpdate(this);
    }

    public override void Render()
    {
        Camera camera = Game.camera;

        //Shader.Error("Before Player Render");

        _shaderProgram.Bind();

        //Shader.Error("After Player Bind: " + _shaderProgram.ID);
        
        Matrix4 model = Matrix4.CreateTranslation(Transform.Position);
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        _playerMesh.RenderMesh();

        //Shader.Error("After Player Render Mesh");
        
        _shaderProgram.Unbind();
    }

    public override void Exit()
    {
        Camera camera = Game.camera;

        Console.WriteLine("Exiting Player State Machine");
        
        _lastCameraPosition = camera.Position;
        _lastCameraYaw = camera.yaw;
        _lastCameraPitch = camera.pitch;
        
        base.Exit();
    }

    public int MeshUpdate()
    {
        _mesh.Position = Transform.Position + new Vector3(-0.4f, 0, -0.4f);

        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
 
        return 1;
    }

    public void MeshRotateUpdate()
    {
        _mesh.Position = Transform.Position + new Vector3(-0.5f, 0, -0.5f);
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public void SnapToBlockUnder()
    {
        physicsBody.SnapToBlockY();
        _mesh.Position = Transform.Position + new Vector3(-0.5f, 0, -0.5f);
        
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
        Vector2 input = Input.GetMovementInput();
        if (input == Vector2.Zero)
            return;
        MovePlayer(playerMovementSpeed, input);
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector2 input)
    {
        Camera camera = Game.camera;
        
        Vector3 direction = camera.FrontYto0() * input.Y - camera.RightYto0() * input.X;
        Vector3 horizontalVelocity = physicsBody.GetHorizontalVelocity();
        MovePlayer(playerMovementSpeed, direction);
        physicsBody.Velocity -= horizontalVelocity;
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
            HugZ.Y = Mathf.FloorToInt(Transform.Position.Y + 0.5f);

        if (physicsBody.Velocity.X < 0)
            HugX = Mathf.FloorToInt(physicsBody.Hitbox.CornerX1Y1Z1 + new Vector3(-1f, 0.5f, 0.5f));
        else if (physicsBody.Velocity.X > 0)
            HugX = Mathf.FloorToInt(physicsBody.Hitbox.CornerX2Y1Z1 + new Vector3(1f, 0.5f, 0.5f));
        else
            HugX.Y = Mathf.FloorToInt(Transform.Position.Y + 0.5f);
        
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
        physicsBody.SetPosition(position);
    }
}

public enum PlayerMovementSpeed
{
    Walk,
    Run,
    Sprint,
    Fall,
    Grappling,
    Jump,
}