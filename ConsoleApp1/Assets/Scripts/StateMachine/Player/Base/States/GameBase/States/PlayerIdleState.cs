using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering idle state");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        Vector2 input = InputManager.GetMovementInput();

        if (input != Vector2.Zero && InputManager.IsDown(Keys.LeftAlt))
        {
            playerGameState.SwitchState(playerGameState.WalkingState);
            return;
        }
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}