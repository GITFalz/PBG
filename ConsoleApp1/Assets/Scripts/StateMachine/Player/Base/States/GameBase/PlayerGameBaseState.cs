

using OpenTK.Mathematics;

public abstract class PlayerGameBaseState
{
    public PlayerGameState GameState;
    public PlayerStateMachine StateMachine => GameState.StateMachine;
    public static Vector2 MovementInput => Input.MovementInput;

    public void Play(string name, float blendTime = 0f) => StateMachine.PlayerModel?.AnimationManager?.Play(name, blendTime);
    public void Loop(string name, float blendTime = 0f) => StateMachine.PlayerModel?.AnimationManager?.Loop(name, blendTime);
    public void PlayQueued(string name, float blendTime = 0f) => StateMachine.PlayerModel?.AnimationManager?.PlayQueued(name, blendTime);
    public void ForceFinish() => StateMachine.PlayerModel?.AnimationManager?.ForceFinish();
    public void Stop() => StateMachine.PlayerModel?.AnimationManager?.Stop();
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