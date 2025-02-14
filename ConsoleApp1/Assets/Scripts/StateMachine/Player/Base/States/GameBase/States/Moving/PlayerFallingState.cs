using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector2 input = Vector2.Zero;
    
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
        playerGameState.PlayerStateMachine.physicsBody.EnableGravity();
        
        Camera = Game.camera;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();
        
        if (playerGameState.PlayerStateMachine.IsHuggingWall() && Input.IsKeyPressed(Keys.Space))
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

    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}