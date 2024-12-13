public class PlayerJumpingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        playerGameState.PlayerStateMachine.Velocity.Y = 0.05f;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        playerGameState.ApplyGravity();
        
        if (playerGameState.PlayerStateMachine.Velocity.Y < 0)
        {
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}