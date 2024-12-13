using OpenTK.Mathematics;

public class PlayerJumpingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = 
            playerGameState.PlayerStateMachine.CanUpdatePhysics() == 1;
        
        playerGameState.PlayerStateMachine.physicsBody.AddForce(new Vector3(0, 60, 0));
    }

    public override void Update(PlayerGameState playerGameState)
    {
        int result = playerGameState.PlayerStateMachine.CanUpdatePhysics();
        playerGameState.PlayerStateMachine.physicsBody.doGravity = result == 1;
        
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
        {
            playerGameState.SwitchState(playerGameState.FallingState);
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