using OpenTK.Mathematics;

public class PlayerGrapplingSwingOutState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector3 direction;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grappling swing out state");
        
        Camera = Game.Camera;

        playerGameState.PlayerStateMachine.physicsBody.EnableGravity();

        direction = Camera.FrontYto0();
    }

    public override void Update(PlayerGameState playerGameState)
    {
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < -2f)
        {
            Camera.SetFOV(60);
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.IsGrounded())
        {
            playerGameState.SwitchState(playerGameState.GroundedState);
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