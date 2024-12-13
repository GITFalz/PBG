public class PlayerFallingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        playerGameState.ApplyGravity();
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}