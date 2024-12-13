public class PlayerFallingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        int result = playerGameState.PlayerStateMachine.CanUpdatePhysics();
        playerGameState.PlayerStateMachine.physicsBody.doGravity = result == 1;
        
        if (result == 0)
        {
            playerGameState.SwitchState(playerGameState.GroundedState);
            return;
        }
        if (result == 1)
        {
            playerGameState.PlayerStateMachine.GravityUpdate();
            return;
        }
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}