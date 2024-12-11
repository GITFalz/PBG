public class PlayerFallingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.Velocity.X = 0;
        playerGameState.PlayerStateMachine.Velocity.Z = 0;
        
        playerGameState.ApplyGravity();
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}