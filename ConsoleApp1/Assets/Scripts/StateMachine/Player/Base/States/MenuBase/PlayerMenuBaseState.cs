public abstract class PlayerMenuBaseState
{
    public PlayerMenuState MenuState;
    public PlayerStateMachine StateMachine => MenuState.StateMachine;

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}