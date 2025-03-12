using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack2State : PlayerGameBaseState
{
    Camera Camera;
    
    double timer = 0;
    Vector2 input;
    
    bool attacked = false;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        
        Console.WriteLine("Entering attack 2 state");
        
        Camera = Game.camera;
        
        attacked = false;
        
        OldAnimationManager.Instance.SetAnimation("Sword", "attack2");
        OldAnimationManager.Instance.SetAnimation("Player", "attack2");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        timer += GameTime.DeltaTime;
        input = Input.GetMovementInput();
        
        if (Input.IsMousePressed(MouseButton.Left))
            attacked = true;
        
        if (timer > 0.3)
        {
            if (attacked)
            {
                playerGameState.SwitchState(playerGameState.Attack3State);
                return;
            }
            
            if (timer > 0.5)
            {
                if (input != Vector2.Zero && Game.MoveTest)
                {
                    playerGameState.SwitchState(playerGameState.NextMovingState);
                    return;
                }

                if (input == Vector2.Zero)
                {
                    playerGameState.SwitchState(playerGameState.IdleState);
                    return;
                }
            }
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }
    }

    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        timer = 0;
    }
}