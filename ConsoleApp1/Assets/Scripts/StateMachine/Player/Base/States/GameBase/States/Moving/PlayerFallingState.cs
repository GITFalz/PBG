using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector2 input = Vector2.Zero;
    
    Vector3 right;
    Vector3 forward;
    Vector3 direction;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering falling state");
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        
        Camera = Game.camera;
        
        OldAnimationManager.Instance.LoopAnimation("Player", "falling");
        
        right = Vector3.Zero;
        forward = Vector3.Zero;
        direction = playerGameState.PlayerStateMachine.physicsBody.GetHorizontalVelocity();
        
        playerGameState.PlayerStateMachine.physicsBody.AddForce(direction);
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
        if (input.X != 0)
            right = -Camera.RightYto0() * input.X;
        if (input.Y != 0)
            forward = Camera.FrontYto0() * input.Y;
    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}