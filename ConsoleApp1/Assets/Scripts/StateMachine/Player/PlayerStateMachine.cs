using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerStateMachine : ScriptingNode
{
    public static PlayerStateMachine Instance;
    public CameraMode cameraMode = CameraMode.Follow;
    public const float WALK_SPEED = 7;
    public const float RUN_SPEED = 14;
    public const float DASH_SPEED = 24;
    public const float SPRINT_SPEED = 22;
    public const float FALL_SPEED = 1;
    public const float GRAPPLE_SPEED = 30;
    public const float JUMP_SPEED = 14;
    
    public Vector3 forward = (0, 0, -1);
    private Vector3 _oldPosition = (0, 0, 0);

    private ShaderProgram _hitShader = new ShaderProgram("Info/Hit.vert", "Info/Hit.frag");
    private VAO _hitVao = new VAO();
    
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
    public PhysicsBody physicsBody;

    private ShaderProgram _shaderProgram;
    
    public float yaw;
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
    
    private Vector3 _lastCameraPosition;
    private float _lastCameraYaw;
    private float _lastCameraPitch;

    private Action _renderHit = () => {};

    public PlayerStateMachine()
    {
        // To be changed (make PLAYER state machine static???)
        Instance = this;
    }
    
    public override void Start()
    {
        new OldAnimationManager();
        
        _lastCameraPitch = Game.camera.pitch;
        _lastCameraYaw = Game.camera.yaw;
        _lastCameraPosition = Game.camera.Position;
        
        physicsBody = Transform.GetComponent<PhysicsBody>();
        
        _currentState = _gameState;
        _currentState.Enter(this);
        
        _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
        
        Transform.Position = new Vector3(0, 200, 0);
        physicsBody.SetPosition(Transform.Position);
        
        _playerMesh = new OldAnimationMesh();
        VoxelData.GenerateStandardMeshBox(_playerMesh, 
            new Vector3(0.8f, 1.75f, 0.8f), 
            new Vector3(-0.4f, -1.75f/2, -0.4f), 
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

        camera.SetCameraMode(CameraMode.Follow);
        
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

        Vector2 input = Input.GetMovementInput();
        
        if (input != Vector2.Zero)
            yaw = -camera.yaw + _inputAngle[input];
        
        forward = Mathf.YAngleToDirection(-yaw);
            
        _currentState.Update(this);

        if (input != Vector2.Zero)
            Info.SetPositionText(_oldPosition, Transform.Position - (0f, 0.875f, 0f));

        camera.Center = Transform.Position + (0f, 0.5f, 0f);

        if (VoxelData.Raycast(camera.Center, camera.front, 4, out Hit hit))
        {
            Vector3i blockPos = hit.BlockPosition;
            Vector3i n = hit.Normal;
            int index = n.X != 0 ? (n.X == 1 ? 1 : 3) : (n.Y != 0 ? (n.Y == 1 ? 2 : 4) : n.Y == 1 ? 5 : 0);
            _renderHit = () => RenderHit((blockPos.X, blockPos.Y, blockPos.Z, index));

            if (Input.IsMousePressed(MouseButton.Right)) 
                WorldManager.SetBlock(blockPos + n, out ChunkData chunkData);
        }

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

        _shaderProgram.Bind();
        
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
        
        _shaderProgram.Unbind();

        _renderHit();
    }

    private void RenderHit(Vector4i data)
    {
        _hitShader.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Game.camera.viewMatrix;
        Matrix4 projection = Game.camera.projectionMatrix;

        int modelLocationA = GL.GetUniformLocation(_hitShader.ID, "model");
        int viewLocationA = GL.GetUniformLocation(_hitShader.ID, "view");
        int projectionLocationA = GL.GetUniformLocation(_hitShader.ID, "projection");
        int faceDataLocationA = GL.GetUniformLocation(_hitShader.ID, "faceData");

        GL.UniformMatrix4(viewLocationA, true, ref view);
        GL.UniformMatrix4(projectionLocationA, true, ref projection);
        GL.UniformMatrix4(modelLocationA, true, ref model);
        GL.Uniform4(faceDataLocationA, data);

        _hitVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    
        _hitVao.Unbind();

        _hitShader.Unbind();

        _renderHit = () => {};
    }

    public void Teleport(Vector3 position)
    {
        physicsBody.physicsPosition = position;
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

    public bool IsGrounded()
    {
        return physicsBody.IsGrounded;
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

    public void ToggleView()
    {
        Game.camera.SetCameraMode(cameraMode == CameraMode.Centered ? CameraMode.Follow : CameraMode.Centered);
    }

    public bool IsHuggingWall()
    {
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