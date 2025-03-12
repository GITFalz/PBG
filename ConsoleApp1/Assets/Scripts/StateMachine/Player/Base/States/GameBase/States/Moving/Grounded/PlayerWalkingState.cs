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
        playerGameState.PlayerStateMachine.physicsBody.Drag = 10f;
        playerGameState.MovementSpeed = PlayerMovementSpeed.Walk;
        
        OldAnimationManager.Instance.LoopAnimation("Player", "walking");
        Game.camera.SetFOV(45);
    }

    public override void Update(PlayerGameState playerGameState)
    {
        input = Input.GetMovementInput();
        
        if (Input.IsKeyPressed(Keys.LeftControl))
        {
            playerGameState.SwitchState(playerGameState.RunningState);
            return;
        }
        
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
        playerGameState.PlayerStateMachine.MovePlayer(PlayerMovementSpeed.Walk);
    }

    public override void Exit(PlayerGameState playerGameState)
    {
        playerGameState.PlayerStateMachine.physicsBody.Drag = 0.3f;
    }
}