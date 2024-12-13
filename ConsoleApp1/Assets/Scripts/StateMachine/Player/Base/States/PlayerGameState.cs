using OpenTK.Mathematics;

public class PlayerGameState : PlayerBaseState
{
    public PlayerStateMachine PlayerStateMachine;
    private PlayerGameBaseState _currentState;
    
    public PlayerAdminState AdminState = new();
    public PlayerFallingState FallingState = new();
    public PlayerGroundedState GroundedState = new();
    public PlayerIdleState IdleState = new();
    public PlayerWalkingState WalkingState = new();
    public PlayerSprintingState SprintingState = new();
    public PlayerJumpingState JumpingState = new();

    public PlayerGameBaseState NextMovingState;
    
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

    public void CalculateGravity()
    {
        PlayerStateMachine.CalculateGravity();
    }
    
    public void GravityUpdate()
    {
        int result = PlayerStateMachine.GravityUpdate();

        if (result == 0)
        {
            SwitchState(GroundedState);
            return;
        }
    }
    
    public void MovePlayer(Vector2 input, PlayerMovementSpeed movementSpeed)
    {
        PlayerStateMachine.MovePlayer(input, movementSpeed);
    }
    
    public void SwitchState(PlayerGameBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}