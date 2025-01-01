using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerDashState : PlayerGameBaseState
{
    private Camera Camera;
    
    Vector2 input;
    float timer = 0;
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering dash state");
        
        Camera = playerGameState.PlayerStateMachine.camera;
        
        timer = 0;

        Vector3 forward = playerGameState.PlayerStateMachine.forward;
        playerGameState.PlayerStateMachine.physicsBody.AddForce(forward * 0.5f);
        
        AnimationManager.Instance.SetAnimation("Player", "dashing");
        AnimationManager.Instance.QueueLoopAnimation("Player", "running");

        Camera.SetFOV(65);
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();
        timer += GameTime.DeltaTime;
        
        if (timer > 0.5f)
        {
            if (input == Vector2.Zero)
            {
                Camera.SetFOV(45);
                playerGameState.SwitchState(playerGameState.IdleState);
                return;
            }
            else
            {
                Camera.SetFOV(45);
                playerGameState.SwitchState(playerGameState.NextMovingState);
                return;
            }
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
        {
            Camera.SetFOV(60);
            playerGameState.SwitchState(playerGameState.JumpingState);
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