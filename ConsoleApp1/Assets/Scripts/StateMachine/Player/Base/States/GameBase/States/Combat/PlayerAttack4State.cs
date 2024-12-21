using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack4State : PlayerGameBaseState
{
    double timer = 0;
    Vector2 input;
    
    bool smashed1 = false;
    bool smashed2 = false;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.DRAG = 2f;
        
        Console.WriteLine("Entering attack 4 state");
        
        smashed1 = false;
        smashed2 = false;
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        playerGameState.PlayerStateMachine.physicsBody.AddForce(Camera.FrontYto0() * 0.1f);
        
        AnimationManager.Instance.SetAnimation("Sword", "attack4");
        AnimationManager.Instance.SetAnimation("Player", "attack4");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        timer += GameTime.DeltaTime;
        input = InputManager.GetMovementInput();
        
        if (timer > 0.12 && !smashed1)
        {
            playerGameState.PlayerStateMachine.physicsBody.AddForce((0, 0.3f, 0));
            smashed1 = true;
        }

        if (timer > 0.3 && !smashed2)
        {
            playerGameState.PlayerStateMachine.physicsBody.AddForce((0, -0.2f, 0));
            smashed2 = true;
        }
        
        if (timer > 0.4)
        {
            if (InputManager.IsMousePressed(MouseButton.Left))
            {
                playerGameState.SwitchState(playerGameState.Attack1State);
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
        
        if (InputManager.IsDown(Keys.Space) && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }
        
        playerGameState.PlayerStateMachine.MeshRotateUpdate();
    }

    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        timer = 0;
    }
}