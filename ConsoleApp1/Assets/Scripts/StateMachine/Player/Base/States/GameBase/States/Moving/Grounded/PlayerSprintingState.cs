using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerSprintingState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector2 input = Vector2.Zero;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering sprinting state");

        //playerGameState.PlayerStateMachine.physicsBody.Drag = 10f;
        playerGameState.NextMovingState = playerGameState.SprintingState;
        playerGameState.MovementSpeed = PlayerMovementSpeed.Run;
        
        Camera = Game.Camera;
        Camera.SetFOV(80);
    }

    public override void Update(PlayerGameState playerGameState)
    { 
        input = Input.GetMovementInput();

        if (input == Vector2.Zero)
        {
            playerGameState.SwitchState(playerGameState.IdleState);
            return;
        }
        
        if (!playerGameState.PlayerStateMachine.BlockSwitch)
        {
            if (Input.IsMousePressed(MouseButton.Right))
            {
                playerGameState.SwitchState(playerGameState.DashState);
                return;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                playerGameState.SwitchState(playerGameState.Attack1State);
                return;
            }
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }

        if (!playerGameState.PlayerStateMachine.IsGrounded())
        {
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        //playerGameState.PlayerStateMachine.physicsBody.Drag = 0.3f;
    }
}