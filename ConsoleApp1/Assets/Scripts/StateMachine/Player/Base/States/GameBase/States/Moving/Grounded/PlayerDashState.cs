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
        
        Camera = Game.Camera;
        timer = 0;

        Vector3 forward = playerGameState.PlayerStateMachine.forward;
        Console.WriteLine("Dashing with speed: " + PlayerStateMachine.DASH_SPEED + " forward: " + forward);
        playerGameState.PlayerStateMachine.physicsBody.AddForce(forward, PlayerStateMachine.DASH_SPEED);
        //playerGameState.PlayerStateMachine.physicsBody.Drag = 10f;
        playerGameState.MovementSpeed = PlayerMovementSpeed.Sprint;

        Camera.SetFOV(90); // 1STANNIVERSARY, EVERFLOWING, WITHYOU
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();
        timer += GameTime.DeltaTime;
        
        if (timer > 0.5f)
        {
            if (input == Vector2.Zero)
            {
                playerGameState.SwitchState(playerGameState.IdleState);
                return;
            }
            else if (Input.IsMouseDown(MouseButton.Right))
            {
                playerGameState.SwitchState(playerGameState.SprintingState);
                return;
            }
            else
            {
                playerGameState.SwitchState(playerGameState.RunningState);
                return;
            }
        }
        
        if (Input.IsKeyDown(Keys.Space))
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }
    }

    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        if (input != Vector2.Zero)
            playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        //playerGameState.PlayerStateMachine.physicsBody.Drag = 0.3f;
    }
}