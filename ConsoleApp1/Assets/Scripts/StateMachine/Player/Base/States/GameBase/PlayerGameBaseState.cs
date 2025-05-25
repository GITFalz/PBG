public abstract class PlayerGameBaseState
{
    public PlayerGameState GameState;
    public PlayerStateMachine StateMachine => GameState.StateMachine;

    public void Loop(string name) => StateMachine.PlayerModel?.AnimationManager?.Loop(name);
    public void LoopAfter(string name) => StateMachine.PlayerModel?.AnimationManager?.LoopAfter(name);
    public void SmoothLoop(string name, float time) => StateMachine.PlayerModel?.AnimationManager?.SmoothLoop(name, time);
    public void Play(string name) => StateMachine.PlayerModel?.AnimationManager?.Play(name);
    public void PlayAfter(string name) => StateMachine.PlayerModel?.AnimationManager?.PlayAfter(name);
    public void Stop() => StateMachine.PlayerModel?.AnimationManager?.Stop();
    public void StopAfter() => StateMachine.PlayerModel?.AnimationManager?.StopAfter(); 
    public void SmoothPlayFinish(string name, float time) => StateMachine.PlayerModel?.AnimationManager?.SmoothPlayFinish(name, time);
    public void SetSpeed(float speed) => StateMachine.PlayerModel?.AnimationManager?.SetSpeed(speed);

    public PlayerGameBaseState(PlayerGameState gameState)
    {
        GameState = gameState;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}