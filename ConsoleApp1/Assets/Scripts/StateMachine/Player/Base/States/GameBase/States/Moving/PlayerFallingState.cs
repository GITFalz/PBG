using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Camera Camera;
    Vector2 input = Vector2.Zero;
    double timer = 0;
    
    
    public override void Enter()
    {
        Console.WriteLine("Entering falling state");

        timer = 0;
        
        Camera = Game.Camera;
    }

    public override void Update()
    {
        input = Input.GetMovementInput();

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }

        if (timer > 1f)
        {
            GameState.MovementSpeed = PlayerMovementSpeed.Walk;
        }
        
        if (StateMachine.IsHuggingWall() && Input.IsKeyPressed(Keys.Space))
        {
            StateMachine.physicsBody.Velocity.Y = 0;
            GameState.SwitchState(GameState.JumpingState);
            return;
        }
        
        if (StateMachine.IsGrounded())
        {
            GameState.SwitchState(GameState.GroundedState);
            return;
        }
        
        timer += GameTime.DeltaTime;
    }
    
    public override void FixedUpdate()
    {
        if (input != Vector2.Zero)
            StateMachine.MovePlayer(PlayerMovementSpeed.Fall);
    }

    public override void Exit()
    {

    }
}