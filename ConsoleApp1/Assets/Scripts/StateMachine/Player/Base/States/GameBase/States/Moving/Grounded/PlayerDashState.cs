using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerDashState : PlayerGameBaseState
{
    private Camera Camera;
    
    float timer = 0;

    public PlayerDashState(PlayerGameState gameState) : base(gameState)
    {
        Camera = Game.Camera;
    }

    public override void Enter()
    {
        timer = 0;
        Vector3 forward = StateMachine.forward;
        StateMachine.physicsBody.AddForce(forward, PlayerStateMachine.DASH_SPEED);
        GameState.MovementSpeed = PlayerMovementSpeed.Sprint;

        Camera.SetFOV(90);
        Play("PlayerDash");
        SetSpeed(1.5f);
    }

    public override void Update()
    {
        timer += GameTime.DeltaTime;

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }
        
        if (timer > 0.5f)
        {
            if (MovementInput == Vector2.Zero)
            {
                GameState.SwitchState(GameState.IdleState);
                return;
            }
            else if (Input.IsMouseDown(MouseButton.Right))
            {
                GameState.SwitchState(GameState.SprintingState);
                return;
            }
            else
            {
                GameState.SwitchState(GameState.RunningState);
                return;
            }
        }
        
        if (Input.IsKeyDown(Keys.Space))
        {
            GameState.SwitchState(GameState.JumpingState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        if (MovementInput != Vector2.Zero)
            StateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit()
    {
        SetSpeed(1.0f);
    }
}