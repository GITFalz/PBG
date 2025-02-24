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
    
    public Vector3 forward = new Vector3(0, 0, -1);
    
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
    
    private EntityMesh _mesh;
    
    // Animation
    private OldAnimationMesh _playerMesh;
    private OldAnimationMesh _swordMesh;
    
    private OldAnimationMesh _testMesh;
    
    private ShaderProgram _shaderProgram;
    public PhysicsBody physicsBody;
    
    public OldAnimation CurrentOldAnimation;
    
    public float yaw;
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
    
    private Vector3 _lastCameraPosition;
    private float _lastCameraYaw;
    private float _lastCameraPitch;

    public UIController InfoMenu;
    public UIText fpsText;
    
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
        VoxelData.GetEntityBoxMesh(_mesh, new Vector3(1, 2, 1), new Vector3(0, 0, 0), 0);
        _mesh.GenerateBuffers();
        
        _swordMesh = new OldAnimationMesh();
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
        
        _playerMesh = new OldAnimationMesh();
        VoxelData.GenerateStandardMeshBox(_playerMesh, 
            new Vector3(1, 2, 1), 
            new Vector3(-0.5f, 0, -0.5f), 
            new Vector3(0, 0, 0), 
            1
        );
        
        _testMesh = new OldAnimationMesh();
        VoxelData.GenerateStandardMeshBox(_testMesh, 
            new Vector3(1, 2, 1), 
            new Vector3(-0.5f, 0, -0.5f), 
            new Vector3(0, 0, 0), 
            1
        );
        
        _testMesh.WorldPosition = new Vector3(0, 60, 0);
        
        _testMesh.GenerateBuffers();
        _testMesh.UpdateMesh();
        
        _swordMesh.GenerateBuffers();
        _swordMesh.UpdateMesh();
        
        _playerMesh.GenerateBuffers();
        _playerMesh.UpdateMesh();

        //UI
        InfoMenu = new UIController();

        TextMesh textMesh = InfoMenu.textMesh;

        fpsText = new("FpsTest", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (100, 20), (5, 5, 5, 5), 0, 0, (0, 0), textMesh);
        fpsText.SetMaxCharCount(9).SetText("Fps: 9999", 0.5f);

        InfoMenu.AddElement(fpsText);

        InfoMenu.GenerateBuffers();
    }

    public override void Resize()
    {
        InfoMenu.OnResize();
    }

    public override void Awake()
    {
        Camera camera = Game.camera;
        
        if (WorldManager.Instance != null)
            WorldManager.Instance.camera = camera;
        
        camera.SetCameraMode(CameraMode.Free);
        
        camera.Position = _lastCameraPosition;
        camera.yaw = _lastCameraYaw;
        camera.pitch = _lastCameraPitch;
        
        camera.UpdateVectors();
        
        base.Awake();

        camera.SetCameraMode(CameraMode.Free);
    }

    public override void Update()
    {
        InfoMenu.Test();

        if (Game.Instance.FpsUpdate())
        {
            Console.WriteLine("Fps: " + GameTime.Fps);
            fpsText.SetText($"Fps: {GameTime.Fps}", 0.5f).GenerateChars().UpdateText();
        }

        Camera camera = Game.camera;
        return;

        camera.Center = Transform.Position + new Vector3(0, 1.8f, 0);

        if (Game.MoveTest)
            camera.Update();
        
        Vector2 input = Input.GetMovementInput();
        
        if (input != Vector2.Zero)
            yaw = -camera.yaw + _inputAngle[input];
        
        forward = Mathf.YAngleToDirection(-yaw);
        
        if (!Game.MoveTest)
            return;
        
        _currentState.Update(this);

        IsHuggingWall();
    }
    
    public override void FixedUpdate()
    {
        return;
        if (!Game.MoveTest)
            return;
        
        _currentState.FixedUpdate(this);
    }

    public override void Render()
    {
        Camera camera = Game.camera;

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

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
        
        _swordMesh.RenderMesh();
        _playerMesh.RenderMesh();
        
        _shaderProgram.Unbind();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        InfoMenu.Render();
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
        _mesh.Position = Transform.Position + new Vector3(-0.5f, 0, -0.5f);

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