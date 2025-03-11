public class PlayerGroundedState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grounded state");
        
        playerGameState.SwitchState(playerGameState.NextMovingState);
    }

    public override void Update(PlayerGameState playerGameState)
    {

    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {

    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}