using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerWalkingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering walking state");
        
        playerGameState.NextMovingState = playerGameState.WalkingState;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        Vector2 input = InputManager.GetMovementInput();
        
        if (InputManager.IsKeyPressed(Keys.LeftControl))
        {
            playerGameState.SwitchState(playerGameState.SprintingState);
            return;
        }
        
        if (input == Vector2.Zero)
        {
            playerGameState.SwitchState(playerGameState.IdleState);
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
        
        playerGameState.MovePlayer(input, PlayerMovementSpeed.Walk);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}