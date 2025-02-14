using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGameState : PlayerBaseState
{
    public PlayerStateMachine PlayerStateMachine;
    private PlayerGameBaseState _currentState;
    
    public PlayerAdminState AdminState = new();
    public PlayerFallingState FallingState = new();
    public PlayerGroundedState GroundedState = new();
    public PlayerIdleState IdleState = new();
    public PlayerWalkingState WalkingState = new();
    public PlayerRunningState RunningState = new();
    public PlayerSprintingState SprintingState = new();
    public PlayerJumpingState JumpingState = new();
    public PlayerAttack1State Attack1State = new();
    public PlayerAttack2State Attack2State = new();
    public PlayerAttack3State Attack3State = new();
    public PlayerAttack4State Attack4State = new();
    public PlayerDashState DashState = new();
    public PlayerGrapplingState GrapplingState = new();
    public PlayerGrapplingSwingOutState GrapplingSwingOutState = new();

    public PlayerGameBaseState NextMovingState;

    public void Start()
    {
        InputManager.SetAction([Keys.Space], () => SwitchState(JumpingState));
        InputManager.SetAction([MouseButton.Left], () => SwitchState(Attack1State));
    }
    
    public override void Enter(PlayerStateMachine playerStateMachine)
    {
        Console.WriteLine("Entering game state");
        
        NextMovingState = WalkingState;
        
        PlayerStateMachine = playerStateMachine;
        
        _currentState = FallingState;
        _currentState.Enter(this);
    }

    public override void Update(PlayerStateMachine playerStateMachine)
    {
        _currentState.Update(this);
    }
    
    public override void FixedUpdate(PlayerStateMachine playerStateMachine)
    {
        _currentState.FixedUpdate(this);
    }

    public override void Exit(PlayerStateMachine playerStateMachine)
    {

    }
    
    public void MovePlayer(Vector2 input, PlayerMovementSpeed movementSpeed)
    {
        //PlayerStateMachine.MovePlayer(input, movementSpeed);
    }
    
    public void SwitchState(PlayerGameBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}