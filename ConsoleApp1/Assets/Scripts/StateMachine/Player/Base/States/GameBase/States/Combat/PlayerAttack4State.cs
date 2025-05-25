using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack4State : PlayerGameBaseState
{
    Camera Camera;
    
    double timer = 0;
    Vector2 input;
    
    bool smashed1 = false;
    bool smashed2 = false;
    
    public override void Enter()
    {
        Console.WriteLine("Entering attack 4 state");
        
        Camera = Game.Camera;
        
        smashed1 = false;
        smashed2 = false;
        
        OldAnimationManager.Instance.SetAnimation("Sword", "attack4");
        OldAnimationManager.Instance.SetAnimation("Player", "attack4");
    }

    public override void Update()
    {
        timer += GameTime.DeltaTime;
        input = Input.GetMovementInput();
        
        if (timer > 0.12 && !smashed1)
        {
            //GameState.PlayerStateMachine.physicsBody.SetAcceleration((0, 0.3f, 0));
            smashed1 = true;
        }

        if (timer > 0.3 && !smashed2)
        {
            //GameState.PlayerStateMachine.physicsBody.SetAcceleration((0, -0.2f, 0));
            smashed2 = true;
        }
        
        if (timer > 0.4)
        {
            if (Input.IsMousePressed(MouseButton.Left))
            {
                GameState.SwitchState(GameState.Attack1State);
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