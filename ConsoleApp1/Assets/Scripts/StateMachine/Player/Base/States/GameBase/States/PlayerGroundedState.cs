public class PlayerGroundedState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grounded state");
        
        playerGameState.PlayerStateMachine.physicsBody.DisableGravity();
        playerGameState.PlayerStateMachine.physicsBody.Velocity.Y = 0;
        
        playerGameState.PlayerStateMachine.SnapToBlockUnder();
        
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