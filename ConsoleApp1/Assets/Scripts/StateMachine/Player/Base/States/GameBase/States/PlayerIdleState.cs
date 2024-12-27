using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering idle state");

        AnimationManager.Instance.LoopAnimation("Player", "idle");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        Vector2 input = InputManager.GetMovementInput();
        
        playerGameState.PlayerStateMachine.physicsBody.Velocity = Vector3.Zero;

        if (input != Vector2.Zero && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.NextMovingState);
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
        
        //playerGameState.PlayerStateMachine.MoveMeshUpdate();
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {

    }

    public override void Exit(PlayerGameState playerGameState)
    {
        
    }
}