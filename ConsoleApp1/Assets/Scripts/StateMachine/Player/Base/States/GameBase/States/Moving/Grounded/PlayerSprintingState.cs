using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerSprintingState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector2 input = Vector2.Zero;
    
    public override void Enter()
    {
        Console.WriteLine("Entering sprinting state");

        //StateMachine.physicsBody.Drag = 10f;
        GameState.NextMovingState = GameState.SprintingState;
        GameState.MovementSpeed = PlayerMovementSpeed.Run;
        
        Camera = Game.Camera;
        Camera.SetFOV(80);
    }

    public override void Update()
    { 
        input = Input.GetMovementInput();

        if (input == Vector2.Zero)
        {
            GameState.SwitchState(GameState.IdleState);
            return;
        }

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
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
        StateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit()
    {
        //StateMachine.physicsBody.Drag = 0.3f;
    }
}