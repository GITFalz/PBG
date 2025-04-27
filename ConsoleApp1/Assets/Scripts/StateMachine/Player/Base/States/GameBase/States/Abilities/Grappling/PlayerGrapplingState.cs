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
        
        Camera = Game.Camera;
        
        Camera.SetFOV(90);
        
        playerGameState.PlayerStateMachine.physicsBody.DisableGravity();
        
        timer = 0;
        grappleDirection = Camera.Front();
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
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Grappling, grappleDirection);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.EnableGravity();
    }
}