using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerWalkingState : PlayerGameBaseState
{
    Vector2 input = Vector2.Zero;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering walking state");
        
        playerGameState.NextMovingState = playerGameState.WalkingState;
        
        AnimationManager.Instance.LoopAnimation("Player", "walking");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();
        
        if (Input.IsKeyPressed(Keys.LeftControl))
        {
            playerGameState.SwitchState(playerGameState.SprintingState);
            return;
        }
        
        if (input == Vector2.Zero)
        {
            playerGameState.SwitchState(playerGameState.IdleState);
            return;
        }
        
        if (Input.IsMousePressed(MouseButton.Left))
        {
            playerGameState.SwitchState(playerGameState.Attack1State);
            return;
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

        playerGameState.PlayerStateMachine.MeshRotateUpdate();
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Walk);
    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}