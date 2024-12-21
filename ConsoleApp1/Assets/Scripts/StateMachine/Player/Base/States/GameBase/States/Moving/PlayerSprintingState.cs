using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerSprintingState : PlayerGameBaseState
{
    Vector2 input = Vector2.Zero;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering sprinting state");
        
        playerGameState.NextMovingState = playerGameState.SprintingState;
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
        
        AnimationManager.Instance.LoopAnimation("Player", "running");
        
        Camera.SetFOV(55);
    }

    public override void Update(PlayerGameState playerGameState)
    { 
        input = InputManager.GetMovementInput();
        
        if (InputManager.IsKeyPressed(Keys.LeftControl))
        {
            playerGameState.SwitchState(playerGameState.WalkingState);
            return;
        }
        
        if (input == Vector2.Zero)
        {
            playerGameState.SwitchState(playerGameState.IdleState);
            return;
        }
        
        if (InputManager.IsMousePressed(MouseButton.Right))
        {
            playerGameState.SwitchState(playerGameState.DashState);
            return;
        }
        
        if (InputManager.IsMousePressed(MouseButton.Left))
        {
            playerGameState.SwitchState(playerGameState.Attack1State);
            return;
        }
        
        if (InputManager.IsDown(Keys.Space) && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }

        if (!playerGameState.PlayerStateMachine.IsGrounded())
        {
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
        
        playerGameState.PlayerStateMachine.MeshRotateUpdate();
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Sprint);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        Camera.SetFOV(45);
    }
}