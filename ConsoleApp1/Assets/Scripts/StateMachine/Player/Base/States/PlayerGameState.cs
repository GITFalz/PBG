using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGameState : PlayerBaseState
{
    public PlayerAdminState AdminState;
    public PlayerFallingState FallingState;
    public PlayerGroundedState GroundedState;
    public PlayerIdleState IdleState;
    public PlayerWalkingState WalkingState;
    public PlayerRunningState RunningState;
    public PlayerSprintingState SprintingState;
    public PlayerJumpingState JumpingState;
    public PlayerAttack1State Attack1State;
    public PlayerAttack2State Attack2State;
    public PlayerAttack3State Attack3State;
    public PlayerAttack4State Attack4State;
    public PlayerDashState DashState;
    public PlayerGrapplingState GrapplingState;
    public PlayerGrapplingSwingOutState GrapplingSwingOutState;

    private PlayerGameBaseState _currentState;

    public PlayerGameBaseState NextMovingState;
    public PlayerMovementSpeed MovementSpeed;

    public PlayerGameState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    {
        AdminState = new PlayerAdminState(this);
        FallingState = new PlayerFallingState(this);
        GroundedState = new PlayerGroundedState(this);
        IdleState = new PlayerIdleState(this);
        WalkingState = new PlayerWalkingState(this);
        RunningState = new PlayerRunningState(this);
        SprintingState = new PlayerSprintingState(this);
        JumpingState = new PlayerJumpingState(this);
        Attack1State = new PlayerAttack1State(this);
        Attack2State = new PlayerAttack2State(this);
        Attack3State = new PlayerAttack3State(this);
        Attack4State = new PlayerAttack4State(this);
        DashState = new PlayerDashState(this);
        GrapplingState = new PlayerGrapplingState(this);
        GrapplingSwingOutState = new PlayerGrapplingSwingOutState(this);
        
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