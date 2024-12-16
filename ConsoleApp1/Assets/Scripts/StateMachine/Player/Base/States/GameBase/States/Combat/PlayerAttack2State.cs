using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack2State : PlayerGameBaseState
{
    double timer = 0;
    Vector2 input;
    
    bool attacked = false;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.DRAG = 2f;
        
        Console.WriteLine("Entering attack 2 state");
        
        attacked = false;
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        playerGameState.PlayerStateMachine.physicsBody.AddForce(Camera.FrontYto0() * 10);
        
        AnimationManager.Instance.SetAnimation("Sword", "attack2");
        AnimationManager.Instance.SetAnimation("Player", "attack2");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        timer += GameTime.DeltaTime;
        input = InputManager.GetMovementInput();
        
        if (InputManager.IsMousePressed(MouseButton.Left))
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