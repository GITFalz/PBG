using OpenTK.Mathematics;

public class PlayerGroundedState : PlayerGameBaseState
{
    private float _timer = 0;
    
    public PlayerGroundedState(PlayerGameState gameState) : base(gameState) { }
    public override void Enter()
    {
        Console.WriteLine("Entering grounded state");
        _timer = 1f;
        SmoothPlayFinish("PlayerLand", 0.5f);
    }

    public override void Update()
    {
        Vector2 input = Input.GetMovementInput();
        
        if (_timer <= 0.5f)
        {
            GameState.SwitchState(GameState.IdleState);
            return;
        }

        if (input != Vector2.Zero)
        {
            Stop();
            GameState.SwitchState(GameState.NextMovingState);
            return;
        }

        _timer -= GameTime.DeltaTime;
    }
    
    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {

    }
}