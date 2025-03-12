using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Camera Camera;
    Vector2 input = Vector2.Zero;
    double timer = 0;
    
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");

        timer = 0;
        
        Camera = Game.camera;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();

        if (timer > 1f)
        {
            playerGameState.MovementSpeed = PlayerMovementSpeed.Walk;
        }
        
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
        
        timer += GameTime.DeltaTime;
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        if (input != Vector2.Zero)
            playerGameState.PlayerStateMachine.MovePlayer(playerGameState.MovementSpeed);
    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}