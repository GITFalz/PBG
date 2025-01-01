using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerAttack3State : PlayerGameBaseState
{
    Camera Camera;
    
    double timer = 0;
    Vector2 input;
    
    bool attacked = false;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        
        Console.WriteLine("Entering attack 3 state");
        
        Camera = playerGameState.PlayerStateMachine.camera;
        
        attacked = false;
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        playerGameState.PlayerStateMachine.physicsBody.AddForce(Camera.FrontYto0() * 0.2f);
        
        AnimationManager.Instance.SetAnimation("Sword" , "attack3");
        AnimationManager.Instance.SetAnimation("Player", "attack3");
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
                playerGameState.SwitchState(playerGameState.Attack4State);
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