using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public PlayerIdleState(PlayerGameState stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Console.WriteLine("Entering idle state");
        Loop("PlayerIdle", 0.5f);
    }

    public override void Update()
    {
        Vector2 input = Input.GetMovementInput();

        if (input != Vector2.Zero)
        {
            GameState.SwitchState(GameState.NextMovingState);
            return;
        }

        if (Input.IsKeyDown(Keys.Space))
        {
            GameState.SwitchState(GameState.JumpingState);
            return;
        }
    }
    
    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        SetSpeed(1.0f);
    }
}