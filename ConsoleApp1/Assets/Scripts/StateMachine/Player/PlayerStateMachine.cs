using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerStateMachine : ScriptingNode
{
    public static PlayerStateMachine Instance = null!;
    public CameraMode CameraMode = CameraMode.Follow;
    public const float WALK_SPEED = 20;
    public const float RUN_SPEED = 60;
    public const float DASH_SPEED = 2000;
    public const float SPRINT_SPEED = 120;
    public const float FALL_SPEED = 15;
    public const float GRAPPLE_SPEED = 200;
    public const float JUMP_SPEED = 900;
    
    public Vector3 forward = (0, 0, -1);
    private Vector3 _oldPosition = (0, 0, 0);
    
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
    
    private PlayerGameState _gameState;
    private PlayerMenuState _menuState;
    
    // Animation
    public PhysicsBody physicsBody;
    public Model? PlayerModel;
    
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
    private Vector3 _CameraOffset = (0, 0.85f, 0);
    public const float SCROLL_SENSITIVITY = 0.4f;
    public const float CAMERA_FOLLOW_SPEED = 50f;
    
    private bool _renderPlayer = true;

    private Action _renderHit = () => {};

    public bool BlockSwitch = false;

    private float _yaw = 0;

    public PlayerStateMachine()
    {
        Instance = this;
        _gameState = new PlayerGameState(this);
        _menuState = new PlayerMenuState(this);
        _currentState = _gameState;
    }
    
    void Start()
    {
        new OldAnimationManager();
        
        _lastCameraPitch = Camera.Pitch;
        _lastCameraYaw = Camera.Yaw;
        _lastCameraPosition = Camera.Position;
        
        physicsBody = Transform.GetComponent<PhysicsBody>();
        PlayerData.PhysicsBody = physicsBody; 
        
        _currentState.Enter();
        
        Transform.Position = new Vector3(0, 200, 0);
        physicsBody.SetPosition(Transform.Position);

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

        Camera.SetCameraMode(CameraMode);

        Camera.Position = _lastCameraPosition;
        Camera.Yaw = _lastCameraYaw;
        Camera.Pitch = _lastCameraPitch;

        Camera.UpdateVectors();

        if (PlayerModel == null)
        {
            Model? model = GameData.GetModel("Player");
            Rig? rig = GameData.GetRig("PlayerRig");
            (string, Animation?) walking = ("PlayerWalking", GameData.GetAnimation("PlayerWalking"));
            (string, Animation?) running = ("PlayerRunning", GameData.GetAnimation("PlayerRunning"));
            (string, Animation?) dash = ("PlayerDash", GameData.GetAnimation("PlayerDash"));
            (string, Animation?) idle = ("PlayerIdle", GameData.GetAnimation("PlayerIdle"));
            (string, Animation?) fall = ("PlayerFall", GameData.GetAnimation("PlayerFall"));
            (string, Animation?) land = ("PlayerLand", GameData.GetAnimation("PlayerLand"));

            if (model == null)
            {
                PopUp.AddPopUp("Player model not found");
                return;
            }

            if (rig == null)
            {
                PopUp.AddPopUp("Player rig not found");
                return;
            }

            model.AnimationManager = new ModelAnimationManager(rig);
            model.BindRig();

            model.AnimationManager.AddAnimations(rig, walking, running, dash, idle, fall, land);

            PlayerModel = model;
            PlayerModel.Animate = true;
        }
    }

    void Update()
    {
        if (Input.IsKeyPressed(Keys.F5))
        {
            ToggleView();
        }

        if (!PlayerData.TestInputs || Camera.GetCameraMode() == CameraMode.Free)
            return;

        _currentState.Update();

        if (MovementInput != Vector2.Zero)
        {
            yaw = -Camera.Yaw + _inputAngle[MovementInput];
            float delta = Mathf.DeltaAngle(_yaw, yaw);
            _yaw += delta * GameTime.DeltaTime * 10;
        }

        if (physicsBody.Velocity != Vector3.Zero)
        {
            Info.SetPositionText(Transform.Position);
        }

        forward = Mathf.YAngleToDirection(-yaw);

        if (VoxelData.Raycast(Camera.Center, Camera.front, 4, out Hit hit))
        {
            Vector3i blockPos = hit.BlockPosition;
            Vector3i n = hit.Normal;
            int index = n.X != 0 ? (n.X == 1 ? 1 : 3) : (n.Y != 0 ? (n.Y == 1 ? 2 : 4) : n.Z == 1 ? 5 : 0);
            _renderHit = () => RenderHit((blockPos.X, blockPos.Y, blockPos.Z, index));

            PlayerData.LookingAtBlock = true;
            PlayerData.LookingAtBlockPosition = blockPos;
            PlayerData.LookingAtBlockPlacementPosition = blockPos + n;
        }
        else if (PlayerData.LookingAtBlock)
        {
            PlayerData.LookingAtBlock = false;
            PlayerData.LookingAtBlockPosition = Vector3i.Zero;
            PlayerData.LookingAtBlockPlacementPosition = Vector3i.Zero;
            _renderHit = () => {};
        }

        Camera.Center = Mathf.Lerp(_oldCameraCenter, Transform.Position + _CameraOffset, GameTime.PhysicsDelta);

        if (Camera.GetCameraMode() == CameraMode.Follow)
        {
            if (Input.IsKeyDown(Keys.LeftControl))
            {
                float scroll = Input.GetMouseScrollDelta().Y;

                CameraDistance -= scroll * SCROLL_SENSITIVITY;
                CameraDistance = Math.Clamp(CameraDistance, 3, 10);
            }

            Vector3 _targetPosition = Camera.Center - Camera.front * CameraDistance;
            float delta = CAMERA_FOLLOW_SPEED * GameTime.DeltaTime;

            Camera.Position = Vector3.Lerp(Camera.Position, _targetPosition, delta);
        }

        _oldPosition = Transform.Position;
        PlayerData.Position = Transform.Position;
        if (PlayerModel != null)
        {
            PlayerModel.Position = Transform.Position;
            PlayerModel.Rotation = (0, _yaw + 90, 0);
        }

        PlayerModel?.Update();
    }
    
    void FixedUpdate()
    {
        _oldCameraCenter = Transform.Position + _CameraOffset;
        if (!PlayerData.UpdatePhysics || Camera.GetCameraMode() == CameraMode.Free || !Game.MoveTest)
            return;
        
        _currentState.FixedUpdate();
    }

    void Render()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);

        _renderHit();

        GL.DepthFunc(DepthFunction.Always);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Matrix4 model = Matrix4.CreateTranslation(Game.CenterX - 5, Game.CenterY - 5, 0);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -1, 1);

        _crosshairShader.Bind();

        GL.UniformMatrix4(crosshairModelLocation, true, ref model);
        GL.UniformMatrix4(crosshairProjectionLocation, true, ref projection);
        GL.Uniform2(crosshairSizeLocation, new Vector2(10, 10));

        _crosshairVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _crosshairVao.Unbind();
        _crosshairShader.Unbind();
        
        PlayerModel?.Render();
    }

    private void RenderHit(Vector4i data)
    {
        _hitShader.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Camera.ViewMatrix;
        Matrix4 projection = Camera.ProjectionMatrix;

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
        Console.WriteLine("Exiting Player State Machine");
        
        _lastCameraPosition = Camera.Position;
        _lastCameraYaw = Camera.Yaw;
        _lastCameraPitch = Camera.Pitch;
    }

    public bool IsGrounded()
    {
        return physicsBody.IsGrounded;
    }
    
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed)
    {
        MovePlayer(playerMovementSpeed, MovementInput);
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector2 input)
    {
        if (input == Vector2.Zero)
            return;

        Vector3 direction = Camera.FrontYto0() * input.Y - Camera.RightYto0() * input.X;
        MovePlayer(playerMovementSpeed, direction);
    }
    
    public void MovePlayer(PlayerMovementSpeed playerMovementSpeed, Vector3 direction)
    {
        direction = Mathf.Normalize(direction);
        physicsBody.AddForce(direction, Speeds[playerMovementSpeed]);
    }

    public void ToggleView()
    {
        if (CameraMode == CameraMode.Follow)
        {
            ToggleView(CameraMode.Centered);
        }
        else
        {
            ToggleView(CameraMode.Follow);
        }
    }

    public void ToggleView(CameraMode CameraMode)
    {
        Camera.SetCameraMode(CameraMode);
        this.CameraMode = CameraMode;
        _renderPlayer = CameraMode == CameraMode.Follow;
        _CameraOffset = CameraMode == CameraMode.Follow ? (0, 0.85f, 0) : (0, 1.85f, 0);
        if (PlayerModel != null)
            PlayerModel.IsShown = _renderPlayer;
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