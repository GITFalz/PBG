using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerFallingState : PlayerGameBaseState
{
    Camera Camera;
    Vector2 input = Vector2.Zero;
    double timer = 0;
    bool _animate = true;

    public PlayerFallingState(PlayerGameState gameState) : base(gameState)
    {
        Camera = Game.Camera;
    }
    
    public override void Enter()
    {
        timer = 0;
        _animate = true;
        Console.WriteLine("Entering falling state");
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

        if (timer > 0.5f && _animate)
        {
            Loop("PlayerFall", 0.5f);
            _animate = false;
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
        SetSpeed(1f);
    }
}