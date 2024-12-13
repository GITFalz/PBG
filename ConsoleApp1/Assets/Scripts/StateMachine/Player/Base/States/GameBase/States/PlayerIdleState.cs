using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering idle state");

        playerGameState.PlayerStateMachine.Velocity.X = 0;
        playerGameState.PlayerStateMachine.Velocity.Z = 0;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        Vector2 input = InputManager.GetMovementInput();

        if (input != Vector2.Zero && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.NextMovingState);
            return;
        }
        
        if (InputManager.IsKeyPressed(Keys.Space) && Game.MoveTest)
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

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}