using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerJumpingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        playerGameState.PlayerStateMachine.physicsBody.AddForce(new Vector3(0, 30, 0));
    }

    public override void Update(PlayerGameState playerGameState)
    {
        if (playerGameState.PlayerStateMachine.IsHuggingWall() && InputManager.IsKeyPressed(Keys.Space))
        {
            playerGameState.PlayerStateMachine.physicsBody.Velocity.Y = 0;
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
        {
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
        
        playerGameState.PlayerStateMachine.MeshUpdate();
        return;
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {

    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}