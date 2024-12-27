using OpenTK.Mathematics;

public class PlayerGrapplingSwingOutState : PlayerGameBaseState
{
    Vector3 direction;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grappling swing out state");
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        playerGameState.PlayerStateMachine.physicsBody.gravity = 20f;
        
        AnimationManager.Instance.SetAnimation("Player", "grappleSwingOut");
        AnimationManager.Instance.QueueAnimation("Player", "grappleSwingEnd");

        direction = Camera.FrontYto0();
    }

    public override void Update(PlayerGameState playerGameState)
    {
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < -2f)
        {
            Camera.SetFOV(45);
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
        Vector3 oldVelocity = playerGameState.PlayerStateMachine.physicsBody.GetHorizontalVelocity();
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Grappling, direction);
        playerGameState.PlayerStateMachine.physicsBody.Velocity -= oldVelocity;
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.gravity = 50f;
    }
}