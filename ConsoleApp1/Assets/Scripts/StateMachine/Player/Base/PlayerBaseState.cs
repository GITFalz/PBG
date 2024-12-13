public abstract class PlayerBaseState
{
    public abstract void Enter(PlayerStateMachine playerStateMachine);
    public abstract void Update(PlayerStateMachine playerStateMachine);
    public abstract void FixedUpdate(PlayerStateMachine playerStateMachine);
    public abstract void Exit(PlayerStateMachine playerStateMachine);
}