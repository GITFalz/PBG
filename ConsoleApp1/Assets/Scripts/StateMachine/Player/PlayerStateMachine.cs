using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerStateMachine : ScriptingNode
{
    public static PlayerStateMachine Instance;
    public CameraMode cameraMode = CameraMode.Follow;
    public const float WALK_SPEED = 80;
    public const float RUN_SPEED = 120;
    public const float DASH_SPEED = 4000;
    public const float SPRINT_SPEED = 160;
    public const float FALL_SPEED = 15;
    public const float GRAPPLE_SPEED = 200;
    public const float JUMP_SPEED = 900;
    
    public Vector3 forward = (0, 0, -1);
    private Vector3 _oldPosition = (0, 0, 0);
    
    public static Vector3 ModifiedBlockPosition = (0, 0, 0);

    private ShaderProgram _shaderProgram = new ShaderProgram("Entity/Entity.vert", "Entity/Entity.frag");
    int playerModelLocation;
    int playerViewLocation;
    int playerProjectionLocation;
    
    private ShaderProgram _hitShader = new ShaderProgram("Info/Hit.vert", "Info/Hit.frag");
    private VAO _hitVao = new VAO();
    int hitModelLocation;
    int hitViewLocation;
    int hitProjectionLocation;
    int hitFaceDataLocation;

    private ShaderProgram _crosshairShader = new ShaderProgram("Utils/Rectangle.vert", "Inventory/Player/Crosshair.frag");
    private VAO _crosshairVao = new VAO();
    int crosshairModelLocation;
    int crosshairProjectionLocation;
    int crosshairSizeLocation;
    
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
    
    public float yaw;
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
    
    private Vector3 _lastCameraPosition;
    private float _lastCameraYaw;
    private float _lastCameraPitch;
    private Vector3 _oldCameraCenter = Vector3.Zero;
    public float CameraDistance = 5;
    public float _targetDistance = 5;
    public const float SCROLL_SENSITIVITY = 0.4f;
    public const float CAMERA_FOLLOW_SPEED = 50f;
    
    private bool _renderPlayer = true;

    private Action _renderHit = () => {};

    public bool BlockSwitch = false;

    public PlayerStateMachine()
    {
        Instance = this;
    }
    
    void Start()
    {
        new OldAnimationManager();
        
        _lastCameraPitch = Game.Camera.Pitch;
        _lastCameraYaw = Game.Camera.Yaw;
        _lastCameraPosition = Game.Camera.Position;
        
        physicsBody = Transform.GetComponent<PhysicsBody>();
        PlayerData.PhysicsBody = physicsBody; 
        
        _currentState = _gameState;
        _currentState.Enter(this);
        
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

        _shaderProgram.Bind();

        playerModelLocation = _shaderProgram.GetLocation("model");
        playerViewLocation = _shaderProgram.GetLocation("view");
        playerProjectionLocation = _shaderProgram.GetLocation("projection");

        _shaderProgram.Unbind();

        _hitShader.Bind();

        hitModelLocation = _hitShader.GetLocation("model");
        hitViewLocation = _hitShader.GetLocation("view");
        hitProjectionLocation = _hitShader.GetLocation("projection");
        hitFaceDataLocation = _hitShader.GetLocation("faceData");

        _hitShader.Unbind();

        _crosshairShader.Bind();

        crosshairModelLocation = _crosshairShader.GetLocation("model");
        crosshairProjectionLocation = _crosshairShader.GetLocation("projection");
        crosshairSizeLocation = _crosshairShader.GetLocation("size");

        _crosshairShader.Unbind();
    }

    void Resize()
    {
        
    }

    void Awake()
    {
        Transform.Position = new Vector3(0, 200, 0);
        physicsBody.SetPosition(Transform.Position);
        _oldCameraCenter = Transform.Position;

        Camera camera = Game.Camera;

        camera.SetCameraMode(cameraMode);
        
        camera.Position = _lastCameraPosition;
        camera.Yaw = _lastCameraYaw;
        camera.Pitch = _lastCameraPitch;
        
        camera.UpdateVectors();
    }

    void Update()
    {      
        Camera camera = Game.Camera;

        if (Input.IsKeyPressed(Keys.F5))
        {
            ToggleView();
        }

        if (!PlayerData.TestInputs || camera.GetCameraMode() == CameraMode.Free)
            return;

        if (Input.IsKeyDown(Keys.J))
            physicsBody.Acceleration = (0.001f, 0, 0);

        Vector2 input = Input.GetMovementInput();

        if (input != Vector2.Zero)
        {
            yaw = -camera.Yaw + _inputAngle[input];
        }
        
        if (physicsBody.Velocity != Vector3.Zero)
        {
            Info.SetPositionText(Transform.Position - (0f, 0.875f, 0f));
        }
        
        forward = Mathf.YAngleToDirection(-yaw);

        PlayerData.LookingAtBlock = false;
        PlayerData.LookingAtBlockPosition = Vector3i.Zero;
        PlayerData.LookingAtBlockPlacementPosition = Vector3i.Zero;

        if (VoxelData.Raycast(camera.Center, camera.front, 4, out Hit hit))
        {
            Vector3i blockPos = hit.BlockPosition;
            Vector3i n = hit.Normal;
            int index = n.X != 0 ? (n.X == 1 ? 1 : 3) : (n.Y != 0 ? (n.Y == 1 ? 2 : 4) : n.Z == 1 ? 5 : 0);
            _renderHit = () => RenderHit((blockPos.X, blockPos.Y, blockPos.Z, index));

            PlayerData.LookingAtBlock = true;
            PlayerData.LookingAtBlockPosition = blockPos;
            PlayerData.LookingAtBlockPlacementPosition = blockPos + n;
        }

        camera.Center = Mathf.Lerp(_oldCameraCenter, Transform.Position + (0, 0.85f, 0), GameTime.PhysicsDelta);
        
        if (camera.GetCameraMode() == CameraMode.Follow)
        {
            if (Input.IsKeyDown(Keys.LeftControl))
            {
                float scroll = Input.GetMouseScrollDelta().Y;
                
                CameraDistance -= scroll * SCROLL_SENSITIVITY;
                CameraDistance = Math.Clamp(CameraDistance, 3, 10);
            }

            Vector3 _targetPosition = camera.Center - camera.front * CameraDistance;
            float delta = CAMERA_FOLLOW_SPEED * GameTime.DeltaTime;

            camera.Position = Vector3.Lerp(camera.Position, _targetPosition, delta);
        }

        _currentState.Update(this);

        _oldPosition = Transform.Position;
        PlayerData.Position = Transform.Position;
    }
    
    void FixedUpdate()
    {
        _oldCameraCenter = Transform.Position + (0, 0.85f, 0);
        if (!PlayerData.UpdatePhysics || Game.Camera.GetCameraMode() == CameraMode.Free || !Game.MoveTest)
            return;
        
        _currentState.FixedUpdate(this);
    }

    void Render()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);

        Matrix4 model;
        Matrix4 projection;

        if (_renderPlayer)
        {
            Camera camera = Game.Camera;

            _shaderProgram.Bind();
            
            model = Matrix4.CreateTranslation(Transform.Position);
            Matrix4 view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix();
            
            GL.UniformMatrix4(playerModelLocation, true, ref model);
            GL.UniformMatrix4(playerViewLocation, true, ref view);
            GL.UniformMatrix4(playerProjectionLocation, true, ref projection);

            _playerMesh.RenderMesh();
            
            _shaderProgram.Unbind();
        }

        _renderHit();
        
        GL.DepthFunc(DepthFunction.Always);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        model = Matrix4.CreateTranslation(Game.CenterX - 5, Game.CenterY - 5, 0);
        projection = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -1, 1);

        _crosshairShader.Bind();

        GL.UniformMatrix4(crosshairModelLocation, true, ref model);
        GL.UniformMatrix4(crosshairProjectionLocation, true, ref projection);
        GL.Uniform2(crosshairSizeLocation, new Vector2(10, 10));

        _crosshairVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _crosshairVao.Unbind();
        _crosshairShader.Unbind();

        GL.DepthFunc(DepthFunction.Less);
    }

    private void RenderHit(Vector4i data)
    {
        _hitShader.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Game.Camera.ViewMatrix;
        Matrix4 projection = Game.Camera.ProjectionMatrix;

        GL.UniformMatrix4(hitModelLocation, true, ref model);
        GL.UniformMatrix4(hitViewLocation, true, ref view);
        GL.UniformMatrix4(hitProjectionLocation, true, ref projection);
        GL.Uniform4(hitFaceDataLocation, data);

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

    void Exit()
    {
        Camera camera = Game.Camera;

        Console.WriteLine("Exiting Player State Machine");
        
        _lastCameraPosition = camera.Position;
        _lastCameraYaw = camera.Yaw;
        _lastCameraPitch = camera.Pitch;
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
        Camera camera = Game.Camera;
        
        Vector3 direction = camera.FrontYto0() * input.Y - camera.RightYto0() * input.X;
        Vector3 horizontalVelocity = physicsBody.GetHorizontalVelocity();
        MovePlayer(playerMovementSpeed, direction);
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector3 direction)
    {
        direction = Mathf.Normalize(direction);
        physicsBody.AddForce(direction, Speeds[playerMovementSpeed]);
    }

    public void ToggleView()
    {
        Game.Camera.SetCameraMode(Game.Camera.GetCameraMode() == CameraMode.Centered ? CameraMode.Follow : CameraMode.Centered);
        cameraMode = Game.Camera.GetCameraMode();
        _renderPlayer = cameraMode == CameraMode.Follow;
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