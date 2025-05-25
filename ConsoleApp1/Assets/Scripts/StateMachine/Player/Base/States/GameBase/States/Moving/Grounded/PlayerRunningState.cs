using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerRunningState : PlayerGameBaseState
{
    Vector2 input = Vector2.Zero;

    public PlayerRunningState(PlayerGameState gameState) : base(gameState)
    {
    }

    public override void Enter()
    {
        Console.WriteLine("Entering running state");

        //StateMachine.physicsBody.Drag = 10f;
        GameState.NextMovingState = GameState.RunningState;
        GameState.MovementSpeed = PlayerMovementSpeed.Run;

        Game.Camera.SetFOV(70);
        SmoothLoop("PlayerRunning", 0.5f);
        SetSpeed(1.2f);
    }

    public override void Update()
    {
        input = Input.GetMovementInput();
        
        if (Input.IsKeyPressed(Keys.LeftControl))
        {
            GameState.SwitchState(GameState.WalkingState);
            return;
        }

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }
        
        if (input == Vector2.Zero)
        {
            GameState.SwitchState(GameState.IdleState);
            return;
        }

        if (!StateMachine.BlockSwitch)
        {
            if (Input.IsMousePressed(MouseButton.Right))
            {
                GameState.SwitchState(GameState.DashState);
                return;
            }
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
        {
            GameState.SwitchState(GameState.JumpingState);
            return;
        }

        if (!StateMachine.IsGrounded())
        {
            GameState.SwitchState(GameState.FallingState);
            return;
        }
    }
    
    public override void FixedUpdate()
    {
        StateMachine.MovePlayer(PlayerMovementSpeed.Run);
    }

    public override void Exit()
    {
        SetSpeed(1f);
    }
}