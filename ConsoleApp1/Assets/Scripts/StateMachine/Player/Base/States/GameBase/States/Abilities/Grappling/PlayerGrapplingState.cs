using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGrapplingState : PlayerGameBaseState
{
    float timer = 0;
    Vector3 grappleDirection;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grappling state");
        
        Camera.SetFOV(70);
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = false;
        
        timer = 0;
        grappleDirection = Camera.Front();
        
        AnimationManager.Instance.SetAnimation("Player", "grappleIn");
        AnimationManager.Instance.QueueLoopAnimation("Player", "grapple");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        timer += GameTime.DeltaTime;
        
        if (InputManager.IsKeyPressed(Keys.LeftShift))
        {
            playerGameState.SwitchState(playerGameState.GrapplingSwingOutState);
            return;
        }

        if (timer > 1f)
        {
            AnimationManager.Instance.SetAnimation("Player", "grappleOut");
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
    }

    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        Vector3 oldVelocity = playerGameState.PlayerStateMachine.physicsBody.Velocity;
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Grappling, grappleDirection);
        Console.WriteLine(grappleDirection);
        playerGameState.PlayerStateMachine.physicsBody.Velocity -= oldVelocity;
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
    }
}