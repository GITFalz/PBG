public abstract class PlayerGameBaseState
{
    public PlayerGameState GameState;
    public PlayerStateMachine StateMachine => GameState.StateMachine;

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}