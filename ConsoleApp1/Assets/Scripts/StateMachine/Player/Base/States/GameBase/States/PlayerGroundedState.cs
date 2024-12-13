public class PlayerGroundedState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.Velocity.Y = 0;
        playerGameState.PlayerStateMachine.SnapToBlockUnder();
        
        playerGameState.SwitchState(playerGameState.IdleState);
    }

    public override void Update(PlayerGameState playerGameState)
    {

    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}