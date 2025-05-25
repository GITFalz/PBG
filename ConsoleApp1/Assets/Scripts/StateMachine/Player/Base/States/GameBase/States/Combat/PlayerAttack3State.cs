using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack3State : PlayerGameBaseState
{
    Camera Camera;
    
    double timer = 0;
    Vector2 input;
    
    bool attacked = false;

    public PlayerAttack3State(PlayerGameState gameState) : base(gameState)
    {
    }

    public override void Enter()
    {
        
        Console.WriteLine("Entering attack 3 state");
        
        Camera = Game.Camera;
        
        attacked = false;
        
        OldAnimationManager.Instance.SetAnimation("Sword" , "attack3");
        OldAnimationManager.Instance.SetAnimation("Player", "attack3");
    }

    public override void Update()
    {
        timer += GameTime.DeltaTime;
        input = Input.GetMovementInput();
        
        if (Input.IsMousePressed(MouseButton.Left))
            attacked = true;
        
        if (timer > 0.3)
        {
            if (attacked)
            {
                GameState.SwitchState(GameState.Attack4State);
                return;
            }
            
            if (timer > 0.5)
            {
                if (input != Vector2.Zero && Game.MoveTest)
                {
                    GameState.SwitchState(GameState.NextMovingState);
                    return;
                }

                if (input == Vector2.Zero)
                {
                    GameState.SwitchState(GameState.IdleState);
                    return;
                }
            }
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
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
        timer = 0;
    }
}