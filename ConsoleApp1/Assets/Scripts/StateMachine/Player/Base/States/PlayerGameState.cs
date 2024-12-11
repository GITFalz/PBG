using OpenTK.Mathematics;

public class PlayerGameState : PlayerBaseState
{
    public PlayerStateMachine PlayerStateMachine;
    private PlayerGameBaseState _currentState;
    
    public PlayerAdminState AdminState = new();
    public PlayerFallingState FallingState = new();
    public PlayerIdleState IdleState = new();
    public PlayerWalkingState WalkingState = new();
    
    public override void Enter(PlayerStateMachine playerStateMachine)
    {
        Console.WriteLine("Entering game state");
        
        PlayerStateMachine = playerStateMachine;
        
        _currentState = FallingState;
        _currentState.Enter(this);
    }

    public override void Update(PlayerStateMachine playerStateMachine)
    {
        _currentState.Update(this);
    }

    public override void Exit(PlayerStateMachine playerStateMachine)
    {

    }

    public void ApplyGravity()
    {
        int result = PlayerStateMachine.ApplyGravity();

        if (result == 0)
        {
            SwitchState(IdleState);
        }
    }
    
    public void MovePlayer(Vector2 input)
    {
        PlayerStateMachine.MovePlayer(input);
    }
    
    public void SwitchState(PlayerGameBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}