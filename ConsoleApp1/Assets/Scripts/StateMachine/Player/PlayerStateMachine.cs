using ConsoleApp1.Engine.Scripts.Core;

public class PlayerStateMachine : Updateable
{
    private PlayerBaseState _currentState;
    
    private PlayerGameState _gameState = new();
    private PlayerMenuState _menuState = new();
    
    public override void Start()
    {
        _currentState = _gameState;
        _currentState.Enter(this);
    }

    public override void Update()
    {
        _currentState.Update(this);
    }
    
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}