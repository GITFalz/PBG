using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerDashState : PlayerGameBaseState
{
    private Camera Camera;
    
    Vector2 input;
    float timer = 0;

    public PlayerDashState(PlayerGameState gameState) : base(gameState)
    {
    }

    public override void Enter()
    {
        Console.WriteLine("Entering dash state");
        
        Camera = Game.Camera;
        timer = 0;

        Vector3 forward = StateMachine.forward;
        Console.WriteLine("Dashing with speed: " + PlayerStateMachine.DASH_SPEED + " forward: " + forward);
        StateMachine.physicsBody.AddForce(forward, PlayerStateMachine.DASH_SPEED);
        //StateMachine.physicsBody.Drag = 10f;
        GameState.MovementSpeed = PlayerMovementSpeed.Sprint;

        Camera.SetFOV(90); // 1STANNIVERSARY, EVERFLOWING, WITHYOU
    }

    public override void Update()
    {
        input = Input.GetMovementInput();
        timer += GameTime.DeltaTime;

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }
        
        if (timer > 0.5f)
        {
            if (input == Vector2.Zero)
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
        if (input != Vector2.Zero)
            StateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit()
    {
        //StateMachine.physicsBody.Drag = 0.3f;
    }
}