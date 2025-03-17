using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerJumpingState : PlayerGameBaseState
{
    Camera Camera;
    Vector2 input = Vector2.Zero;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        
        playerGameState.PlayerStateMachine.physicsBody.Drag = 2;
        Camera = Game.camera;
        
        playerGameState.PlayerStateMachine.physicsBody.AddForce(new Vector3(0, PlayerStateMachine.JUMP_SPEED, 0));
        
        OldAnimationManager.Instance.SetAnimation("Player", "jumping");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();

        if (Input.IsKeyPressed(Keys.F))
        {
            playerGameState.SwitchState(playerGameState.GrapplingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
        {
            Camera.SetFOV(45);
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.IsGrounded() && playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
        {
            Camera.SetFOV(45);
            playerGameState.SwitchState(playerGameState.GroundedState);
            return;
        }
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        if (input != Vector2.Zero)
            playerGameState.PlayerStateMachine.MovePlayer(playerGameState.MovementSpeed);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.Drag = 0.1f;
    }
}