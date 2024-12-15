using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Vector2 input = Vector2.Zero;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = InputManager.GetMovementInput();
        
        if (playerGameState.PlayerStateMachine.IsHuggingWall() && InputManager.IsKeyPressed(Keys.Space))
        {
            playerGameState.PlayerStateMachine.physicsBody.Velocity.Y = 0;
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.IsGrounded())
        {
            playerGameState.SwitchState(playerGameState.GroundedState);
            return;
        }
        
        playerGameState.PlayerStateMachine.MeshUpdate();
        return;
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Fall);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}