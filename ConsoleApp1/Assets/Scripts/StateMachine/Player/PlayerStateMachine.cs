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
    public const float FALL_SPEED = 5f;
    
    public static readonly Dictionary<PlayerMovementSpeed, float> Speeds = new Dictionary<PlayerMovementSpeed, float>()
    {
        {PlayerMovementSpeed.Walk, WALK_SPEED},
        {PlayerMovementSpeed.Sprint, SPRINT_SPEED},
        {PlayerMovementSpeed.Fall, FALL_SPEED},
    };
    
    private PlayerBaseState _currentState;
    
    private PlayerGameState _gameState = new();
    private PlayerMenuState _menuState = new();
    
    private EntityMesh _mesh;
    private AnimationMesh _swordMesh;
    
    private ShaderProgram _shaderProgram;
    
    public WorldManager WorldManager;
    
    public PhysicsBody physicsBody;

    public float yaw;

    public float angle;

    public bool isGrounded;
    
    
    //Random values
    Vector3i HugZ = Vector3i.Zero;
    Vector3i HugX = Vector3i.Zero;
    
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
        
        _swordMesh = new AnimationMesh();
        VoxelData.GenerateStandardMeshBox(_swordMesh, 2);
        VoxelData.GenerateStandardMeshBox(_swordMesh, 2);
        _swordMesh.Sizes.Add(new Vector3(0.1f, 0.3f, 0.1f));
        _swordMesh.Sizes.Add(new Vector3(0.5f, 2f, 0.1f));
        
        _swordMesh.Position.Add(new Vector3(0, 100, 0));
        _swordMesh.Position.Add(new Vector3(-0.2f, 100.3f, 0));
        
        _swordMesh.GenerateBuffers();
        
        _swordMesh.UpdateScale();
        _swordMesh.UpdatePosition();
        
        _swordMesh.UpdateMesh();
        
        //Debug.Start();
    }

    public override void Update(FrameEventArgs args)
    {
        if (InputManager.IsKeyPressed(Keys.M))
            physicsBody.doGravity = !physicsBody.doGravity;

        _currentState.Update(this);
        PlayerData.Position = transform.Position + new Vector3(0, 1.8f, 0);
        
        _swordMesh.UpdateRotation(new Vector3(0.05f, 100, 0.05f), new Vector3(1, 0, 0), angle);
        _swordMesh.UpdateMesh();
        
        angle += 10 * GameTime.DeltaTime;
        
        IsHuggingWall();
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
        _swordMesh.RenderMesh();
        
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
        
        yaw = -Camera.GetYaw();
        
        _mesh.UpdatePosition();
        _mesh.UpdateRotation(_mesh.Position + new Vector3(0.5f, 0, 0.5f), new Vector3(0, 1, 0), yaw);
        _mesh.UpdateMesh();
    }

    public void SnapToBlockUnder()
    {
        transform.Position.Y = Mathf.RoundToInt(transform.Position.Y);
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
        
        Vector3 direction = Camera.FrontYto0() * input.Y - Camera.RightYto0() * input.X;
        Vector3 oldVelocity = physicsBody.GetHorizontalVelocity();
        
        direction = Mathf.Normalize(direction);
        
        physicsBody.AddForce(direction, Speeds[playerMovementSpeed]);
        physicsBody.Velocity -= oldVelocity;
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
}

public enum PlayerMovementSpeed
{
    Walk,
    Sprint,
    Fall,
}