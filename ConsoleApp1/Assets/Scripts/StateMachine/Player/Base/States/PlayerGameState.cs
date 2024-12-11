public class PlayerGameState : PlayerBaseState
{
    private PlayerGameBaseState _currentState;
    
    private PlayerAdminState _adminState = new();
    
    public override void Enter(PlayerStateMachine playerStateMachine)
    {
        Console.WriteLine("Entering game state");
        
        _currentState = _adminState;
        _currentState.Enter(this);
    }

    public override void Update(PlayerStateMachine playerStateMachine)
    {
        
    }

    public override void Exit(PlayerStateMachine playerStateMachine)
    {

    }
    
    public void SwitchState(PlayerGameBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}