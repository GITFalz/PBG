public class PlayerFallingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        playerGameState.GravityUpdate();
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        playerGameState.CalculateGravity();
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}