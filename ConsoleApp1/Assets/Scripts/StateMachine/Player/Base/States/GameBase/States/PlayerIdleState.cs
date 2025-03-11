using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering idle state");
        playerGameState.PlayerStateMachine.physicsBody.Drag = 7f;
        playerGameState.NextMovingState = playerGameState.WalkingState;
    }

    public override void Update(PlayerGameState playerGameState)
    {
        Vector2 input = Input.GetMovementInput();

        if (input != Vector2.Zero && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.NextMovingState);
            return;
        }
        
        if (Input.IsKeyPressed(Keys.Space) && Game.MoveTest)
        {
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }

        if (Input.IsMousePressed(MouseButton.Left))
        {
            playerGameState.SwitchState(playerGameState.Attack1State);
            return;
        }
        
        if (!playerGameState.PlayerStateMachine.IsGrounded())
        {
            Console.WriteLine("Falling");
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
        playerGameState.PlayerStateMachine.physicsBody.Drag = 0.1f;
    }
}