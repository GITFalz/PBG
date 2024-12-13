public abstract class PlayerGameBaseState
{
    public abstract void Enter(PlayerGameState playerGameState);
    public abstract void Update(PlayerGameState playerGameState);
    public abstract void FixedUpdate(PlayerGameState playerGameState);
    public abstract void Exit(PlayerGameState playerGameState);
}