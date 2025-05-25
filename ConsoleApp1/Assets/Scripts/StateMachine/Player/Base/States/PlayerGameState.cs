using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGameState : PlayerBaseState
{
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

    private PlayerGameBaseState _currentState;

    public PlayerGameBaseState NextMovingState;
    public PlayerMovementSpeed MovementSpeed;

    public PlayerGameState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    {
        _currentState = IdleState;
        NextMovingState = IdleState;
    }
    
    public override void Enter()
    {
        Console.WriteLine("Entering game state");
        
        _currentState = FallingState;
        _currentState.Enter();
    }

    public override void Update()
    {
        _currentState.Update();
    }
    
    public override void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }

    public override void Exit()
    {

    }
    
    public void MovePlayer(Vector2 input, PlayerMovementSpeed movementSpeed)
    {
        //PlayerStateMachine.MovePlayer(input, movementSpeed);
    }
    
    public void SwitchState(PlayerGameBaseState newState)
    {
        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}