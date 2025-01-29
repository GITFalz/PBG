using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGrapplingState : PlayerGameBaseState
{
    Camera Camera;
    
    float timer = 0;
    Vector3 grappleDirection;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering grappling state");
        
        Camera = Game.camera;
        
        Camera.SetFOV(70);
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = false;
        
        timer = 0;
        grappleDirection = Camera.Front();
        
        OldAnimationManager.Instance.SetAnimation("Player", "grappleIn");
        OldAnimationManager.Instance.QueueLoopAnimation("Player", "grapple");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        timer += GameTime.DeltaTime;
        
        if (Input.IsKeyPressed(Keys.LeftShift))
        {
            playerGameState.SwitchState(playerGameState.GrapplingSwingOutState);
            return;
        }

        if (timer > 1f)
        {
            OldAnimationManager.Instance.SetAnimation("Player", "grappleOut");
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